using System;
using System.Threading.Tasks;
using HearthMirror.Objects;
using HearthWatcher.Providers;

namespace HearthWatcher
{
	public class ExperienceWatcher : PollingWatcher
	{
		private readonly IExperienceProvider _experienceProvider;
		private RewardTrackData? _prev;

		public ExperienceWatcher(IExperienceProvider experienceProvider, int delay = 1000) : base(delay)
		{
			_experienceProvider = experienceProvider;
		}

		public event Action<RewardTrackData>? RewardTrackDataChanged;

		protected override Task<bool> TickAsync()
		{
			var newRewards = _experienceProvider.GetRewardTrackData();
			if(newRewards != null)
			{
				if(_prev == null || _prev.Xp != newRewards.Xp || _prev.Level != newRewards.Level || _prev.XpNeeded != newRewards.XpNeeded)
					Dispatch(() => RewardTrackDataChanged?.Invoke(newRewards));
				_prev = newRewards;
			}
			return Task.FromResult(false);
		}

		protected override void OnLoopEnd() => _prev = null;
	}
}
