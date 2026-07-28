namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Your first spell each turn costs (0). Battlecry: Discover a spell."
public class Kalecgos : RunedOrb
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.Kalecgos;
}

public class KalecgosCorePlaceholder : Kalecgos
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.KalecgosCorePlaceholder;
}
