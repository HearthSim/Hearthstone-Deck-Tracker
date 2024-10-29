using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warlock;

public class InfernalStratagemEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warlock.InfernalStratagem_StrategicInfernoEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.InfernalStratagem;

	public InfernalStratagemEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectTarget EffectTarget => EffectTarget.Self;

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
