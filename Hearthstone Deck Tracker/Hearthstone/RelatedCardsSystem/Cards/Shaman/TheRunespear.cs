using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "After your hero attacks, Discover a spell and cast it with random targets."
public class TheRunespear : Marshspawn
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.TheRunespear;
}
