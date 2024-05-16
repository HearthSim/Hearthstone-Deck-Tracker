using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HearthMirror.Objects;

namespace HearthWatcher
{
	public class BattlegroundsLeaderboardWatcher
	{
		public delegate void BattlegroundsLeaderboardEventHandler(object sender, BattlegroundsLeaderboardArgs args);

		private readonly IBattlegroundsLeaderboardProvider _provider;
		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private BattlegroundsLeaderboardArgs? _prev;

		public BattlegroundsLeaderboardWatcher(IBattlegroundsLeaderboardProvider provider, int delay = 16)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
			_delay = delay;
		}

		public event BattlegroundsLeaderboardEventHandler Change;

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
				var curr = new BattlegroundsLeaderboardArgs(
					_provider.BattlegroundsLeaderboardHoveredEntityId
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
