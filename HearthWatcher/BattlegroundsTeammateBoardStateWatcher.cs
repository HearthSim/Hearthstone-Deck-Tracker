using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HearthMirror.Objects;

namespace HearthWatcher
{
	public class BattlegroundsTeammateBoardStateWatcher : PollingWatcher
	{
		public delegate void BattlegroundsTeammateBoardStateWatcherHandler(object sender, BattlegroundsTeammateBoardStateArgs args);

		private readonly IBattlegroundsTeammateBoardStateProvider _provider;
		private BattlegroundsTeammateBoardStateArgs? _prev;

		public BattlegroundsTeammateBoardStateWatcher(IBattlegroundsTeammateBoardStateProvider provider, int delay = 200) : base(delay)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
		}

		public event BattlegroundsTeammateBoardStateWatcherHandler? Change;

		protected override Task<bool> TickAsync()
		{
			var curr = new BattlegroundsTeammateBoardStateArgs(
				_provider.BattlegroundsTeammateBoardState?.ViewingTeammate ?? false,
				_provider.BattlegroundsTeammateBoardState?.MulliganHeroes ?? new List<string>(),
				_provider.BattlegroundsTeammateBoardState?.Entities ?? new List<BattlegroundsTeammateBoardStateEntity>()
			);
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
