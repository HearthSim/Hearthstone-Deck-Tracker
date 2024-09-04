using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Hunter;

public class ParrotSanctuaryEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.ParrotSanctuary_ParrotingEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Hunter.ParrotSanctuary;

	public ParrotSanctuaryEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
