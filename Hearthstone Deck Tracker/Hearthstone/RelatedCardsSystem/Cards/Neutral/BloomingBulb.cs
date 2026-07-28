using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Cast three random {0}-Cost spells. (Upgrades each turn!)" — Cultivating Sprite's token.
// The current spell cost is on the entity's TAG_SCRIPT_DATA_NUM_1; 0 or no entity means
// the base cost of 1. Three independent casts -> batch of 3 with replacement.
public class BloomingBulb : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.CultivatingSprite_BloomingBulbToken;
	protected override int BatchSize => 3;
	protected override bool IsInPool(Card card) => card.TypeEnum == CardType.SPELL;
	protected override string PoolCacheKey => "spells";

	protected override int? TargetCost(Player player, Entity? hoveredEntity)
	{
		var cost = hoveredEntity?.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1) ?? 0;
		return cost > 0 ? cost : 1;
	}
}
