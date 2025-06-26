using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warlock;

public class SpelunkerEnchantPlayerEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warlock.Spelunker_SpelunkerEnchantPlayerEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.Spelunker;

	public SpelunkerEnchantPlayerEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
