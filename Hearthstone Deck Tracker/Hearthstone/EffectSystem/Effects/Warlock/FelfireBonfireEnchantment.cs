using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warlock;

public class FelfireBonfireEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warlock.FelfireBonfire_AmpleOfferingEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.FelfireBonfire;

	public FelfireBonfireEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}


	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
