using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.DemonHunter;

public class GorishisFavorEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Demonhunter.UnleashtheColossus_GorishisFavorEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Demonhunter.UnleashtheColossus_GorishiColossusToken;

	public GorishisFavorEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.DamageModification;
}
