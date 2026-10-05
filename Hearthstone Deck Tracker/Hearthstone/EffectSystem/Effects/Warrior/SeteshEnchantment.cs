using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warrior;

// "Battlecry: Gain 20 Armor. In 2 turns, convert your Armor into a Golem with that many stats and Taunt."
public class SeteshEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warrior.Setesh_SeteshTrackerEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warrior.Setesh;

	public SeteshEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.Summon;
}
