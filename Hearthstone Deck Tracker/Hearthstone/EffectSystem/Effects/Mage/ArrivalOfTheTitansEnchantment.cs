using Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.EffectSystem.Effects.Mage;

// "Draw 2 spells. Refresh 6 Mana Crystals. You can only play spells for the rest of the turn."
public class ArrivalOfTheTitansEnchantment : EntityBasedEffect
{
	public override string CardId => HearthDb.CardIds.NonCollectible.Mage.ArrivaloftheTitans_TitansWatchEnchantment;
	protected override string CardIdToShowInUI => HearthDb.CardIds.Collectible.Mage.ArrivalOfTheTitans;

	public ArrivalOfTheTitansEnchantment(int entityId, bool isControlledByPlayer) : base(entityId, isControlledByPlayer)
	{
	}

	public override bool UniqueEffect => true;
	public override EffectDuration EffectDuration => EffectDuration.SameTurn;
	public override EffectTag EffectTag => EffectTag.CardLock;
}
