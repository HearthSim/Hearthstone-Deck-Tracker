using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warrior;

public class ExpeditionSergeantEnchantment: EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warrior.ExpeditionSergeant_ExpeditionSergeantFutureBuffEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warrior.ExpeditionSergeant;

	public ExpeditionSergeantEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.MinionModification;

}
