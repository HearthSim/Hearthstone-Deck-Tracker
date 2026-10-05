using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Battlecry: Discover any spell with Cost equal to this minion's Attack. It costs (0) this turn."
// The bucket is the hovered copy's current Attack; without an entity, the printed Attack.
public class ElementalOfMadness : StateValuePoolCard
{
	private const int PrintedAttack = 2;

	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.ElementalOfMadness;
	protected override int BatchSize => 3;
	protected override bool IsWithReplacement => false;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.SPELL;
	protected override string PoolCacheKey => "spells";

	protected override int? TargetCost(Player player, Entity? hoveredEntity) =>
		hoveredEntity != null && hoveredEntity.HasTag(GameTag.ATK) ? hoveredEntity.GetTag(GameTag.ATK) : PrintedAttack;
}
