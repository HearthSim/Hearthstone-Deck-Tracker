using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

public class YrelBeaconOfHope: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Paladin.YrelBeaconOfHope;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Paladin.LibramOfWisdom),
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Paladin.LibramOfJustice),
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Paladin.LibramOfHope),
		};

}
