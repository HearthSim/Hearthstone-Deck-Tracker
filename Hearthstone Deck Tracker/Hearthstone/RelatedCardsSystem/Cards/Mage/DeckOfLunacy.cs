using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Transform spells in your deck into ones that cost (3) more. (They keep their original Cost.)"
public class DeckOfLunacy : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.DeckOfLunacy;
	protected override int CostOffset => 3;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.DeckSpells;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.SPELL;
	protected override string PoolCacheKey => "spells";
}
