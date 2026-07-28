using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: If you played an Elemental last turn, Discover any Elemental from the past."
public class ErodedSediment : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ErodedSediment;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.HasRace(Race.ELEMENTAL)
				&& (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
	}
}
