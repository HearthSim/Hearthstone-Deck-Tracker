using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;

public class WarlocEnchantment: EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.Warloc_DoomEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.Warloc;

	public WarlocEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
