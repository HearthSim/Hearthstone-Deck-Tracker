#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthWatcher.LogReader;

#endregion

namespace HearthWatcher
{
	public class LogWatcher
	{
		internal const int UpdateDelay = 100;
		private readonly List<LogFileWatcher> _logWatchers = new List<LogFileWatcher>();
		private bool _running;
		private bool _stop;

		public LogWatcher(IEnumerable<LogWatcherInfo> logReaderInfos)
		{
			_logWatchers.AddRange(logReaderInfos.Select(x => new LogFileWatcher(x)));
			foreach(var watcher in _logWatchers)
			{
				watcher.OnLogFileFound += (msg) => OnLogFileFound?.Invoke(msg);
				watcher.OnLogLineIgnored += (msg) => OnLogLineIgnored?.Invoke(msg);
			}
		}

		private LogFileWatcher PowerLogWatcher => _logWatchers.Single(x => x.Info.Name == "Power");
		private LogFileWatcher LoadingScreenLogWatcher => _logWatchers.Single(x => x.Info.Name == "LoadingScreen");

		public event Action<List<LogLine>> OnNewLines;
		public event Action<string> OnLogLineIgnored;
		public event Action<string> OnLogFileFound;

		public async void Start(string logDirectory)
		{
			if(_running)
				return;
			var startingPoint = GetStartingPoint(logDirectory);
			foreach(var logReader in _logWatchers)
				logReader.Start(startingPoint, logDirectory);
			_running = true;
			_stop = false;
			var newLines = new SortedList<DateTime, List<LogLine>>();
			while(!_stop)
			{
				await Task.Factory.StartNew(() =>
				{
					foreach(var logReader in _logWatchers)
					{
						var lines = logReader.Collect();
						foreach(var line in lines)
						{
							if(!newLines.TryGetValue(line.Time, out var logLines))
								newLines.Add(line.Time, logLines = new List<LogLine>());
							logLines.Add(line);
						}
					}
				});
				OnNewLines?.Invoke(new List<LogLine>(newLines.Values.SelectMany(x => x)));
				newLines.Clear();
				await Task.Delay(UpdateDelay);
			}
			_running = false;
		}

		private DateTime GetStartingPoint(string logDirectory)
		{
			var powerEntry = PowerLogWatcher.FindEntryPoint(logDirectory, new[] { "tag=GOLD_REWARD_STATE", "End Spectator" });
			var lsEntry = LoadingScreenLogWatcher.FindEntryPoint(logDirectory, "Gameplay.Start");
			return lsEntry > powerEntry ? lsEntry : powerEntry;
		}

		public async Task<bool> Stop(bool force = false)
		{
			if(!_running)
				return false;
			_stop = true;
			while(_running)
				await Task.Delay(50);
			await Task.WhenAll(_logWatchers.Where(x => force || x.Info.Reset).Select(x => x.Stop()));
			return true;
		}
	}
}
