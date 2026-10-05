using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;

// "Y'Shaarj ability: Your next card costs (5) less."
public class DomainOfYshaarjEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.YShaarjTitanBane_DomainOfYshaarjToken2;
	protected override string CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.YShaarjTitanBane_DomainOfYshaarjToken1;

	public DomainOfYshaarjEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
