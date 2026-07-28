using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Summon two random 1-Cost minions. Overload: (1)"
public class FirstContact : MaelstromPortal
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.FirstContact;
	public override int EventCount() => 2;
}
