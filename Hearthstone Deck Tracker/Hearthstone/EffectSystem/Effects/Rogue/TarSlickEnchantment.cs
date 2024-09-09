using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Rogue;

public class TarSlickEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.TarSlick_OilyEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Rogue.TarSlick;

	public TarSlickEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => true;
	public override EffectTarget EffectTarget => EffectTarget.Enemy;

	public override EffectDuration EffectDuration => EffectDuration.SameTurn;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
