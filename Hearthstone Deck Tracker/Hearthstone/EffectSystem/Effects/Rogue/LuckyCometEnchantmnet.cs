using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Rogue;

public class LuckyComentEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Rogue.LuckyComet_ComboCometEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Rogue.LuckyComet;

	public LuckyComentEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
