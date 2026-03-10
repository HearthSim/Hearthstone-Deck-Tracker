using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Hunter;

public class EbyssianEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.Ebyssian_EbyssiansBlessingEnchantment1;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Hunter.Ebyssian;

	public EbyssianEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
