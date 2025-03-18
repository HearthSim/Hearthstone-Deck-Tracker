using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Rogue;

public class FoxyFraudEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Rogue.FoxyFraud_EnablingEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Rogue.FoxyFraud;

	public FoxyFraudEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.SameTurn;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
