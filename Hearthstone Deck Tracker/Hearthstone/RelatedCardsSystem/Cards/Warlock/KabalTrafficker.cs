using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "At the end of your turn, add a random Demon to your hand."
public class KabalTrafficker : CallOfTheVoidLegacy
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.KabalTrafficker;
}
