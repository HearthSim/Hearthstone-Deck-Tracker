using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class RateLimiter
	{
		private readonly int _maxCount;
		private readonly Queue<DateTime> _lastRun;
		private readonly TimeSpan _timeSpan;
		private Func<Task> _nextTask;
		private bool _running;

		public RateLimiter(int maxCount, TimeSpan timeSpan)
		{
			_maxCount = maxCount;
			_timeSpan = timeSpan;
			_lastRun = new Queue<DateTime>();
		}

		public async Task Run(Func<Task> task, Action onThrottled = null)
		{
			_nextTask = task;
			if(_running)
				return;
			_running = true;
			await RateLimit(onThrottled);
			_lastRun.Enqueue(DateTime.Now);
			await _nextTask();
			_running = false;
		}

		private async Task RateLimit(Action onThrottled)
		{
			while(_lastRun.Any())
			{
				if(_lastRun.Peek() >= DateTime.Now - _timeSpan)
					break;
				_lastRun.Dequeue();
			}
			if(_lastRun.Count >= _maxCount)
			{
				onThrottled?.Invoke();
				await Task.Delay(_timeSpan - (DateTime.Now - _lastRun.Peek()));
			}
		}
	}
}
