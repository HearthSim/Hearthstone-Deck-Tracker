using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Deathrattle: Add a random Lich King card to your hand."
public class Arfus : ICardWithRelatedCards
{
	protected readonly List<Card?> LichKingCards = new() {
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TheLichKing_DeathCoilToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TheLichKing_ObliterateToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TheLichKing_DeathGripToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TheLichKing_DeathAndDecayToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TheLichKing_AntiMagicShellToken2),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TheLichKing_DoomPactToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TheLichKing_ArmyOfTheFrozenThroneToken),
		Database.GetCardFromId(HearthDb.CardIds.NonCollectible.Deathknight.TheLichKing_FrostmourneToken),
	};

	public virtual string GetCardId() => HearthDb.CardIds.Collectible.Neutral.Arfus;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => LichKingCards;
}

public class ArfusCore : Arfus
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ArfusCorePlaceholder;
}
