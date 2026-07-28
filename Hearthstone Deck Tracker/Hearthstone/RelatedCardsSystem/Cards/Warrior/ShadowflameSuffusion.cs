using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Deal $2 damage. Discover a Warrior minion with a Dark Gift."
public class ShadowflameSuffusion : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.ShadowflameSuffusion;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsClass("Warrior"))
			.Select(c => new Card(c));
	}
}
