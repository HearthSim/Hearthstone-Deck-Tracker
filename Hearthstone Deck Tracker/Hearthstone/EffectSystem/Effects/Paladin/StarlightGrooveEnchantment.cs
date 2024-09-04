using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Paladin;

public class StarlightGrooveEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Paladin.StarlightGroove_FeelingTheGrooveEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Paladin.StarlightGroove;

	public StarlightGrooveEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.HeroModification;
}
