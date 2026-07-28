using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Battlecry: If your deck has no Neutral cards, add 5 random Paladin cards to your hand."
public class LightforgedCrusader : PaladinCardPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.LightforgedCrusader;
	public override int Picks() => 1;
	public override int EventCount() => 5;
	public override bool IsWithReplacement() => true;
}
