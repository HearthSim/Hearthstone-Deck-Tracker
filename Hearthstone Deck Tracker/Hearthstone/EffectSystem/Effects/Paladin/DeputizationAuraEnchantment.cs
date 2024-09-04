using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Paladin;

public class DeputizationAuraEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Paladin.DeputizationAura_MyFavoriteDeputyEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Paladin.DeputizationAura;

	public DeputizationAuraEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.MultipleTurns;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
