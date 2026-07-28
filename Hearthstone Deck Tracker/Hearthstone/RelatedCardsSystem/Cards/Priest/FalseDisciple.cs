using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Battlecry: Discover a Legendary Priest minion from the past."
public class FalseDisciple : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.FalseDisciple;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Rarity: Rarity.LEGENDARY } && c.IsClass("Priest"))
			.Select(c => new Card(c));
	}
}
