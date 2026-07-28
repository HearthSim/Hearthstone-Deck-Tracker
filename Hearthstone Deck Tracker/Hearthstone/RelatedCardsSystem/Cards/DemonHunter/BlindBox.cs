using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Get 2 random Demons. Outcast: Discover them instead."
public class BlindBox : DemonMinionPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.BlindBox;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
	public override int EventCount() => 2;
}
