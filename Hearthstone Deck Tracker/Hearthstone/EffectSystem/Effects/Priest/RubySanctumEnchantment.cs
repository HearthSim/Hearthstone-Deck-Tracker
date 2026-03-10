using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Priest;

public class RubySanctumEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.RubySanctum_LifebindEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Priest.RubySanctum;

	public RubySanctumEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.HealingModification;
}
