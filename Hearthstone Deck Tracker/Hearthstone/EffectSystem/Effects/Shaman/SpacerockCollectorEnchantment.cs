using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Shaman;

public class PlanetaryNavigatorEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Shaman.PlanetaryNavigator_PlanetaryNavigatorFutureBuffEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Shaman.PlanetaryNavigator;

	public PlanetaryNavigatorEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
