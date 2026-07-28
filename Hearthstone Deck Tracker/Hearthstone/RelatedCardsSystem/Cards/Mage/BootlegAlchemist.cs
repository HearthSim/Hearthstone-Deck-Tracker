using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Choose a card in your hand. Transform it into a spell that costs (5) more (keeps its original Cost)."
public class BootlegAlchemist : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.BootlegAlchemist;
	protected override int CostOffset => 5;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.HandCards;
	protected override bool AffectsAllTargets => false;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.SPELL;
	protected override string PoolCacheKey => "spells";
}
