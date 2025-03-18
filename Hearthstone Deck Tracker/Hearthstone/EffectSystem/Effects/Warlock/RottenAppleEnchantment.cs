using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warlock;

public class RottenAppleEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warlock.RottenApple_FractureEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.RottenApple;

	public RottenAppleEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => false;
	public override EffectTarget EffectTarget => EffectTarget.Self;
	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.ManaCrystalModification;
}
