using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class CostumeMerchant : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.CostumeMerchant;

	protected readonly List<Card?> Masks = new List<Card?> {
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Druid.PantherMask),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Hunter.DevilsaurMask),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Mage.SheepMask),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Priest.BehemothMask),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Warlock.BatMask)
	};

	public bool ShouldShowForOpponent(Player opponent) => false;
	public List<Card?> GetRelatedCards(Player player) =>
		Masks;
}
