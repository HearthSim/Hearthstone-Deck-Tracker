using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.DemonHunter;

public class SigilOfSilenceEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.SigilofSilence_SigilEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Demonhunter.SigilOfSilence;

	public SigilOfSilenceEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => true;
	public override EffectTarget EffectTarget => EffectTarget.Enemy;
	public override EffectDuration EffectDuration =>EffectDuration.NextTurn;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
