using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warrior;

public class BloodsailDeckhandsEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warrior.BloodsailDeckhand_ToArrrmsCoreEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warrior.BloodsailDeckhandLegacy;

	public BloodsailDeckhandsEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
