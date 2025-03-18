using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warlock;

public class AgamagganEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.Agamaggan_CorruptedThornsEnchantment1;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.Agamaggan;

	public AgamagganEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => true;
	public override EffectTarget EffectTarget => EffectTarget.Self;
	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
