using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Summon a 5/5 Voidwing with Lifesteal. (The next Shadow spell you play repeats this!)"
public class ElderWordManifest : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Priest.ElderWordManifest;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Priest.ElderWordManifest_VoidwingToken),
		};
}
