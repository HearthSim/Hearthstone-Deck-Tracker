using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;

public class SpacePirateEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.SpacePirate_SpacePiracyEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.SpacePirate;

	public SpacePirateEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectTarget EffectTarget => EffectTarget.Self;
	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
