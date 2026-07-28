using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Get a random Demon. Reduce its Cost by (2). Repeatable this turn."
public class AbductionRay : CallOfTheVoidLegacy
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.AbductionRay;
}
