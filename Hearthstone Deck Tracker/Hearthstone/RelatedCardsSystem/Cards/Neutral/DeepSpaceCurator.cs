namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Spellburst: Get a random minion of the spell's Cost. Set its Cost to (0)."
// The triggering spell is a future cast; spells in hand are the proxy candidates.
public class DeepSpaceCurator : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.DeepSpaceCurator;
	protected override int CostOffset => 0;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.HandSpells;
	protected override bool AffectsAllTargets => false;
}
