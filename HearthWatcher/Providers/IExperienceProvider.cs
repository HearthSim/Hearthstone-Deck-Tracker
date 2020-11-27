using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.Providers
{
	public interface IExperienceProvider
	{
		RewardTrackData GetRewardTrackData();
	}
}
