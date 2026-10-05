using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.DeathKnight;

// "Draw 2 minions. Refresh 5 Mana Crystals. You can only play minions for the rest of the turn."
public class ArrivalOfTheOldGodsEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Deathknight.ArrivaloftheOldGods_OldGodsWatchEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Deathknight.ArrivalOfTheOldGods;

	public ArrivalOfTheOldGodsEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => true;
	public override EffectDuration EffectDuration => EffectDuration.SameTurn;
	public override EffectTag EffectTag => EffectTag.CardLock;
}
