namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Whenever you cast a spell, summon a random minion of the same Cost."
// The triggering spell is a future cast; spells in hand are the proxy candidates.
public class SummoningStone : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.SummoningStone;
	protected override int CostOffset => 0;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.HandSpells;
	protected override bool AffectsAllTargets => false;
}
