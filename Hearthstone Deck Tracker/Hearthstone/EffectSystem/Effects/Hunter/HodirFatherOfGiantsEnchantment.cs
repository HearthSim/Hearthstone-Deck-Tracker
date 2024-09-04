using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Hunter;

public class HodirFatherOfGiantsEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Hunter.HodirFatherofGiants_GiantizeEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Hunter.HodirFatherOfGiants;

	public HodirFatherOfGiantsEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
