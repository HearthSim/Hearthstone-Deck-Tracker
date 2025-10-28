using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.DemonHunter;

public class TheEternalHoldEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Demonhunter.TheEternalHold_JailbreakEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Demonhunter.TheEternalHold;

	public TheEternalHoldEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;

	public override EffectTag EffectTag => EffectTag.CostModification;
}
