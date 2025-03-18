using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warlock;

public class FracturedPowerEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warlock.FracturedPower_DelayedManaEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.FracturedPower;

	public FracturedPowerEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => false;
	public override EffectTarget EffectTarget => EffectTarget.Self;
	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.ManaCrystalModification;
}
