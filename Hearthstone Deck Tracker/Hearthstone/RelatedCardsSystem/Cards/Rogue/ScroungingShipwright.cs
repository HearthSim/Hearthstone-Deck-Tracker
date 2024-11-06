using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

public class ScroungingShipwright: ICardWithRelatedCards
{
	private readonly List<Card?> _starshipPieces = new List<Card?> {
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Rogue.TheGravitationalDisplacer),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Demonhunter.ShattershardTurret),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Demonhunter.FelfusedBattery),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Druid.ShatariCloakfield),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Druid.StarlightReactor),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Deathknight.GuidingFigure),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Deathknight.SoulboundSpire),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Warlock.FelfireThrusters),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Warlock.HeartOfTheLegion),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Hunter.Biopod),
		Database.GetCardFromId(HearthDb.CardIds.Collectible.Hunter.SpecimenClaw),
	};

	public string GetCardId() => HearthDb.CardIds.Collectible.Rogue.ScroungingShipwright;

	public bool ShouldShowForOpponent(Player opponent) => false;

	public List<Card?> GetRelatedCards(Player player) =>
		_starshipPieces.Where(card => card != null && !card.IsClass(player.CurrentClass)).ToList();
}

