using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Paladin;

public class HotSpringGliderEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Paladin.HotSpringGlider_WeeeeeEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Paladin.HotSpringGlider;

	public HotSpringGliderEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
