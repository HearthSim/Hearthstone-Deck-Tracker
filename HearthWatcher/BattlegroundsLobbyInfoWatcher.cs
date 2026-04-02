using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher
{
	public class BattlegroundsLobbyInfoWatcher
	{
		public delegate void BattlegroundsLobbyInfoEventHandler(object sender, BattlegroundsLobbyInfoArgs args);

		private readonly IBattlegroundsLobbyInfoProvider _provider;
		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private BattlegroundsLobbyInfoArgs? _prev;

		public BattlegroundsLobbyInfoWatcher(IBattlegroundsLobbyInfoProvider provider, int delay = 200)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
			_delay = delay;
		}

		public event BattlegroundsLobbyInfoEventHandler Change;

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
				var curr = new BattlegroundsLobbyInfoArgs(_provider.BattlegroundsLobbyInfo);
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
