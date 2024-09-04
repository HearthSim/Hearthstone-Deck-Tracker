using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Shaman;

public class MelomaniaEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Shaman.Melomania_MelomaniaEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Shaman.Melomania;

	public MelomaniaEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.SameTurn;
	public override EffectTag EffectTag => EffectTag.CardAmount;
}
