using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Mage;

public class IngeniousArtificerEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Mage.IngeniousArtificer_IngeniousArtficerFutureBuffEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Mage.IngeniousArtificer;

	public IngeniousArtificerEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.ManaCrystalModification;
}
