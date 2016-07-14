#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public class LogReader
	{
		private readonly string _filePath;
		internal readonly LogReaderInfo Info;
		private ConcurrentQueue<LogLineItem> _lines = new ConcurrentQueue<LogLineItem>();
		private long _offset;
		private bool _running;
		private DateTime _startingPoint;
		private bool _logFileExists;


		private bool _stop;
		private Thread _thread;

		public LogReader(LogReaderInfo info)
		{
			Info = info;
			_filePath = string.IsNullOrEmpty(info.FilePath)
				            ? Path.Combine(Config.Instance.HearthstoneDirectory, Config.Instance.HearthstoneLogsDirectoryName, $"{Info.Name}.log") : info.FilePath;
		}

		public void Start(DateTime startingPoint)
		{
			Log.Debug("Starting " + Info.Name);
			if (_running)
			{
				Log.Debug(Info.Name + " is already running.");
				return;
			}
			MoveOrDeleteLogFile();
			_startingPoint = startingPoint;
			_stop = false;
			_offset = 0;
			_logFileExists = false;
			_thread = new Thread(ReadLogFile) {IsBackground = true};
			_thread.Start();
		}

		private void MoveOrDeleteLogFile()
		{
			if(File.Exists(_filePath))
			{
				try
				{
					//check if we can move it
					File.Move(_filePath, _filePath);
					var old = _filePath.Replace(".log", "_old.log");
					if(File.Exists(old))
					{
						try
						{
							File.Delete(old);
						}
						catch
						{
						}
					}
					File.Move(_filePath, old);
				}
				catch
				{
					try
					{
						File.Delete(_filePath);
					}
					catch
					{
					}
				}
			}
		}

		public async Task Stop()
		{
			Log.Debug("Stopping " + Info.Name);
			_stop = true;
			while(_running || _thread == null || _thread.ThreadState == ThreadState.Unstarted)
				await Task.Delay(50);
			_lines = new ConcurrentQueue<LogLineItem>();
			await Task.Factory.StartNew(() => _thread?.Join());
			Log.Debug(Info.Name + " stopped.");
		}

		public IEnumerable<LogLineItem> Collect()
		{
			var count = _lines.Count;
			for(var i = 0; i < count; i++)
			{
				LogLineItem line;
				if(_lines.TryDequeue(out line))
					yield return line;
			}
		}

		private void ReadLogFile()
		{
			_running = true;
			FindInitialOffset();
			while(!_stop)
			{
				var fileInfo = new FileInfo(_filePath);
				if(fileInfo.Exists)
				{
					if(!_logFileExists)
					{
						_logFileExists = true;
						Log.Info($"Found {Info.Name}.log.");
					}
					using(var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						fs.Seek(_offset, SeekOrigin.Begin);
						if(fs.Length == _offset)
						{
							Thread.Sleep(LogReaderManager.UpdateDelay);
							continue;
						}
						using(var sr = new StreamReader(fs))
						{
							string line;
							while(!sr.EndOfStream && (line = sr.ReadLine()) != null)
							{
								if(line.StartsWith("D "))
								{
									if(!sr.EndOfStream && sr.Peek() != 'D')
										break;
									if(!Info.HasFilters || (Info.StartsWithFilters?.Any(x => line.Substring(19).StartsWith(x)) ?? false)
										|| (Info.ContainsFilters?.Any(x => line.Substring(19).Contains(x)) ?? false))
									{
										var logLine = new LogLineItem(Info.Name, line);
										if(logLine.Time >= _startingPoint)
											_lines.Enqueue(logLine);
									}
								}
								else
									Log.Warn("Ignored line: " + line);
								_offset += Encoding.UTF8.GetByteCount(line + Environment.NewLine);
							}
						}
					}
				}
				Thread.Sleep(LogReaderManager.UpdateDelay);
			}
			_running = false;
		}

		private void FindInitialOffset()
		{
			var fileInfo = new FileInfo(_filePath);
			if(fileInfo.Exists)
			{
				using(var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using(var sr = new StreamReader(fs, Encoding.ASCII))
				{
					var offset = 0;
					while(offset < fs.Length)
					{
						var sizeDiff = 4096 - Math.Min(fs.Length - offset, 4096);
						offset += 4096;
						var buffer = new char[4096];
						fs.Seek(Math.Max(fs.Length - offset, 0), SeekOrigin.Begin);
						sr.ReadBlock(buffer, 0, 4096);
						var skip = 0;
						for(var i = 0; i < 4096; i++)
						{
							skip++;
							if(buffer[i] == '\n')
								break;
						}
						offset -= skip;
						var lines = (new string(buffer.Skip(skip).ToArray())).Split(new[] {Environment.NewLine}, StringSplitOptions.None).ToArray();
						for(int i = lines.Length - 1; i > 0; i--)
						{
							if(string.IsNullOrWhiteSpace(lines[i].Trim('\0')))
								continue;
							var logLine = new LogLineItem(Info.Name, lines[i]);
							if(logLine.Time < _startingPoint)
							{
								var negativeOffset = lines.Take(i + 1).Sum(x => Encoding.UTF8.GetByteCount(x + Environment.NewLine));
								_offset = Math.Max(fs.Length - offset + negativeOffset + sizeDiff, 0);
								return;
							}
						}
					}
				}
			}
			_offset = 0;
		}

		public DateTime FindEntryPoint(string str) => FindEntryPoint(new[] {str});

		public DateTime FindEntryPoint(string[] str)
		{
			var fileInfo = new FileInfo(_filePath);
			if(fileInfo.Exists)
			{
				var targets = str.Select(x => new string(x.Reverse().ToArray())).ToList();
				using(var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using(var sr = new StreamReader(fs, Encoding.ASCII))
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
							skip++;
							if(buffer[i] == '\n')
								break;
						}
						if(skip >= 4096)
							continue;
						offset -= skip;
						var reverse = new string(buffer.Skip(skip).Reverse().ToArray());
						var targetOffsets = targets.Select(x => reverse.IndexOf(x, StringComparison.Ordinal)).Where(x => x > -1).ToList();
						var targetOffset = targetOffsets.Any() ? targetOffsets.Min() : -1;
						if(targetOffset != -1)
						{
							var line = new string(reverse.Substring(targetOffset).TakeWhile(c => c != '\n').Reverse().ToArray());
							return new LogLineItem("", line).Time;
						}
					}
				}
			}
			return DateTime.MinValue;
		}
	}
}