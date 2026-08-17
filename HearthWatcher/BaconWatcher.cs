using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;
using HearthMirror.Objects;

namespace HearthWatcher
{
	public class BaconWatcher : PollingWatcher
	{
		public delegate void BaconEventHandler(object sender, BaconEventArgs args);

		private readonly IBaconProvider _provider;
		private BaconEventArgs? _prev;

		public BaconWatcher(IBaconProvider baconProvider, int delay = 200) : base(delay)
		{
			_provider = baconProvider ?? throw new ArgumentNullException(nameof(baconProvider));
		}

		public event BaconEventHandler? Change;

		protected override Task<bool> TickAsync()
		{
			var curr = new BaconEventArgs(
				_provider.SelectedBattlegroundsGameMode ?? SelectedBattlegroundsGameMode.UNKNOWN
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
