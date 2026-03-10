using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warrior;

public class CommanderGeddonEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.CommanderGeddon_BarrenEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warrior.CommanderGeddon;

	public CommanderGeddonEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.HeroModification;
}
