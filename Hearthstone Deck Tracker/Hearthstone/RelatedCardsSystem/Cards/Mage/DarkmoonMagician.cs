using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Elusive. After you cast a spell, cast a random spell that costs (1) more."
public class DarkmoonMagician : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.DarkmoonMagician;
	protected override int CostOffset => 1;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.HandSpells;
	protected override bool AffectsAllTargets => false;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.SPELL;
	protected override string PoolCacheKey => "spells";
}
