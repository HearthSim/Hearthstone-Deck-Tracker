namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: After you cast a spell this game, summon a random minion of the same Cost."
// The triggering spell is a future cast; spells in hand are the proxy candidates.
public class JailhouseManastorm : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.JailhouseManastorm;
	protected override int CostOffset => 0;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.HandSpells;
	protected override bool AffectsAllTargets => false;
}
