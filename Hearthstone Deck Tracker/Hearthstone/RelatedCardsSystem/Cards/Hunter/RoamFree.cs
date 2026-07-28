namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Replace your future Animal Companions with random Beasts that cost (2) more. Choose one to summon."
// "Choose one" of the three new companions -> discover-style pick of 3 distinct cards.
public class RoamFree : AnimalCompanionUpgradeCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.RoamFree;
	protected override int CostOffset => 2;
	protected override int BatchSize => 3;
	protected override bool IsWithReplacement => false;
}
