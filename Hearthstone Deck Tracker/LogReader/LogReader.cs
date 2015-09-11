using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.LogReader
{
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

		public void Start(DateTime startingPoint)
		{
			MoveOrDeleteLogFile();
			_startingPoint = startingPoint;
			_stop = false;
			_offset = 0;
			_thread = new Thread(ReadLogFile) { IsBackground = true };
			_thread.Start();
		}

		private void MoveOrDeleteLogFile()
		{
			if(File.Exists(_filePath))
			{
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
				try
				{
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
			while(_running)
				await Task.Delay(50);
			await Task.Factory.StartNew(() => _thread.Join());
		}

		public List<LogLineItem> Collect() 
		{
			lock(_sync)
			{
				_collected = true;
				return _lines.ToList();
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
									_offset += Encoding.UTF8.GetByteCount(line + Environment.NewLine);
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
}