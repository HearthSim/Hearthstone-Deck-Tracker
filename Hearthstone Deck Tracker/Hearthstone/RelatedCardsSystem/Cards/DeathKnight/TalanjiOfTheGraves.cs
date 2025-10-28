using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class TalanjiOfTheGraves : ICardWithHighlight, ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.TalanjiOfTheGraves;

	public HighlightColor ShouldHighlight(Card card, IEnumerable<Card> deck) =>
		HighlightColorHelper.GetHighlightColor(card.Id == HearthDb.CardIds.NonCollectible.Deathknight.TalanjioftheGraves_BwonsamdiToken);

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => _boons;

	private readonly List<Card?> _boons = new List<Card?> {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TalanjioftheGraves_BoonOfPowerToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TalanjioftheGraves_BoonOfLongevityToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TalanjioftheGraves_BoonOfSpeedToken),
	};
}

public class WhatBefellZandalar : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.NonCollectible.Deathknight.TalanjioftheGraves_WhatBefellZandalarToken;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => _boons;

	private readonly List<Card?> _boons = new List<Card?> {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TalanjioftheGraves_BoonOfPowerToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TalanjioftheGraves_BoonOfLongevityToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TalanjioftheGraves_BoonOfSpeedToken),
	};
}
