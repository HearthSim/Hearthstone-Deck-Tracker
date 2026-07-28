using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Frenzy: Add a random spell from your class to your hand."
public class Peon : PlayerClassSpellPool
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Peon;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;
}
