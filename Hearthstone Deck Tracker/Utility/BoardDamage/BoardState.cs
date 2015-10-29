using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public class BoardState
	{
		public PlayerBoard Player { get; private set; }
		public PlayerBoard Opponent { get; private set; }

		public BoardState()
		{
			Player = CreatePlayerBoard();
			Opponent = CreateOpponentBoard();
		}

		public BoardState(List<CardEntity> player, List<CardEntity> opponent, Dictionary<int, Entity> entities, int playerId)
		{
			Player = CreateBoard(player, entities, true, playerId);
			Opponent = CreateBoard(opponent, entities, false, playerId);
		}

		public bool IsPlayerDeadToBoard()
		{
			if(Player.Hero == null)
				return true;
			return Opponent.Damage >= Player.Hero.Health;
		}

		public bool IsOpponentDeadToBoard()
		{
			if(Opponent.Hero == null)
				return true;
			return Player.Damage >= Opponent.Hero.Health;
		}

		private PlayerBoard CreatePlayerBoard()
		{
			return CreateBoard(new List<CardEntity>(Core.Game.Player.Board), Core.Game.Entities, true, Core.Game.Player.Id);
		}

		private PlayerBoard CreateOpponentBoard()
		{
			return CreateBoard(new List<CardEntity>(Core.Game.Opponent.Board), Core.Game.Entities, false, Core.Game.Player.Id);
		}

		private PlayerBoard CreateBoard(List<CardEntity> list, Dictionary<int, Entity> entities, bool isPlayer, int playerId)
		{
			var activeTurn = !(EntityHelper.IsPlayersTurn(entities) ^ isPlayer);
			// if there is no hero in the list, try to find it
			var heroFound = list.Any(e => EntityHelper.IsHero(e.Entity));
			if(!heroFound)
			{
				var hero = EntityHelper.GetHeroEntity(isPlayer, entities, playerId);
				if (hero != null)
					list.Add(new CardEntity(hero));
			}

			return new PlayerBoard(list, activeTurn);
		}		
	}
}
