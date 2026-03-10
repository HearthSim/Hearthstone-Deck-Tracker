using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Paladin;

public class CrusaderAura : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.CrusaderAura_CrusaderAuraCoreEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Paladin.CrusaderAuraCore;

	public CrusaderAura(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.MultipleTurns;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
