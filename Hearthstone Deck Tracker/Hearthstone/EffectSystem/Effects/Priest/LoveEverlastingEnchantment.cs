using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Priest;

public class LoveEverlastingEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Priest.LoveEverlasting_EverlastingLoveEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Priest.LoveEverlasting;

	public LoveEverlastingEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.MultipleTurns;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
