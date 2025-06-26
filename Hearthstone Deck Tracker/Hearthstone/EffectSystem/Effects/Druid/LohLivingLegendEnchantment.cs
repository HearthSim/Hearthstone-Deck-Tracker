using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Druid;

public class LohLivingLegendEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Druid.LohtheLivingLegend_LivingLegendEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Druid.LohTheLivingLegend;

	public LohLivingLegendEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
