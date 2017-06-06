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
		private string _filePath;
		private ConcurrentQueue<LogLine> _lines = new ConcurrentQueue<LogLine>();
		private bool _logFileExists;
		private long _offset;
		private bool _running;
		private DateTime _startingPoint;
		private bool _stop;
		private Thread _thread;

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
			_filePath = Path.Combine(logDirectory, Info.Name + ".log");
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
				var fileInfo = new FileInfo(_filePath);
				if(fileInfo.Exists)
				{
					if(!_logFileExists)
					{
						_logFileExists = true;
						OnLogFileFound?.Invoke(Info.Name);
					}
					using(var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						fs.Seek(_offset, SeekOrigin.Begin);
						if(fs.Length == _offset)
						{
							Thread.Sleep(LogWatcher.UpdateDelay);
							continue;
						}
						using(var sr = new StreamReader(fs))
						{
							string line;
							while(!sr.EndOfStream && (line = sr.ReadLine()) != null)
							{
								if(line.StartsWith("D "))
								{
									var next = sr.Peek();
									if(!sr.EndOfStream && !(next == 'D' || next == 'W'))
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
				}
				Thread.Sleep(LogWatcher.UpdateDelay);
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
			var fileInfo = new FileInfo(Path.Combine(logDirectory, Info.Name + ".log"));
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
							return new LogLine("", line).Time;
						}
					}
				}
			}
			return DateTime.MinValue;
		}
	}
}
