using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Spellburst: Cast a random spell of the same Cost."
// The triggering spell is a future cast; spells in hand are the proxy candidates.
public class EnchantedCauldron : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.EnchantedCauldron;
	protected override int CostOffset => 0;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.HandSpells;
	protected override bool AffectsAllTargets => false;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.SPELL;
	protected override string PoolCacheKey => "spells";
}
