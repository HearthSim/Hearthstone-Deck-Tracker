#region

using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public class PlayerBoard
	{
		// TODO: optimize this somehow
		public PlayerBoard(List<CardEntity> list, bool activeTurn)
		{
			Cards = new List<BoardEntity>();
			var filtered = Filter(list);
			var weapon = GetWeapon(filtered);
			foreach(var card in filtered)
			{
				if(card.Entity.IsHero)
				{
					Hero = new BoardHero(card, weapon, activeTurn);
					Cards.Add(Hero);
				}
				else
					Cards.Add(new BoardCard(card, activeTurn));
			}
		}

		public BoardHero Hero { get; private set; }

		public List<BoardEntity> Cards { get; private set; }

		public int Damage
		{
			get { return Cards.Where(x => x.Include).Sum(x => x.Attack); }
		}

		public CardEntity GetWeapon(List<CardEntity> list)
		{
			var weapons = list.Where(x => x.Entity.IsWeapon).ToList();
			if(weapons.Count == 1)
				return weapons[0];
			return list.FirstOrDefault(x => x.Entity.HasTag(GAME_TAG.JUST_PLAYED) && x.Entity.GetTag(GAME_TAG.JUST_PLAYED) == 1);
		}

		public override string ToString()
		{
			var health = Hero == null ? 0 : Hero.Health;
			return string.Format("(H:{0} A:{1})", health, Damage);
		}

		private List<CardEntity> Filter(List<CardEntity> cards)
		{
			return
				cards.Where(
				            x =>
				            x != null && x.Entity != null && x.Entity.GetTag(GAME_TAG.CARDTYPE) != (int)TAG_CARDTYPE.ENCHANTMENT
				            && x.Entity.GetTag(GAME_TAG.CARDTYPE) != (int)TAG_CARDTYPE.HERO_POWER
				            && x.Entity.GetTag(GAME_TAG.ZONE) != (int)TAG_ZONE.SETASIDE
				            && x.Entity.GetTag(GAME_TAG.ZONE) != (int)TAG_ZONE.GRAVEYARD).ToList();
		}
	}
}