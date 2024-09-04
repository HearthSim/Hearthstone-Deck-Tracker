using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Hunter;

public class ThornmantleMusicianEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Hunter.ThornmantleMusician_ThornmantlesMuseEnchantment1;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Hunter.ThornmantleMusician;

	public ThornmantleMusicianEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
