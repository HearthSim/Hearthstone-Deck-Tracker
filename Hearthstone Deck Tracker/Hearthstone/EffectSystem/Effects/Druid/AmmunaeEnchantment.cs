using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Druid;

// "Battlecry: For the rest of the game, after you play a Deathrattle minion, summon a copy of it."
public class AmmunaeEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Druid.Ammunae_LifeConstructedEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Druid.Ammunae;

	public AmmunaeEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.Summon;
}
