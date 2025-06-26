using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.DeathKnight;

public class WaveOfTarEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Deathknight.WaveofTar_StuckEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Deathknight.WaveOfTar;

	public WaveOfTarEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectTarget EffectTarget => EffectTarget.Enemy;

	public override EffectDuration EffectDuration => EffectDuration.Conditional;
	public override EffectTag EffectTag => EffectTag.CostModification;
}
