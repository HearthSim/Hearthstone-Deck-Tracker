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

		public BoardState(List<CardEntity> player, List<CardEntity> opponent)
		{
			Player = CreateBoard(player, true);
			Opponent = CreateBoard(opponent, false);
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
			return CreateBoard(new List<CardEntity>(Core.Game.Player.Board), true);
		}

		private PlayerBoard CreateOpponentBoard()
		{
			return CreateBoard(new List<CardEntity>(Core.Game.Opponent.Board), false);
		}

		private PlayerBoard CreateBoard(List<CardEntity> list, bool isPlayer)
		{
			var activeTurn = !(EntityHelper.IsPlayersTurn() ^ isPlayer);
			// if there is no hero in the list, try to find it
			var heroFound = list.Any(e => EntityHelper.IsHero(e.Entity));
			if(!heroFound)
			{
				var hero = EntityHelper.GetHeroEntity(isPlayer);
				if (hero != null)
					list.Add(new CardEntity(hero));
			}

			return new PlayerBoard(list, activeTurn);
		}		
	}
}
