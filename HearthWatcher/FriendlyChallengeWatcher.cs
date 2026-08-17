using HearthWatcher.EventArgs;
using HearthWatcher.Providers;
using System;
using System.Threading.Tasks;

namespace HearthWatcher
{
	public class FriendlyChallengeWatcher : PollingWatcher
	{
		public delegate void FriendlyChallengeEventHandler(object sender, FriendlyChallengeEventArgs args);

		private readonly IFriendlyChallengeProvider _challengeProvider;
		private bool _previousVisibilityValue;

		public FriendlyChallengeWatcher(IFriendlyChallengeProvider challengeProvider, int delay = 500) : base(delay)
		{
			_challengeProvider = challengeProvider ?? throw new ArgumentNullException(nameof(challengeProvider));
		}

		public event FriendlyChallengeEventHandler? OnFriendlyChallenge;

		protected override Task<bool> TickAsync()
		{
			var dialogVisible = _challengeProvider.DialogVisible;
			if(dialogVisible != _previousVisibilityValue)
			{
				_previousVisibilityValue = dialogVisible;
				Dispatch(() => OnFriendlyChallenge?.Invoke(this, new FriendlyChallengeEventArgs(dialogVisible)));
			}
			return Task.FromResult(false);
		}

		protected override void OnLoopEnd() => _previousVisibilityValue = false;
	}
}
