using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Hunter;

public class CelestialShotEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Hunter.CelestialShot_CelestialEmbraceEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Hunter.CelestialShot;

	public CelestialShotEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.SpellDamage;
}
