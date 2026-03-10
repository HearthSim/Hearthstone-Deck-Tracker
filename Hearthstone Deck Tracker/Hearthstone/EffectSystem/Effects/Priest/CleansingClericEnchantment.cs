using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Priest;

public class CleansingClericEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Priest.CleansingCleric_FreeFromCorruptionEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Priest.CleansingCleric;

	public CleansingClericEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.HealingModification;
}
