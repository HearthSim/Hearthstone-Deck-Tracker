namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Whenever you cast a spell, fill your board with random minions of that Cost."
// The triggering spell is a future cast; spells in hand are the proxy candidates. Casting one
// fills the empty board slots with minions of its cost, so each candidate draws BatchSize
// (available slots) times from its cost bucket.
public class LinaShopManager : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.LinaShopManager;
	protected override int CostOffset => 0;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.HandSpells;
	protected override bool AffectsAllTargets => false;
	protected override int BatchSize => BoardFill.PlayerSlots;
}
