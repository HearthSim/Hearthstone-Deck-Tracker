using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.UI.WebControls.WebParts;
using System.Windows.Ink;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Handlers;

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class LogReaderManager
	{
		private static readonly PowerGameStateHandler PowerGameStateLineHandler = new PowerGameStateHandler();
		private static readonly PowerHandler PowerLineHandler = new PowerHandler();
		private static readonly RachelleHandler RachelleHandler = new RachelleHandler();
		private static readonly AssetHandler AssetHandler = new AssetHandler();
		private static readonly ZoneHandler ZoneHandler = new ZoneHandler();
		private static readonly BobHandler BobHandler = new BobHandler();
		private static readonly ArenaHandler ArenaHandler = new ArenaHandler();

		private static LogReader _powerLogReader;
		private static LogReader _bobLogReader;
		private static readonly List<LogReader> LogReaders = new List<LogReader>(); 
		private static bool _running;

		private static HsGameState _gameState;
		private static GameV2 _game;

		private static DateTime _startingPoint;

		private static void InitializeLogReaders()
		{
			_powerLogReader = new LogReader(Hearthstone_Deck_Tracker.LogReader.LogReaders.PowerLogReaderInfo);
			_bobLogReader = new LogReader(Hearthstone_Deck_Tracker.LogReader.LogReaders.BobLogReaderInfo);
            LogReaders.Add(_powerLogReader);
			LogReaders.Add(_bobLogReader);
			LogReaders.Add(new LogReader(Hearthstone_Deck_Tracker.LogReader.LogReaders.ZoneLogReaderInfo));
			LogReaders.Add(new LogReader(Hearthstone_Deck_Tracker.LogReader.LogReaders.RachelleLogReaderInfo));
			LogReaders.Add(new LogReader(Hearthstone_Deck_Tracker.LogReader.LogReaders.AssetLogReaderInfo));
			LogReaders.Add(new LogReader(Hearthstone_Deck_Tracker.LogReader.LogReaders.ArenaLogReaderInfo));
		}


		public static void Start(GameV2 game)
		{
			InitializeGameState(game);
			InitializeLogReaders();
			_startingPoint = GetStartingPoint();
			StartLogReaders();
		}

		private static async void StartLogReaders()
		{
			foreach(var logReader in LogReaders)
				logReader.Start(_startingPoint);
			_running = true;
			_stop = false;
			while(!_stop)
			{
				if(LogReaders.Any(ls => ls.NeedsReset))
				{
					_stop = true;
					break;
				}
				await Task.Factory.StartNew(() =>
				{
					foreach(var logReader in LogReaders)
					{
						var lines = logReader.Collect();
						foreach(var line in lines)
							ToProcess.Add(line.Time, line);
					}
				});
				ProcessNewLines();
				await Task.Delay(Config.Instance.UpdateDelay);
			}
			_running = false;
			if(LogReaders.Any(ls => ls.NeedsReset))
			{
				await Stop();
				_game.Reset();
				_gameState.Reset();
				_startingPoint = GetStartingPoint();
				StartLogReaders();
			}
		}

		private static DateTime GetStartingPoint()
		{
			var powerEntry = _powerLogReader.FindEntryPoint("GameState.DebugPrintPower() - CREATE_GAME");
			var bobEntry = _bobLogReader.FindEntryPoint("legend rank");
			var bobEntry2 = _bobLogReader.FindEntryPoint("NetCache <<<GET NetCacheFeatures");
			var bob = bobEntry > bobEntry2 ? bobEntry : bobEntry2;
			return powerEntry > bob ? powerEntry : bob;
		}


		public static int GetTurnNumber()
		{
			return _gameState.GetTurnNumber();
		}

		public static async Task<bool> RankedDetection(int timeoutInSeconds = 3)
		{
			Logger.WriteLine("waiting for ranked detection", "LogReader");
			_gameState.AwaitingRankedDetection = true;
			_gameState.WaitingForFirstAssetUnload = true;
			_gameState.FoundRanked = false;
			_gameState.LastAssetUnload = DateTime.Now;
			var timeout = TimeSpan.FromSeconds(timeoutInSeconds);
			while(_gameState.WaitingForFirstAssetUnload || (DateTime.Now - _gameState.LastAssetUnload) < timeout)
			{
				await Task.Delay(100);
				if(_gameState.FoundRanked)
					break;
			}
			return _gameState.FoundRanked;
		}

		public static async Task Stop()
		{
			if(!_running)
				return;
			_stop = true;
			while(_running)
				await Task.Delay(50);
			await Task.WhenAll(LogReaders.Select(x => x.Stop()));
		}

		public static async Task Restart(bool resetStartingPoint)
		{
			if(!_running)
				return;
			if(resetStartingPoint)
			{
				await Stop();
				_startingPoint = GetStartingPoint();
				StartLogReaders();
			}
			_gameState.Reset();
		}
		private static void InitializeGameState(GameV2 game)
		{
			_game = game;
			_gameState = new HsGameState(game);
			_gameState.GameHandler = new GameEventHandler(game);
			_gameState.GameHandler.ResetConstructedImporting();
			_gameState.Reset();
		}

		private static void ProcessNewLines()
		{
				foreach(var item in ToProcess)
				{
					switch(item.Value.Namespace)
					{
						case "Power":
							PowerGameStateLineHandler.Handle(item.Value.Line, _gameState, _game);
							break;
						case "Zone":
							ZoneHandler.Handle(item.Value.Line, _gameState);
							break;
						case "Asset":
							AssetHandler.Handle(item.Value.Line, _gameState, _game);
							break;
						case "Bob":
							BobHandler.Handle(item.Value.Line, _gameState, _game);
							break;
						case "Rachelle":
							RachelleHandler.Handle(item.Value.Line, _gameState, _game);
							break;
						case "Arena":
							ArenaHandler.Handle(item.Value.Line, _gameState, _game);
							break;
					}
				}
				ToProcess.Clear();
		}

		private static readonly SortedList<DateTime, LogLineItem> ToProcess = new SortedList<DateTime, LogLineItem>(new DuplicateKeyComparer<DateTime>());
		private static bool _stop;
	}

	public class DuplicateKeyComparer<TKey> : IComparer<TKey> where TKey : IComparable
	{
		public int Compare(TKey x, TKey y)
		{
			var result = x.CompareTo(y);
			return result == 0 ? 1 : result;
		}
	}

	public class LogReader
	{
		private readonly string _filePath;
		private readonly LogReaderInfo _info;
		private Thread _thread;
		private long _offset;
		private readonly List<LogLineItem> _lines = new List<LogLineItem>();
		private readonly object _sync = new object();

		public LogReader(LogReaderInfo info)
		{
			_info = info;
			_filePath = Path.Combine(Config.Instance.HearthstoneDirectory, string.Format("Logs/{0}.log", _info.Name));
		}


		private bool _stop;
		private bool _running;
		private bool _collected;
		private DateTime _startingPoint;
		public bool NeedsReset { get; private set; }

		public void Start(DateTime startingPoint)
		{
			NeedsReset = false;
			_startingPoint = startingPoint;
			_stop = false;
			_offset = 0;
			_thread = new Thread(ReadLogFile) { IsBackground = true };
			_thread.Start();
		}

		public async Task Stop()
		{
			_stop = true;
			while(_running)
				await Task.Delay(50);
			await Task.Factory.StartNew(() => _thread.Join());
		}

		public List<LogLineItem> Collect() 
		{
			lock(_sync)
			{
				var lastLine = _lines.LastOrDefault();
				if(lastLine != null && (DateTime.Now - lastLine.Time) < TimeSpan.FromSeconds(300))
				{
					_collected = true;
					return _lines.ToList();
				}
				return new List<LogLineItem>();
			}
		}

		private void ReadLogFile()
		{
			_running = true;
			while(!_stop)
			{
				lock(_sync)
				{
					if(_collected)
					{
						_lines.Clear();
						_collected = false;
					}
					var fileInfo = new FileInfo(_filePath);
					if(fileInfo.Exists)
					{
						using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
						{
							fs.Seek(_offset, SeekOrigin.Begin);
							if(fs.Length == _offset)
							{
								Thread.Sleep(Config.Instance.UpdateDelay);
								continue;
							}
							if(_offset > fs.Length)
							{
								NeedsReset = true;
								break;
							}
							using (var sr = new StreamReader(fs))
							{
								string line;
								while(!sr.EndOfStream && (line = sr.ReadLine()) != null)
								{
									if(!line.StartsWith("D ") || (!sr.EndOfStream && sr.Peek() != 'D'))
										break;
									if(!_info.HasFilters || _info.StartsWithFilters.Any(x => line.Substring(19).StartsWith(x))
									   || _info.ContainsFilters.Any(x => line.Substring(19).Contains(x)))
									{
										var logLine = new LogLineItem(_info.Name, line, fileInfo.LastWriteTime);
										if(logLine.Time >= _startingPoint)
											_lines.Add(logLine);
									}
									_offset += line.Length + Environment.NewLine.Length;
								}
							}
						}
					}
				}
				Thread.Sleep(Config.Instance.UpdateDelay);
			}
			_running = false;
		}

		public DateTime FindEntryPoint(string str)
		{
			var fileInfo = new FileInfo(_filePath);
			if(fileInfo.Exists)
			{
				var target = new string(str.Reverse().ToArray());
				using (var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					using(var sr = new StreamReader(fs))
					{
						var offset = 0;
						while(offset < fs.Length)
						{
							offset += 4096;
							var buffer = new char[4096];
							fs.Seek(Math.Max(fs.Length - offset, 0), SeekOrigin.Begin);
							sr.ReadBlock(buffer, 0, 4096);
							var skip = 0;
							for(var i = 0; i < 4096; i++)
							{
								if(buffer[i] == '\n')
									break;
								skip++;
							}
							offset -= skip;
							var reverse = new string(buffer.Skip(skip).Reverse().ToArray());
							var gameStartOffset = reverse.IndexOf(target, StringComparison.Ordinal);
							if(gameStartOffset != -1)
							{
								var line = new string(reverse.Substring(gameStartOffset).TakeWhile(c => c != '\n').Reverse().ToArray());
								return new LogLineItem("", line, fileInfo.LastWriteTime).Time;
							}
						}

					}
				}
			}
			return DateTime.MinValue;
		}
	}

	public class LogLineItem
	{
		public string Namespace { get; set; }
		public DateTime Time { get; private set; }
		public string Line { get; set; }

		public LogLineItem(string ns, string line, DateTime date)
		{
			Namespace = ns;
			Line = line;
			DateTime time;
			Time = DateTime.TryParse(Line.Substring(2, 16), out time) ? date.Date.Add(time.TimeOfDay) : date;
			if(Time > DateTime.Now)
				Time = Time.AddDays(-1);
		}
	}

	public class LogReaders
	{
		public static LogReaderInfo PowerLogReaderInfo
		{
			get { return new LogReaderInfo { Name = "Power", StartsWithFilters = new[] { "GameState." }, ContainsFilters = new[] { "Begin Spectating", "Start Spectator", "End Spectator" } }; }
		}
		public static LogReaderInfo AssetLogReaderInfo
		{
			get { return new LogReaderInfo { Name = "Asset" }; }
		}
		public static LogReaderInfo BobLogReaderInfo
		{
			get { return new LogReaderInfo { Name = "Bob" }; }
		}
		public static LogReaderInfo RachelleLogReaderInfo
		{
			get { return new LogReaderInfo { Name = "Rachelle" }; }
		}
		public static LogReaderInfo ZoneLogReaderInfo
		{
			get { return new LogReaderInfo {Name = "Zone", ContainsFilters = new[] {"zone from"}}; }
		}
		public static LogReaderInfo ArenaLogReaderInfo
		{
			get { return new LogReaderInfo { Name = "Arena" }; }
		}
	}

	public class LogReaderInfo
	{
		public string Name { get; set; }

		public bool HasFilters
		{
			get { return StartsWithFilters != null && ContainsFilters != null; }
		}
		public string[] StartsWithFilters { get; set; }
		public string[] ContainsFilters { get; set; }
	}
}
