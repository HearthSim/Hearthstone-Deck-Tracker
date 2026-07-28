using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "The next spell you cast this turn costs (3) less. Discover a spell."
public class HauntingVisions : Marshspawn
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.HauntingVisions;
}
