using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Rogue;

public class ValeeraTheHollowEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Rogue.ValeeratheHollow_VeilOfShadowsEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Rogue.ValeeraTheHollowICECROWN;

	public ValeeraTheHollowEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.NextTurn;
	public override EffectTag EffectTag => EffectTag.HeroModification;
}
