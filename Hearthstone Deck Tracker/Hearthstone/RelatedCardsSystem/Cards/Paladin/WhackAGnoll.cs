using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Discover a Paladin weapon from the past. Give it +1/+1."
public class WhackAGnoll : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.WhackAGnoll;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.WEAPON } && c.IsClass("Paladin"))
			.Select(c => new Card(c));
	}
}
