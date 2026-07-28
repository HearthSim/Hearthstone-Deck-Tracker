using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Discover 2 Demons. Finale: Give them +1/+2."
public class DemonicDynamics : DemonicStudies
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.DemonicDynamics;
	public override int Picks() => 3;
	public override int EventCount() => 2;
}
