using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Restore #5 Health. Discover a spell."
public class AmphibiousElixir : Marshspawn
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.AmphibiousElixir;
}
