using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warrior;

public class PunchCardEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warrior.PunchCard_PunchedInEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warrior.PunchCard;

	public PunchCardEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.SameTurn;
	public override EffectTag EffectTag => EffectTag.HeroModification;
}
