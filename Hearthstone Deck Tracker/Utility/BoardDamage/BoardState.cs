#region

using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public class BoardState
	{
		public BoardState()
		{
			Player = CreatePlayerBoard();
			Opponent = CreateOpponentBoard();
		}

		public BoardState(List<Entity> player, List<Entity> opponent, Dictionary<int, Entity> entities, int playerId)
		{
			Player = CreateBoard(player, entities, true, playerId);
			Opponent = CreateBoard(opponent, entities, false, playerId);
		}

		public PlayerBoard Player { get; }
		public PlayerBoard Opponent { get; }

		public bool IsPlayerDeadToBoard() => Player.Hero == null || Opponent.Damage >= Player.Hero.Health;

		public bool IsOpponentDeadToBoard() => Opponent.Hero == null || Player.Damage >= Opponent.Hero.Health;

		private PlayerBoard CreatePlayerBoard() => CreateBoard(new List<Entity>(Core.Game.Player.Board), Core.Game.Entities, true, Core.Game.Player.Id);

		private PlayerBoard CreateOpponentBoard() => CreateBoard(new List<Entity>(Core.Game.Opponent.Board), Core.Game.Entities, false, Core.Game.Player.Id);

		private PlayerBoard CreateBoard(List<Entity> list, Dictionary<int, Entity> entities, bool isPlayer, int playerId)
		{
			var activeTurn = !(EntityHelper.IsPlayersTurn(entities) ^ isPlayer);
			// if there is no hero in the list, try to find it
			var heroFound = list.Any(EntityHelper.IsHero);
			if(!heroFound)
				list?.Add(EntityHelper.GetHeroEntity(isPlayer, entities, playerId));

			return new PlayerBoard(list, activeTurn);
		}
	}
}
