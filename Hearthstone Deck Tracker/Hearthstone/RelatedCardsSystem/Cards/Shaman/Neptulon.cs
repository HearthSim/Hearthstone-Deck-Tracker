using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: Add 4 random Murlocs to your hand. Overload: (3)"
public class Neptulon : UnderbellyAngler
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Neptulon;
	public override int EventCount() => 4;
}
