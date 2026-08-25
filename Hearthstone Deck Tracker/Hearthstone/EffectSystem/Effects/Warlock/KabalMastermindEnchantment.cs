using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warlock;

public class KabalMastermindEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warlock.KabalMastermind_ImpRovedImpFormantsEnchantment;

	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.KabalMastermind;

	public KabalMastermindEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
