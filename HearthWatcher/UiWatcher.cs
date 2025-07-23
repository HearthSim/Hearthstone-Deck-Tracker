using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher
{
	public class UiWatcher
	{
		public delegate void UiEventHandler(object sender, UIEventArgs args);

		private readonly IUiProvider _provider;
		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private UIEventArgs _prev = null;

		public UiWatcher(IUiProvider uiProvider, int delay = 200)
		{
			_provider = uiProvider ?? throw new ArgumentNullException(nameof(uiProvider));
			_delay = delay;
		}

		public event UiEventHandler Change;

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
				var curr = new UIEventArgs(
					_provider.IsShopOpen ?? false,
					_provider.IsJournalOpen ?? false,
					_provider.IsPopupShowing ?? false,
					_provider.IsFriendsListVisible ?? false,
					_provider.IsBlurActive ?? false
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
