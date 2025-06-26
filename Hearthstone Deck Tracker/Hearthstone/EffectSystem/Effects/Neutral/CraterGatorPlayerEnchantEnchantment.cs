using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;

public class CraterGatorPlayerEnchantEnchantment: EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.CraterGator_CraterGatorPlayerEnchantEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.CraterGator;

	public CraterGatorPlayerEnchantEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}
	
	public override EffectTarget EffectTarget => EffectTarget.Enemy;

	public override EffectDuration EffectDuration => EffectDuration.NextTurn;
	public override EffectTag EffectTag => EffectTag.HeroModification;

}
