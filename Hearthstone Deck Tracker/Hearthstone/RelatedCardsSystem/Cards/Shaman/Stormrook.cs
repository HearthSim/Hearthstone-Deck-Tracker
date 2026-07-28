using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Whenever you would damage this with a Nature spell, summon a random 5-Cost minion instead."
public class Stormrook : TinyToys
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Stormrook;
	public override int EventCount() => 1;
}
