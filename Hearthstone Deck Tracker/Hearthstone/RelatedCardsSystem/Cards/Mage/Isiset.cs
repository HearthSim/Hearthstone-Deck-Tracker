using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: Summon two 10/10 Constructs. They are Frozen until you spend 15 Mana on spells."
public class Isiset : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Mage.Isiset;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		new List<Card?>
		{
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Mage.ColossusOfTheCold),
			Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Mage.ColossusOfTheCold),
		};
}
