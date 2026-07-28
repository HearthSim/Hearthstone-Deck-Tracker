using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Transform all spells in your hand into ones that cost (3) more. (They keep their original Cost.)"
public class EnergyShaper : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.EnergyShaper;
	protected override int CostOffset => 3;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.HandSpells;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.SPELL;
	protected override string PoolCacheKey => "spells";
}
