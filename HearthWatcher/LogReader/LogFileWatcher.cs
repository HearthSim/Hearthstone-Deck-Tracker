#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace HearthWatcher.LogReader
{
	public class LogFileWatcher
	{
		internal readonly LogWatcherInfo Info;
		private string _logDir;
		private ConcurrentQueue<LogLine> _lines = new ConcurrentQueue<LogLine>();
		private bool _logFileExists;
		private long _offset;
		private bool _running;
		private DateTime _startingPoint;
		private bool _stop;
		private Thread _thread;

		/**
		 * Limit the amount of LogLines we keep in the ConcurrentQueue, so that we don't run out of memory.
		 * If we hit the limit we temporarily stop advancing through the log file until the queue has room again.
		 * This limit will usually only be hit in very late Battlegrounds matches, especially when restarting HDT or
		 * when a combat has excessively many actions.
		 */
		private const int MAX_LOG_LINE_BUFFER = 100_000;

		private DirectoryInfo _latestActiveDir;
		private DirectoryInfo _latestInactiveDir;
		private DateTime _lastCheck;

		private DirectoryInfo GetActualLogDir()
		{
			var now = DateTime.Now;
			if((now - _lastCheck).TotalSeconds < 5)
				return _latestActiveDir;
			_lastCheck = now;

			DirectoryInfo latest;
			try
			{
				var subDirs = new DirectoryInfo(_logDir).GetDirectories();
				if(subDirs.Length == 0)
					return null;

				latest = subDirs.OrderByDescending(x => x.CreationTime).First();
				if(latest.FullName == _latestInactiveDir?.FullName)
					return null;
			}
			catch
			{
				return null;
			}

			try
			{
				var file = latest.GetFiles().FirstOrDefault();
				if(file == null)
					return null;

				file.MoveTo(file.FullName);
			}
			catch(IOException)
			{
				// We're not able te move the file. This is the active directory.
				_latestActiveDir = latest;
				return latest;
			}
			catch(Exception)
			{
			}

			_latestInactiveDir = latest;
			return null;
		}

		private string GetFilePath()
		{
			var dir = GetActualLogDir();
			if(dir == null)
			{
				// Does not actually exist. We're using this as a fallback
				return Path.Combine(_logDir, Info.Name + ".log");
			}
			return Path.Combine(dir.FullName, Info.Name + ".log");
		}


		public LogFileWatcher(LogWatcherInfo info)
		{
			Info = info;
		}

		public event Action<string> OnLogFileFound;
		public event Action<string> OnLogLineIgnored;

		public void Start(DateTime startingPoint, string logDirectory)
		{
			if(_running)
				return;
			_logDir = logDirectory;
			MoveOrDeleteLogFile();
			_startingPoint = startingPoint;
			_stop = false;
			_offset = 0;
			_logFileExists = false;
			_thread = new Thread(ReadLogFile) { IsBackground = true };
			_thread.Start();
		}

		private void MoveOrDeleteLogFile()
		{
			var filePath = GetFilePath();
			if(File.Exists(filePath))
			{
				try
				{
					//check if we can move it
					File.Move(filePath, filePath);
					var old = filePath.Replace(".log", "_old.log");
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

					File.Move(filePath, old);
				}
				catch
				{
					try
					{
						File.Delete(filePath);
					}
					catch
					{
						try
						{
							/*
							 * The Overwolf log file reader appears to keep files open in read mode even while
							 * Hearthstone is not running. Since only Hearthstone creates a write lock on the file
							 * we can reset the content instead of moving or deleting it.
							 */
							File.WriteAllText(filePath, string.Empty);
						}
						catch
						{
						}
					}
				}
			}
		}

		public async Task Stop()
		{
			_stop = true;
			while(_running || _thread == null || _thread.ThreadState == ThreadState.Unstarted)
				await Task.Delay(50);
			_lines = new ConcurrentQueue<LogLine>();
			await Task.Factory.StartNew(() => _thread?.Join());
		}

		public IEnumerable<LogLine> Collect()
		{
			var count = _lines.Count;
			for(var i = 0; i < count; i++)
			{
				LogLine line;
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
				var filePath = GetFilePath();
				var fileInfo = new FileInfo(filePath);
				if(fileInfo.Exists)
				{
					if(!_logFileExists)
					{
						_logFileExists = true;
						OnLogFileFound?.Invoke(Info.Name);
					}

					FileStream? fs = null;
					try
					{
						fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
						fs.Seek(_offset, SeekOrigin.Begin);
						if(fs.Length == _offset)
						{
							Thread.Sleep(LogWatcher.UpdateDelay);
							continue;
						}
						using(var sr = new StreamReader(fs))
						{
							string line;
							var sb = new StringBuilder();
							while(!sr.EndOfStream && _lines.Count < MAX_LOG_LINE_BUFFER)
							{
								sb.Clear();
								var foundEndOfLine = false;
								var prevChar = '\0';
								while(!sr.EndOfStream)
								{
									var c = (char)sr.Read();
									if(c == '\n' && prevChar == '\r')
									{
										foundEndOfLine = true;
										break;
									}
									sb.Append(c);
									prevChar = c;
								}
								if(!foundEndOfLine) break;

								line = sb.ToString(0, sb.Length - 1);

								if(line.StartsWith("D "))
								{
									var next = sr.Peek();
									if(!sr.EndOfStream && !(next == 'D' || next == 'W' || next == 'E'))
										break;
									var logLine = new LogLine(Info.Name, line);
									if((!Info.HasFilters || (Info.StartsWithFilters?.Any(x => logLine.LineContent.StartsWith(x)) ?? false)
										|| (Info.ContainsFilters?.Any(x => logLine.LineContent.Contains(x)) ?? false))
										&& logLine.Time >= _startingPoint)
										_lines.Enqueue(logLine);
								}
								else
									OnLogLineIgnored?.Invoke($"{Info.Name}: {line}");
								_offset += Encoding.UTF8.GetByteCount(line + Environment.NewLine);
							}
						}
					}
					catch {
						// some kind of error - maybe the log file has disappeared?
						// stick around until somebody calls Stop()
						Thread.Sleep(LogWatcher.UpdateDelay);
						continue;
					}
					finally
					{
						if(fs is IDisposable dispose)
							dispose.Dispose();
					}
				}
				Thread.Sleep(LogWatcher.UpdateDelay);
			}
			_running = false;
		}

		private void FindInitialOffset()
		{
			var filePath = GetFilePath();
			var fileInfo = new FileInfo(filePath);
			if(fileInfo.Exists)
			{
				using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
							if(i > 0 && buffer[i - 1] == '\r' && buffer[i] == '\n')
								break;
						}
						offset -= skip;
						var lines =
							new string(buffer.Skip(skip).ToArray()).Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToArray();
						for(var i = lines.Length - 1; i > 0; i--)
						{
							if(string.IsNullOrWhiteSpace(lines[i].Trim('\0')))
								continue;
							var logLine = new LogLine(Info.Name, lines[i]);
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

		public DateTime FindEntryPoint(string logDirectory, string str) => FindEntryPoint(logDirectory, new[] { str });

		public DateTime FindEntryPoint(string logDirectory, string[] str)
		{
			_logDir = logDirectory;
			var filePath = GetFilePath();
			var fileInfo = new FileInfo(filePath);
			if(fileInfo.Exists)
			{
				var targets = str.Select(x => new string(x.Reverse().ToArray())).ToList();
				using(var fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
							if(i > 0 && buffer[i - 1] == '\r' && buffer[i] == '\n')
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
							return new LogLine("", line).Time;
						}
					}
				}
			}
			return DateTime.MinValue;
		}
	}
}
