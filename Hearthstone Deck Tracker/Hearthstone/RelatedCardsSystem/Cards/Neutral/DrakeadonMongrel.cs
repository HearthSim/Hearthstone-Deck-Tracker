namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Summon two random 4-Cost minions."
public class DrakeadonMongrel : PilotedSkyGolem
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.DrakeadonMongrel;
	public override int EventCount() => 2;
}
