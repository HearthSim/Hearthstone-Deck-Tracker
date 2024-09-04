using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Priest;

public class ShadowtouchedKvaldirEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.ShadowtouchedKvaldir_TwistedToTheCoreEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Priest.ShadowtouchedKvaldir;

	public ShadowtouchedKvaldirEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CardActivation;
}
