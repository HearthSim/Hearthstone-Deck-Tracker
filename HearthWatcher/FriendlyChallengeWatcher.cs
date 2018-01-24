using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher
{
	public class FriendlyChallengeWatcher
	{
		public delegate void FriendlyChallengeEventHandler(object sender, FriendlyChallengeEventArgs args);

		private readonly IFriendlyChallengeProvider _challengeProvider;
		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private bool _previousVisibilityValue;

		public FriendlyChallengeWatcher(IFriendlyChallengeProvider challengeProvider, int delay = 500)
		{
			_challengeProvider = challengeProvider ?? throw new ArgumentNullException(nameof(challengeProvider));
			_delay = delay;
		}

		public event FriendlyChallengeEventHandler OnFriendlyChallenge;

		public void Run()
		{
			_watch = true;
			if(!_running)
				CheckForFriendlyChallenge();
		}

		public void Stop() => _watch = false;

		private async void CheckForFriendlyChallenge()
		{
			_running = true;
			while(_watch)
			{
				await Task.Delay(_delay);
				if(!_watch)
					break;
				var dialogVisible = _challengeProvider.DialogVisible;
				if(dialogVisible != _previousVisibilityValue)
				{
					OnFriendlyChallenge?.Invoke(this, new FriendlyChallengeEventArgs(dialogVisible));
					_previousVisibilityValue = dialogVisible;
				}
			}
			_previousVisibilityValue = false;
			_running = false;
		}
	}
}
