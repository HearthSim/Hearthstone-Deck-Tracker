using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher
{
	public class ChoicesWatcher : PollingWatcher
	{
		public delegate void ChoicesEventHandler(object sender, EventArgs.ChoicesWatcher args);

		private readonly IChoicesProvider _provider;
		private EventArgs.ChoicesWatcher? _prev;

		public ChoicesWatcher(IChoicesProvider choicesProvider, int delay = 16) : base(delay)
		{
			_provider = choicesProvider ?? throw new ArgumentNullException(nameof(choicesProvider));
		}

		public event ChoicesEventHandler? Change;

		protected override Task<bool> TickAsync()
		{
			var curr = new EventArgs.ChoicesWatcher(_provider.CurrentChoice);
			if(_prev == null || !curr.Equals(_prev))
			{
				_prev = curr;
				Dispatch(() => Change?.Invoke(this, curr));
			}
			return Task.FromResult(false);
		}

		protected override void OnLoopEnd() => _prev = null;
	}
}
