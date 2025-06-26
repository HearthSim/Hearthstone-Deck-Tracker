using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class GorishiStinger: ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.GorishiWasp_GorishiStingerToken;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.SilithidQueen_SilithidGrubToken),
		};
}
