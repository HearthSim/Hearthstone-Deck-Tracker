using HearthMirror.Objects;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	internal record ExperienceUpdate(int Experience, int ExperienceNeeded, int Level, int LevelChange, bool Animate);

	internal class ExperienceTracker
	{
		private RewardTrackData? _prev;

		// returns null when the values did not change since the last update
		public ExperienceUpdate? Update(RewardTrackData data)
		{
			var prev = _prev;
			_prev = data;
			if(prev != null && prev.Xp == data.Xp && prev.Level == data.Level && prev.XpNeeded == data.XpNeeded)
				return null;
			return new ExperienceUpdate(
				data.Xp,
				data.XpNeeded,
				data.Level,
				prev != null ? data.Level - prev.Level : 0,
				ShouldAnimate(prev, data)
			);
		}

		//Difficult to replicate, but there appears to be an issue where the old levels will appear once with level = 0 or level = 1 improperly.
		//This looks like how it does when it behaves properly and someone just gains a few levels, so it's difficult to parse from this incorrect case.
		//Therefore this should catch the jumps from 0 levels to a high levels (spamming the player with animations) and high xp gains at low levels.
		private static bool ShouldAnimate(RewardTrackData? prev, RewardTrackData curr)
		{
			if(prev == null)
				return false;
			if(prev.Level <= 1 && curr.Level - prev.Level > 5)
				return false;
			return true;
		}
	}
}
