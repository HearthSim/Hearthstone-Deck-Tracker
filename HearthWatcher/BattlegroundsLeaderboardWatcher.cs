using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher
{
	public class BattlegroundsLeaderboardWatcher : PollingWatcher
	{
		public delegate void BattlegroundsLeaderboardEventHandler(object sender, BattlegroundsLeaderboardArgs args);

		private readonly IBattlegroundsLeaderboardProvider _provider;
		private BattlegroundsLeaderboardArgs? _prev;

		public BattlegroundsLeaderboardWatcher(IBattlegroundsLeaderboardProvider provider, int delay = 16) : base(delay)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
		}

		public event BattlegroundsLeaderboardEventHandler? Change;

		protected override Task<bool> TickAsync()
		{
			var curr = new BattlegroundsLeaderboardArgs(
				_provider.BattlegroundsLeaderboardHoveredEntityId
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
