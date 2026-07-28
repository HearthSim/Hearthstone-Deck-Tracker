namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: Transform adjacent minions into random minions that cost (1) more."
// Which minions end up adjacent depends on placement, so all friendly minions are
// treated as candidates and the summary averages over them.
public class BogstrokClacker : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.BogstrokClacker;
	protected override int CostOffset => 1;
	protected override bool AffectsAllTargets => false;
}
