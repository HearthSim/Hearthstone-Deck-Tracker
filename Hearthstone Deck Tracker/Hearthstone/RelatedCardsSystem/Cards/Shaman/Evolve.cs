namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform your minions into random minions that cost (1) more."
public class Evolve : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Evolve;
	protected override int CostOffset => 1;
}
