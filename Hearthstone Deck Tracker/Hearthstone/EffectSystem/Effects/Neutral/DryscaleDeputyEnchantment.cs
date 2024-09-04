using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;

public class DryscaleDeputyEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.DryscaleDeputy_DryscaleDeputyEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.DryscaleDeputy;

	public DryscaleDeputyEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CardAmount;
}
