using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Add 5 random Beasts to your hand. They are Temporary."
public class WildernessPack : BeastMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.WildernessPack;
	public override int Picks() => 1;
	public override int EventCount() => 5;
	public override bool IsWithReplacement() => true;
}
