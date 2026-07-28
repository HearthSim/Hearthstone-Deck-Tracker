using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;

// "Discover a Dragon from the past with a Dark Gift."
// Past pools declare their own GetCardPool (never shared across the past/present
// boundary); the Dark Gift is a post-pick modifier, not a pool filter.
public class WingsOfEternity : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Priest.WingsOfEternity;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsDragon() && (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
	}
}
