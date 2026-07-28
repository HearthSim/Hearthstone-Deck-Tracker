namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform all friendly minions into ones that cost (1) more. They summon the originals when they die."
public class Ascendance : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Ascendance;
	protected override int CostOffset => 1;
}
