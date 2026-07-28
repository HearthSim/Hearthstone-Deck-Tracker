using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Whenever this takes damage, get a random Fire spell. It costs (3) less."
// Non-collectible token created by Vulcanos. Fire spell pool + generator inherited from Pyrotechnician.
public class PlumeOfVulcanos : FireSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Mage.Vulcanos_PlumeOfVulcanosToken1;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class PlumeOfVulcanos2 : PlumeOfVulcanos
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Mage.Vulcanos_PlumeOfVulcanosToken2;
}
