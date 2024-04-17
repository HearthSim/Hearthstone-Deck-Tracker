using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HearthMirror.Objects;

namespace HearthWatcher
{
	public class BattlegroundsTeammateBoardStateWatcher
	{
		public delegate void BattlegroundsTeammateBoardStateWatcherHandler(object sender, BattlegroundsTeammateBoardStateArgs args);

		private readonly IBattlegroundsTeammateBoardStateProvider _provider;
		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private BattlegroundsTeammateBoardStateArgs? _prev;

		public BattlegroundsTeammateBoardStateWatcher(IBattlegroundsTeammateBoardStateProvider provider, int delay = 200)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
			_delay = delay;
		}

		public event BattlegroundsTeammateBoardStateWatcherHandler Change;

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
				var curr = new BattlegroundsTeammateBoardStateArgs(
					_provider.BattlegroundsTeammateBoardState?.ViewingTeammate ?? false,
					_provider.BattlegroundsTeammateBoardState?.MulliganHeroes ?? new List<string>(),
					_provider.BattlegroundsTeammateBoardState?.Entities ?? new List<BattlegroundsTeammateBoardStateEntity>()
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
