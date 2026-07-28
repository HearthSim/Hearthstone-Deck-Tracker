using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Stealth Whenever this attacks, add a card from another class to your hand."
public class ShakuTheCollector : OffClassCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.ShakuTheCollector;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}

public class ShakuTheCollectorCore : ShakuTheCollector
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.ShakuTheCollectorCore;
}
