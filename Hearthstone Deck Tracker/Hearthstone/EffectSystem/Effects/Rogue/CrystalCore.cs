using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Rogue;

public class CrystalCoreEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.TheCavernsBelow_CrystallizedTokenUNGORO1;
	protected override string CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Rogue.TheCavernsBelow_CrystalCoreTokenUNGORO;

	public CrystalCoreEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => true;
	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.MinionModification;
}
