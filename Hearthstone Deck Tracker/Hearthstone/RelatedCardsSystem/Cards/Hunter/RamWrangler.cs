using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Battlecry: If you have a Beast, summon a random Beast."
public class RamWrangler : BeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.RamWrangler;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
