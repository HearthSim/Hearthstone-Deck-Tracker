using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher
{
	public class ChoicesWatcher
	{
		public delegate void ChoicesEventHandler(object sender, EventArgs.ChoicesWatcher args);

		private readonly IChoicesProvider _provider;
		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private EventArgs.ChoicesWatcher? _prev = null;

		public ChoicesWatcher(IChoicesProvider choicesProvider, int delay = 16)
		{
			_provider = choicesProvider ?? throw new ArgumentNullException(nameof(choicesProvider));
			_delay = delay;
		}

		public event ChoicesEventHandler? Change;

		public void Run()
		{
			_watch = true;
			if(!_running)
				Update();
		}

		public void Stop() => _watch = false;

		private async void Update()
		{
			_running = true;
			while(_watch)
			{
				await Task.Delay(_delay);
				if(!_watch)
					break;
				var curr = new EventArgs.ChoicesWatcher(_provider.CurrentChoice);
				if(curr.Equals(_prev))
					continue;
				Change?.Invoke(this, curr);
				_prev = curr;
			}
			_prev = null;
			_running = false;
		}
	}
}
