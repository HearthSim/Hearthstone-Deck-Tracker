using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher
{
	public class FriendlyChallengeDialogVisibilityWatcher
	{
		public delegate void DialogVisibilityChangedEventHandler(object sender, DialogVisibilityEventArgs args);

		private readonly IDialogVisibilityProvider _visibilityProvider;
		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private bool _previousVisibilityValue;

		public FriendlyChallengeDialogVisibilityWatcher(IDialogVisibilityProvider visibilityProvider, int delay = 500)
		{
			_visibilityProvider = visibilityProvider ?? throw new ArgumentNullException(nameof(visibilityProvider));
			_delay = delay;
		}

		public event DialogVisibilityChangedEventHandler OnDialogVisibilityChanged;

		public void Run()
		{
			_watch = true;
			if (!_running)
				CheckForDialogVisibility();
		}

		public void Stop() => _watch = false;

		private async void CheckForDialogVisibility()
		{
			_running = true;
			while (_watch)
			{
				await Task.Delay(_delay);
				if (!_watch)
					break;
				var visible = _visibilityProvider.DialogVisible;
				if (visible != _previousVisibilityValue)
				{
					OnDialogVisibilityChanged?.Invoke(this, new DialogVisibilityEventArgs(visible));
					_previousVisibilityValue = visible;
				}
			}
			_previousVisibilityValue = false;
			_running = false;
		}
	}
}
