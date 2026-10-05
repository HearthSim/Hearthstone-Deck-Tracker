using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;

// "After you play a neutral card, your next spell costs (1) less this turn."
public class MoguWarmasterEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.MoguWarmaster_WarShoutEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.MoguWarmaster;

	public MoguWarmasterEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.SameTurn;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
