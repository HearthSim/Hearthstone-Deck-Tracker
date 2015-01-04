using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

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
