using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

public class MalorneTheWaywatcher: ICardWithRelatedCards
{
	protected readonly List<Card?> WildGods = new() {
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Deathknight.Ursoc),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Demonhunter.Omen),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Druid.ForestLordCenarius),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Hunter.Goldrinn),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Mage.Aessina),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Paladin.Ursol),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Priest.AvianaElunesChosen),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Rogue.Ashamane),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Shaman.Ohnahra),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Warlock.Agamaggan),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Warrior.Tortolla),
	};
	public string GetCardId() => HearthDb.CardIds.Collectible.Neutral.MalorneTheWaywatcher;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) => WildGods;
}
