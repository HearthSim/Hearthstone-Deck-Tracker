using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility
{
	internal class ScheduledTaskRunner : Singleton<ScheduledTaskRunner>
	{
		private readonly Dictionary<TimeSpan, TaskList> _tasks;
		private Tuple<Action, bool> _latest;

		private ScheduledTaskRunner()
		{
			_tasks = new Dictionary<TimeSpan, TaskList>();
		}

		public void Schedule(Action action, TimeSpan interval)
		{
			if(!_tasks.TryGetValue(interval, out var taskList))
			{
				taskList = new TaskList(interval);
				_tasks[interval] = taskList;
			}
			taskList.Add(action);
			_latest = new Tuple<Action, bool>(action, true);
			Run();
		}

		public void Remove(Action action, TimeSpan interval)
		{
			if(_tasks.TryGetValue(interval, out var taskList) && taskList.Remove(action))
			{
				if(!taskList.Any())
					_tasks.Remove(interval);
				_latest = new Tuple<Action, bool>(action, false);
				Run();
			}
		}

		private async void Run()
		{
			var latest = new Tuple<Action, bool>(_latest.Item1, _latest.Item2);
			bool TasksChanged() => latest.Item1 != _latest.Item1 || latest.Item2 != _latest.Item2;
			while(_tasks.Any())
			{
				var delay = _tasks.Values.Min(x => x.TimeRemaining);
				if(delay > TimeSpan.Zero)
					await Task.Delay(delay);
				if(TasksChanged())
					return;
				foreach(var taskList in _tasks.Values.Where(x => x.ShouldRun))
					taskList.Run();
			}
		}

		private class TaskList
		{
			private readonly List<Action> _actions;
			private readonly TimeSpan _interval;
			private DateTime _lastRun;

			public TaskList(TimeSpan interval)
			{
				_interval = interval;
				_actions = new List<Action>();
				_lastRun = DateTime.Now;
			}

			private TimeSpan TimeSinceLastRun => DateTime.Now - _lastRun;
			public TimeSpan TimeRemaining => _interval - TimeSinceLastRun;
			public bool ShouldRun => TimeRemaining <= TimeSpan.Zero;

			public void Run()
			{
				foreach(var action in _actions)
					action?.Invoke();
				_lastRun = DateTime.Now;
			}

			public void Add(Action action) => _actions.Add(action);

			public bool Remove(Action action) => _actions.Remove(action);

			public bool Any() => _actions.Any();
		}
	}
}
