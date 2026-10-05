using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "At the start of your next turn, summon three 3/1 Shredclaws with Rush."
public class SigilOfHatching : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.SigilOfHatching;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.SigilofHatching_ShredclawToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.SigilofHatching_ShredclawToken),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Demonhunter.SigilofHatching_ShredclawToken),
		};
}
