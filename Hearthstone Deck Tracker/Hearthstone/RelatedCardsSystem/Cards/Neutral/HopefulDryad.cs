using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class HopefulDryad: ICardWithRelatedCards
{
	protected readonly List<Card?> DreamCards = new() {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.Dream),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.NightmareExpert1),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.YseraAwakens),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.LaughingSister),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.EmeraldDrake),
	};
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.HopefulDryad;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => DreamCards;
}
