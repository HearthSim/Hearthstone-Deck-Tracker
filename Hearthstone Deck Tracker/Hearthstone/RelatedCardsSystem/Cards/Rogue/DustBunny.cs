using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class DustBunny: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.DustBunny;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new()
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.TheCoinCore),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.KoboldMiner_RockToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.KingMukla_BananasToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Rogue.WickedKnifeLegacy),
		};
}
