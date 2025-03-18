using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Druid;

public class DreamboundDiscipleEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Druid.DreamboundDisciple_DreamboundEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Druid.DreamboundDisciple;

	public DreamboundDiscipleEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.HeroModification;
}
