using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class Shaladrassil: ICardWithRelatedCards
{
	protected readonly List<Card?> DreamCards = new() {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.Dream),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.NightmareExpert1),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.YseraAwakens),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.LaughingSister),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.EmeraldDrake),

		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.Shaladrassil_CorruptedDreamToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.Shaladrassil_CorruptedNightmareToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.Shaladrassil_CorruptedAwakeningToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.Shaladrassil_CorruptedLaughingSisterToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.DreamCards.Shaladrassil_CorruptedDrakeToken),
	};
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Shaladrassil;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => DreamCards;
}
