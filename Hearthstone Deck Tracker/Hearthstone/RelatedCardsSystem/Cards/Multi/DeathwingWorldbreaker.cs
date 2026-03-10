using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Multi;

public class DeathwingWorldbreaker : ICardWithRelatedCards
{
	public string GetCardId() => HearthDb.CardIds.Collectible.Invalid.DeathwingWorldbreakerHeroic;

	private readonly List<Card?> _heroPowerEffects = new List<Card?> {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.DragonsReignToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.ToppleToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.RazeToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Neutral.EnthrallToken),
	};

	public bool ShouldShowForOpponent(Player opponent) => false;
	public List<Card?> GetRelatedCards(Player player) => _heroPowerEffects;
}
