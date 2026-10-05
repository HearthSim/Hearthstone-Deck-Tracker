using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Neutral;

// "Mastermind Yogg-Saron ability: Your next spell this turn casts twice."
public class BrainOfYoggSaronEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Shaman.MastermindYoggSaron_BrainyToken;
	protected override string CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.MastermindYoggSaron_BrainOfYoggSaronToken;

	public BrainOfYoggSaronEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.SameTurn;
	public override EffectTag EffectTag => EffectTag.CardActivation;
}
