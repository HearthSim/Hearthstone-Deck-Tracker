namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Add two random 1-Cost minions to your hand."
public class Ravencaller : GravelsnoutKnight
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Ravencaller;
	public override int EventCount() => 2;
}
