#region

using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public static class EntityHelper
	{
		public static bool IsHero(Entity e)
		{
			return e.HasTag(GAME_TAG.CARDTYPE) && e.GetTag(GAME_TAG.CARDTYPE) == (int)TAG_CARDTYPE.HERO && e.HasTag(GAME_TAG.ZONE)
			       && e.GetTag(GAME_TAG.ZONE) == (int)TAG_ZONE.PLAY;
		}

		public static Entity GetHeroEntity(bool forPlayer)
		{
			return GetHeroEntity(forPlayer, Core.Game.Entities, Core.Game.Player.Id);
		}

		public static Entity GetHeroEntity(bool forPlayer, Dictionary<int, Entity> entities, int id)
		{
			if(!forPlayer)
				id = (id % 2) + 1;

			var heros = entities.Where(x => IsHero(x.Value)).Select(x => x.Value).ToList();

			return heros.FirstOrDefault(x => x.GetTag(GAME_TAG.CONTROLLER) == id);
		}

		public static bool IsPlayersTurn()
		{
			return IsPlayersTurn(Core.Game.Entities);
		}

		public static bool IsPlayersTurn(Dictionary<int, Entity> entities)
		{
			var firstPlayer = entities.FirstOrDefault(e => e.Value.HasTag(GAME_TAG.FIRST_PLAYER)).Value;
			if(firstPlayer != null)
			{
				int offset = firstPlayer.IsPlayer ? 0 : 1;
				Entity gameRoot = entities.FirstOrDefault(e => e.Value != null && e.Value.Name == "GameEntity").Value;
				if(gameRoot != null)
					return (gameRoot.Tags[GAME_TAG.TURN] + offset) % 2 == 1;
			}
			return false;
		}
	}
}