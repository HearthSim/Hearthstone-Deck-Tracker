using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Druid;

public class RavenousFlockPlayerEnchantEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Druid.RavenousFlock_RavenousFlockPlayerEnchantEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Druid.RavenousFlock;

	public RavenousFlockPlayerEnchantEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.NextTurn;
	public override EffectTag EffectTag => EffectTag.Summon;
}
