using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;

public class StrandedSpacemanEnchantment: EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.StrandedSpaceman_StrandedSpacemanFutureBuffEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.StrandedSpaceman;

	public StrandedSpacemanEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.MinionModification;

}
