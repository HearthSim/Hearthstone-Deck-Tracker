#region

using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

#endregion

namespace Hearthstone_Deck_Tracker.Replay
{
	public static class ReplayHelper
	{
		public static bool IsInZone(this Entity entity, TAG_ZONE zone)
		{
			return entity.HasTag(GAME_TAG.ZONE) && entity.GetTag(GAME_TAG.ZONE) == (int)zone;
		}

		public static bool IsControlledBy(this Entity entity, int controller)
		{
			return entity.HasTag(GAME_TAG.CONTROLLER) && entity.GetTag(GAME_TAG.CONTROLLER) == controller;
		}
	}
}