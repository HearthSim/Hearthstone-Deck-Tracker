#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class GameTime
	{
		private DateTime _time;
		public List<TimedTaskCompletionSource<object>> TimedTasks = new List<TimedTaskCompletionSource<object>>();

		public DateTime Time
		{
			get { return _time; }
			set
			{
				_time = value;
				foreach(var task in TimedTasks.Where(x => x.ExpirationTime <= value).OrderBy(x => x.ExpirationTime).ToList())
				{
					task.TaskCompletionSource.SetResult(null);
					TimedTasks.Remove(task);
				}
			}
		}

		public Task WaitForDuration(int milliseconds)
		{
			var tcs = new TaskCompletionSource<object>();
			TimedTasks.Add(new TimedTaskCompletionSource<object>(tcs, Time.AddMilliseconds(milliseconds)));
			return tcs.Task;
		}

		public class TimedTaskCompletionSource<T>
		{
			public TimedTaskCompletionSource(TaskCompletionSource<T> taskCompletionSource, DateTime expirationTime)
			{
				TaskCompletionSource = taskCompletionSource;
				ExpirationTime = expirationTime;
			}

			public TaskCompletionSource<T> TaskCompletionSource { get; set; }
			public DateTime ExpirationTime { get; set; }
		}
	}
}
