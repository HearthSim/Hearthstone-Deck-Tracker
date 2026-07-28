using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Deal $2 damage to a friendly character. Discover a Demon."
public class DarkPossession : DemonicStudies
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DarkPossession;
}
