using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Deal 1 damage. Summon a random minion of that Cost."
public class ShimmerShot : StateValuePoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.ShimmerShot;

	protected override int? TargetCost(Player player, Entity? hoveredEntity)
	{
		var playerEntity = hoveredEntity?.IsControlledBy(Core.Game.Player.Id) ?? false ? Core.Game.PlayerEntity : Core.Game.OpponentEntity;
		var playerSpellDamage = playerEntity?.GetTag(GameTag.CURRENT_SPELLPOWER) ?? 0;
		var shimmerShotSpellDamage = hoveredEntity?.GetTag(GameTag.CURRENT_SPELLPOWER) ?? 0;
		var cost = 1 + playerSpellDamage + shimmerShotSpellDamage;
		return cost > 0 ? cost : 1;
	}
}
