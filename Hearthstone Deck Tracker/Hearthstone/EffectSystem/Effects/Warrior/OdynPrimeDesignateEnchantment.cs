using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warrior;

public class OdynPrimeDesignateEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Warrior.OdynPrimeDesignate_ImpenetrableEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warrior.OdynPrimeDesignate;

	public OdynPrimeDesignateEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.HeroModification;
}
