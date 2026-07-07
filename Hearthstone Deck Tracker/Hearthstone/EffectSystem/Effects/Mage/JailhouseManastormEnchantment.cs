using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Mage;

public class JailhouseManastormEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Mage.JailhouseManastorm_ManastormSummoningEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Mage.JailhouseManastorm;

	public JailhouseManastormEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.Summon;
}
