using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Old God. Battlecry: End your turn. Yogg-saron takes an extra turn for you. (Once per game)"
public class MastermindYoggSaron : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.MastermindYoggSaron;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.MastermindYoggSaron_BrainOfYoggSaronToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.MastermindYoggSaron_MadnessOfYoggSaronToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.MastermindYoggSaron_AscentOfYoggSaronToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.MastermindYoggSaron_DomainOfYoggSaronToken),
		};
}
