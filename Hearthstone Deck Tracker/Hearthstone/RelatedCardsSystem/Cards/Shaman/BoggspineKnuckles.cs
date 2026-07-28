namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "After your hero attacks, transform your minions into random ones that cost (1) more."
public class BoggspineKnuckles : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.BoggspineKnuckles;
	protected override int CostOffset => 1;
}
