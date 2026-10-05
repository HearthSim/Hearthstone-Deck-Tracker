using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Priest;

// "Deal 3 damage to a random enemy. (The next Shadow spell you play repeats this!)"
public class ElderWordFlayEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Priest.ElderWordFlay_FlayEssenceEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Priest.ElderWordFlay;

	public ElderWordFlayEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CardActivation;
}
