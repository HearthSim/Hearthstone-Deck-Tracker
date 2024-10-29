using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;

public class AceWayfinderEnchantment: EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.AceWayfinder_AceWayfinderFutureBuffEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.AceWayfinder;

	public AceWayfinderEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.MinionModification;

}
