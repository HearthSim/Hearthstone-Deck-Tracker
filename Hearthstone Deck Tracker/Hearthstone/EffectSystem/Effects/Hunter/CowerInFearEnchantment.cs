using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Hunter;

public class CowerInFearEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Hunter.CowerinFear_CowerInFearPlayerEnchantEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Hunter.CowerInFear;

	public CowerInFearEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.SameTurn;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
