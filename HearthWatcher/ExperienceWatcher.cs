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

		public ExperienceWatcher(IExperienceProvider experienceProvider, int delay = 500)
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
						NewExperienceHandler?.Invoke(this, new ExperienceEventArgs(newRewards.Xp, newRewards.XpNeeded, newRewards.Level, _rewardTrackData != null ? newRewards.Level - _rewardTrackData.Level : 0, _rewardTrackData != null));
					}
					_rewardTrackData = newRewards;
				}
			}
			_running = false;
		}
	}
}
