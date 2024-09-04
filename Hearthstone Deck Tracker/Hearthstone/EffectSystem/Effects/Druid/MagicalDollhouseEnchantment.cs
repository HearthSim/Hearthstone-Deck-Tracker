using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Druid;

public class MagicalDollhouseEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Druid.MagicalDollhouse_MagicalHarvestEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Druid.MagicalDollhouse;


	public MagicalDollhouseEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.SameTurn;
	public override EffectTag EffectTag => EffectTag.SpellDamage;
}
