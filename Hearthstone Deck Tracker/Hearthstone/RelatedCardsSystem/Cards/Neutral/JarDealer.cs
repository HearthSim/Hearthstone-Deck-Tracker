namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Add a random 1-Cost minion to your hand."
public class JarDealer : GravelsnoutKnight
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.JarDealer;
}
