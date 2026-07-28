using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Battlecry: If you've cast a spell last turn, Discover an Elemental."
public class Whirlweaver : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.Whirlweaver;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && (c.IsClass(playerClass) || c.IsClass("Neutral")) && c.IsElemental())
			.Select(c => new Card(c));
	}
}
