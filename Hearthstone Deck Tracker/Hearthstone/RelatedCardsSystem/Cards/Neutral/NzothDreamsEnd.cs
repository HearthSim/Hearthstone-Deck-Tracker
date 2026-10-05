using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Old God Cost (1) less for each Deathrattle minion you've summoned this game."
public class NzothDreamsEnd : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.NzothDreamsEnd;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.NZothDreamsEnd_CallOfNzothToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.NZothDreamsEnd_MasteryOfNzothToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.NZothDreamsEnd_ArmyOfNzothToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.NZothDreamsEnd_DomainOfNzothToken1),
		};
}
