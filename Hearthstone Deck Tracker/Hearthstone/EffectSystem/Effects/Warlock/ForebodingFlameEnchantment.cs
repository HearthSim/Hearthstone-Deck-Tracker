using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warlock;

public class ForebodingFlameEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warlock.ForebodingFlame_BurningLegionsBoonEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.ForebodingFlame;

	public ForebodingFlameEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectTarget EffectTarget => EffectTarget.Self;

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
