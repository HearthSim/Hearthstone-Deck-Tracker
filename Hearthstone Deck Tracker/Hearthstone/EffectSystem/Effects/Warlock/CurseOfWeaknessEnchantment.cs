using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warlock;

public class CurseOfWeaknessEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.CurseofWeakness_CurseOfWeaknessEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.CurseOfWeakness;

	public CurseOfWeaknessEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => true;
	public override EffectTarget EffectTarget => EffectTarget.Enemy;
	public override EffectDuration EffectDuration => EffectDuration.NextTurn;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
