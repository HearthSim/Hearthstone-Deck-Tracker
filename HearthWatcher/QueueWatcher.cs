using HearthMirror.Enums;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher
{
	public class QueueWatcher : PollingWatcher
	{
		public delegate void QueueEventHandler(object sender, QueueEventArgs args);

		private readonly IQueueProvider _provider;
		private FindGameState? _prev;

		public QueueWatcher(IQueueProvider queueProvider, int delay = 50) : base(delay)
		{
			_provider = queueProvider ?? throw new ArgumentNullException(nameof(queueProvider));
		}

		public event QueueEventHandler? InQueueChanged;

		protected override Task<bool> TickAsync()
		{
			var state = _provider.FindGameState;
			var isInQueue = state != null && state > 0;
			var wasInQueue = _prev != null && _prev > 0;
			var prev = _prev;
			_prev = state;
			if(isInQueue != wasInQueue)
				Dispatch(() => InQueueChanged?.Invoke(this, new QueueEventArgs(isInQueue, state, prev)));
			return Task.FromResult(false);
		}

		protected override void OnLoopEnd() => _prev = null;
	}
}
