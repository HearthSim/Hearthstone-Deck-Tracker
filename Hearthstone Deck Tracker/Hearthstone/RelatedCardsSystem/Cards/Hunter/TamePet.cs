namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Replace your future Animal Companions with random Beasts that cost (1) more. Draw a card."
public class TamePet : AnimalCompanionUpgradeCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.TamePet;
	protected override int CostOffset => 1;
}
