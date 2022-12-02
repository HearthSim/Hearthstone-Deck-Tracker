using HearthMirror.Enums;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher
{
	public class QueueWatcher
	{
		public delegate void QueueEventHandler(object sender, QueueEventArgs args);

		private readonly IQueueProvider _provider;
		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private FindGameState? _prev = null;

		public QueueWatcher(IQueueProvider queueProvider, int delay = 200)
		{
			_provider = queueProvider ?? throw new ArgumentNullException(nameof(queueProvider));
			_delay = delay;
		}

		public event QueueEventHandler InQueueChanged;

		public void Run()
		{
			_watch = true;
			if(!_running)
				CheckForQueue();
		}

		public void Stop() => _watch = false;

		private async void CheckForQueue()
		{
			_running = true;
			while(_watch)
			{
				await Task.Delay(_delay);
				if(!_watch)
					break;
				var state = _provider.FindGameState;
				var isInQueue = state != null && state > 0;
				var wasInQueue = _prev != null && _prev > 0;
				if(isInQueue != wasInQueue)
					InQueueChanged?.Invoke(this, new QueueEventArgs(isInQueue, state, _prev));
				_prev = state;
			}
			_prev = null;
			_running = false;
		}
	}
}
