using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

// "Summon a Voidlord." (non-collectible, one of Godfather Kazakus' trial options)
public class TonicOfTyranny : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Warlock.GodfatherKazakus_TonicOfTyrannyToken;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.Collectible.Warlock.Voidlord),
		};
}
