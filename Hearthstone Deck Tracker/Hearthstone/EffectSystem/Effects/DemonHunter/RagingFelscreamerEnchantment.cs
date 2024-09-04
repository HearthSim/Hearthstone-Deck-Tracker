using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.DemonHunter;

public class RagingFelscreamerEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Demonhunter.RagingFelscreamer_FelscreamEnchantmentDEMON_HUNTER_INITIATE1;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Demonhunter.RagingFelscreamerCore;

	public RagingFelscreamerEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
