using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Priest;

public class AlexstraszaGuardianOfLifeEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.AlexstraszaGuardianofLife_CleansedOfCorruptionEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Priest.AlexstraszaGuardianOfLife;

	public AlexstraszaGuardianOfLifeEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.DamageModification;
}
