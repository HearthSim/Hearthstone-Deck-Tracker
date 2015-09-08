#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader.Handlers;

#endregion

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
		private static readonly SortedList<DateTime, List<LogLineItem>> ToProcess = new SortedList<DateTime, List<LogLineItem>>();
		private static bool _stop;

		private static void InitializeLogReaders()
		{
			_powerLogReader = new LogReader(LogReaderNamespaces.PowerLogReaderInfo);
			_bobLogReader = new LogReader(LogReaderNamespaces.BobLogReaderInfo);
			LogReaders.Add(_powerLogReader);
			LogReaders.Add(_bobLogReader);
			LogReaders.Add(new LogReader(LogReaderNamespaces.ZoneLogReaderInfo));
			LogReaders.Add(new LogReader(LogReaderNamespaces.RachelleLogReaderInfo));
			LogReaders.Add(new LogReader(LogReaderNamespaces.AssetLogReaderInfo));
			LogReaders.Add(new LogReader(LogReaderNamespaces.ArenaLogReaderInfo));
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
				await Task.Factory.StartNew(() =>
				{
					foreach(var logReader in LogReaders)
					{
						var lines = logReader.Collect();
						foreach(var line in lines)
						{
							List<LogLineItem> logLines;
							if(!ToProcess.TryGetValue(line.Time, out logLines))
								ToProcess.Add(line.Time, logLines = new List<LogLineItem>());
							logLines.Add(line);
						}
					}
				});
				ProcessNewLines();
				await Task.Delay(Config.Instance.UpdateDelay);
			}
			_running = false;
		}

		private static DateTime GetStartingPoint()
		{
			var powerEntry = _powerLogReader.FindEntryPoint("GameState.DebugPrintPower() - CREATE_GAME");
			var bobEntry = _bobLogReader.FindEntryPoint("legend rank");
			return powerEntry > bobEntry ? powerEntry : bobEntry;
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

		public static async Task Restart()
		{
			if(!_running)
				return;
			await Stop();
			_startingPoint = GetStartingPoint();
			_gameState.Reset();
			StartLogReaders();
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
			var updateInterface = false;
			foreach(var item in ToProcess)
			{
				foreach(var line in item.Value)
				{
					switch(line.Namespace)
					{
						case "Power":
							PowerGameStateLineHandler.Handle(line.Line, _gameState, _game);
							updateInterface = true;
							break;
						case "Zone":
							ZoneHandler.Handle(line.Line, _gameState);
							break;
						case "Asset":
							AssetHandler.Handle(line.Line, _gameState, _game);
							break;
						case "Bob":
							BobHandler.Handle(line.Line, _gameState, _game);
							break;
						case "Rachelle":
							RachelleHandler.Handle(line.Line, _gameState, _game);
							break;
						case "Arena":
							ArenaHandler.Handle(line.Line, _gameState, _game);
							break;
					}
				}
			}
			ToProcess.Clear();
			//if(updateInterface)
				Helper.UpdateEverything(_game);
		}
	}
}