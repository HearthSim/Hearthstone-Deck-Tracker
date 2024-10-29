using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Rogue;

public class SpacerockCollectorEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Rogue.SpacerockCollector_RockCollectionEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Rogue.SpacerockCollector;

	public SpacerockCollectorEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
