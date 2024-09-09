using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Rogue;

public class ConcealEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Rogue.Conceal_ConcealedEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Rogue.Conceal;

	public ConcealEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => true;
	public override EffectDuration EffectDuration => EffectDuration.NextTurn;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
