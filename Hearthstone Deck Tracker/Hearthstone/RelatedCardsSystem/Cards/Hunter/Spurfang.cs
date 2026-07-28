using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Battlecry and Deathrattle: Summon a random Beast with Cost equal to this minion's Attack."
public class Spurfang : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.Spurfang;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.MINION && card.IsBeast();
	protected override string PoolCacheKey => "beasts";

	// In-hand hover reads the live Attack (buffs count); deck hover has no entity, so fall
	// back to the printed Attack.
	protected override int? TargetCost(Player player, Entity? hoveredEntity) =>
		hoveredEntity?.Attack ?? Database.GetCardFromId(GetCardId())?.Attack;
}
