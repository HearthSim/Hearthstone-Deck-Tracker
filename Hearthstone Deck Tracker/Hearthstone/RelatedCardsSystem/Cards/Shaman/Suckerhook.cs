using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "At the end of your turn, transform your weapon into one that costs (1) more."
public class Suckerhook : RelativeCostPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.Suckerhook;
	protected override int CostOffset => 1;
	protected override RelativeCostTargetSource TargetSource => RelativeCostTargetSource.FriendlyWeapon;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.WEAPON;
	protected override string PoolCacheKey => "weapons";
}
