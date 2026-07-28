namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Add a random 8-Cost minion to your hand. At the start of your turns, reduce its Cost by (1)."
public class Circadiamancer : ContainmentUnit
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Circadiamancer;
}
