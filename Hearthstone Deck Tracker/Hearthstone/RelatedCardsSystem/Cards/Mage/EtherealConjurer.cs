namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Discover a spell."
public class EtherealConjurer : RunedOrb
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.EtherealConjurer;
}

public class EtherealConjurerCorePlaceholder : EtherealConjurer
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.EtherealConjurerCorePlaceholder;
}
