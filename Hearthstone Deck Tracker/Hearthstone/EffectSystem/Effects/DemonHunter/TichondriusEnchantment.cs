using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.DemonHunter;

public class TichondriusEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Demonhunter.Tichondrius_DemonicSummoningEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Demonhunter.TichondriusCorePlaceholder;

	public TichondriusEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;

	public override EffectTag EffectTag => EffectTag.CostModification;
}
