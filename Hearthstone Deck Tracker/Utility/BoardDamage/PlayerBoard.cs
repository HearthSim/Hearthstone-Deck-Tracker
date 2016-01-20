#region

using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using static Hearthstone_Deck_Tracker.Enums.GAME_TAG;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.BoardDamage
{
	public class PlayerBoard
	{
		// TODO: optimize this somehow
		public PlayerBoard(List<CardEntity> list, bool activeTurn)
		{
			Cards = new List<IBoardEntity>();
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

		public BoardHero Hero { get; }

		public List<IBoardEntity> Cards { get; }

		public int Damage => Cards.Where(x => x.Include).Sum(x => x.Attack);

		public CardEntity GetWeapon(List<CardEntity> list)
		{
			var weapons = list.Where(x => x.Entity.IsWeapon).ToList();
			return weapons.Count == 1 ? weapons[0] : list.FirstOrDefault(x => x.Entity.HasTag(JUST_PLAYED) && x.Entity.GetTag(JUST_PLAYED) == 1);
		}

		public override string ToString() => $"(H:{Hero?.Health ?? 0} A:{Damage})";

		private List<CardEntity> Filter(List<CardEntity> cards)
			=>
				cards.Where(
						    x =>
							x?.Entity?.GetTag(CARDTYPE) != (int)TAG_CARDTYPE.ENCHANTMENT && x?.Entity?.GetTag(CARDTYPE) != (int)TAG_CARDTYPE.HERO_POWER
							&& x?.Entity?.GetTag(ZONE) != (int)TAG_ZONE.SETASIDE && x?.Entity?.GetTag(ZONE) != (int)TAG_ZONE.GRAVEYARD).ToList();
	}
}