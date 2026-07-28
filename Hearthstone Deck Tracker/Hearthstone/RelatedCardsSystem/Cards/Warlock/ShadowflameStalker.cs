using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Battlecry: Discover a Demon with a Dark Gift. Get a copy of it."
public class ShadowflameStalker : DemonicStudies
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.ShadowflameStalker;
}
