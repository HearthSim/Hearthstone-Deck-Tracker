using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher
{
	public class UiWatcher : PollingWatcher
	{
		public delegate void UiEventHandler(object sender, UIEventArgs args);

		private readonly IUiProvider _provider;
		private UIEventArgs? _prev;

		public UiWatcher(IUiProvider uiProvider, int delay = 200) : base(delay)
		{
			_provider = uiProvider ?? throw new ArgumentNullException(nameof(uiProvider));
		}

		public event UiEventHandler? Change;

		protected override Task<bool> TickAsync()
		{
			var curr = new UIEventArgs(
				_provider.IsShopOpen ?? false,
				_provider.IsJournalOpen ?? false,
				_provider.IsPopupShowing ?? false,
				_provider.IsFriendsListVisible ?? false,
				_provider.IsBlurActive ?? false,
				_provider.IsGameMenuShown ?? false
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
