using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher
{
	public class BattlegroundsLobbyInfoWatcher : PollingWatcher
	{
		public delegate void BattlegroundsLobbyInfoEventHandler(object sender, BattlegroundsLobbyInfoArgs args);

		private readonly IBattlegroundsLobbyInfoProvider _provider;
		private BattlegroundsLobbyInfoArgs? _prev;

		public BattlegroundsLobbyInfoWatcher(IBattlegroundsLobbyInfoProvider provider, int delay = 200) : base(delay)
		{
			_provider = provider ?? throw new ArgumentNullException(nameof(provider));
		}

		public event BattlegroundsLobbyInfoEventHandler? Change;

		protected override Task<bool> TickAsync()
		{
			var curr = new BattlegroundsLobbyInfoArgs(_provider.BattlegroundsLobbyInfo);
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
