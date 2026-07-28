using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: Summon a random 5-Cost minion. Repeat for each spell school you've cast this game."
public class RazzleDazzler : TinyToys
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.RazzleDazzler;
	public override int EventCount() => 1;
}
