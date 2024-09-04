using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Paladin;

public class ResistanceAuraEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Paladin.ResistanceAura_ResistanceCoreEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Paladin.ResistanceAuraCore;

	public ResistanceAuraEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectTarget EffectTarget => EffectTarget.Enemy;
	public override EffectDuration EffectDuration => EffectDuration.MultipleTurns;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
