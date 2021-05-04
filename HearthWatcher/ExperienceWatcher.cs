using System.Threading.Tasks;
using HearthMirror.Objects;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;

namespace HearthWatcher
{
	public class ExperienceWatcher
	{
		public delegate void ExperienceEventHandler(object sender, ExperienceEventArgs args);

		private RewardTrackData _rewardTrackData = null;
		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private readonly IExperienceProvider _experienceProvider;

		public ExperienceWatcher(IExperienceProvider experienceProvider, int delay = 1000)
		{
			_delay = delay;
			_experienceProvider = experienceProvider;
		}

		public event ExperienceEventHandler NewExperienceHandler;

		public void Run()
		{
			_watch = true;
			if(!_running)
				CheckForExperience();
		}

		public void Stop() => _watch = false;

		private async void CheckForExperience()
		{
			_running = true;
			while(_watch)
			{
				await Task.Delay(_delay);
				if(!_watch)
					break;
				var newRewards = _experienceProvider.GetRewardTrackData();
				if(newRewards != null)
				{
					if(_rewardTrackData == null ||
					_rewardTrackData.Xp != newRewards.Xp ||
					_rewardTrackData.Level != newRewards.Level ||
					_rewardTrackData.XpNeeded != newRewards.XpNeeded)
					{
						NewExperienceHandler?.Invoke(this, new ExperienceEventArgs(newRewards.Xp, newRewards.XpNeeded, newRewards.Level, _rewardTrackData != null ? newRewards.Level - _rewardTrackData.Level : 0, ShouldAnimate(newRewards)));
					}
					_rewardTrackData = newRewards;
				}
			}
			_running = false;
		}

		//Difficult to replicate, but there appears to be an issue where the old levels will appear once with level = 0 or level = 1 improperly.
		//This looks like how it does when it behaves properly and someone just gains a few levels, so it's difficult to parse from this incorrect case.
		//Therefore this should catch the jumps from 0 levels to a high levels (spamming the player with animations) and high xp gains at low levels.

		private bool ShouldAnimate (RewardTrackData newRewards)
		{
			if(_rewardTrackData == null)
				return false;
			if(_rewardTrackData.Level <= 1 && newRewards.Level - _rewardTrackData.Level > 5)
				return false;

			return true;
		}
	}
}
