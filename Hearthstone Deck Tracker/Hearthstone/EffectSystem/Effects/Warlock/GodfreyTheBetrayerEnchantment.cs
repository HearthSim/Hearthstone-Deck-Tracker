using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Warlock;

public class GodfreyTheBetrayerEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Neutral.GodfreytheBetrayer_GodfreysAtlasEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Warlock.GodfreyTheBetrayer;

	public GodfreyTheBetrayerEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override EffectDuration EffectDuration => EffectDuration.Permanent;
	public override EffectTag EffectTag => EffectTag.CardAmount;
}
