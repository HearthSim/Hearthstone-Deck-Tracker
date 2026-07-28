namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Rush. Deathrattle: Summon three random 1-Cost minions."
public class BuildingBlockGolem : GravelsnoutKnight
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.BuildingBlockGolem;
	public override int EventCount() => 3;
}
