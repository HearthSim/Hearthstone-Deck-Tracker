using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Paladin;

public class InventorsAuraEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Paladin.InventorsAura_EmpoweredWorkshopEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Paladin.InventorsAura;

	public InventorsAuraEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.MultipleTurns;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
