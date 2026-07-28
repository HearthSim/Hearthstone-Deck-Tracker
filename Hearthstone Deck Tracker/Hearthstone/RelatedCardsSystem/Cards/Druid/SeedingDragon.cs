namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Druid;

// "Taunt. Deathrattle: Get a random Dragon. It costs (2) less."
public class SeedingDragon : MerithraOfTheDream
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Druid.SeedingDragon;
}
