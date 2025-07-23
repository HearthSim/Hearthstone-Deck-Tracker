using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;
using HearthMirror.Objects;

namespace HearthWatcher
{
	public class BaconWatcher
	{
		public delegate void BaconEventHandler(object sender, BaconEventArgs args);

		private readonly IBaconProvider _provider;
		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private BaconEventArgs _prev = null;

		public BaconWatcher(IBaconProvider baconProvider, int delay = 200)
		{
			_provider = baconProvider ?? throw new ArgumentNullException(nameof(baconProvider));
			_delay = delay;
		}

		public event BaconEventHandler Change;

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
				var curr = new BaconEventArgs(
					_provider.SelectedBattlegroundsGameMode ?? SelectedBattlegroundsGameMode.UNKNOWN
				);
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
