using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;

public class AstrobiologistEnchantment: EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.Astrobiologist_AstrobiologistEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Neutral.Astrobiologist;

	public AstrobiologistEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.MinionModification;

}
