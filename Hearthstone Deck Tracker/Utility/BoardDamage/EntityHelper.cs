#region

using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using static HearthDb.Enums.GameTag;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public static class EntityHelper
	{
		public static bool IsHero(Entity e) => e.HasTag(CARDTYPE) && e.GetTag(CARDTYPE) == (int)CardType.HERO && e.HasTag(ZONE)
											   && e.GetTag(ZONE) == (int)Zone.PLAY;

		public static Entity GetHeroEntity(bool forPlayer) => GetHeroEntity(forPlayer, Core.Game.Entities, Core.Game.Player.Id);

		public static Entity GetHeroEntity(bool forPlayer, Dictionary<int, Entity> entities, int id)
		{
			if(!forPlayer)
				id = (id % 2) + 1;
			var heroes = entities.Where(x => IsHero(x.Value)).Select(x => x.Value).ToList();
			return heroes.FirstOrDefault(x => x.GetTag(CONTROLLER) == id);
		}

		public static bool IsPlayersTurn() => IsPlayersTurn(Core.Game.Entities);

		public static bool IsPlayersTurn(Dictionary<int, Entity> entities)
		{
			var firstPlayer = entities.FirstOrDefault(e => e.Value.HasTag(FIRST_PLAYER)).Value;
			if(firstPlayer != null)
			{
				var offset = firstPlayer.IsPlayer ? 0 : 1;
				var gameRoot = entities.FirstOrDefault(e => e.Value != null && e.Value.Name == "GameEntity").Value;
				if(gameRoot == null)
					return false;
				var turn = gameRoot.GetTag(TURN);
				return turn > 0 && ((turn + offset)%2 == 1);
			}
			return false;
		}
	}
}
