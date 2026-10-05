using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Mage;

// "Move all spells in your deck to the top. You can pick to draw from the top or bottom each turn."
public class DiscsOfNorgannonEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Mage.DiscsofNorgannon_ShiftTheDiscsEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Mage.DiscsOfNorgannon;

	public DiscsOfNorgannonEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => true;
	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.CardAmount;
}
