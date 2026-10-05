using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Summon two 2/3 Fanatics with Taunt."
public class ServantsOfCthun : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Neutral.CThunEyeoftheEmpire_ServantsOfCthunToken;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.CThunEyeoftheEmpire_FacelessFanaticToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.CThunEyeoftheEmpire_FacelessFanaticToken),
		};
}
