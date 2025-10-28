using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warlock;

public class MurlocRafaamEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_MrgleermMrgloslgyToken;
	protected override string CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Warlock.TimethiefRafaam_MurlocRafaamToken;

	public MurlocRafaamEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
