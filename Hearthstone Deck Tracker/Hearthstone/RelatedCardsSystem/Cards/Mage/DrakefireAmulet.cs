namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Tradeable Discover 2 Dragons. Summon them."
public class DrakefireAmulet : AzureExplorer
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.DrakefireAmulet;
	public override int EventCount() => 2;
}
