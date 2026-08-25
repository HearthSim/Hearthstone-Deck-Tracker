using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Destroy all minions. Each player gets a 3-Cost spell that resummons theirs."
public class SlimeEm : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.SlimeEm;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Priest.Slimeem_EctoplasmToken),
		};
}
