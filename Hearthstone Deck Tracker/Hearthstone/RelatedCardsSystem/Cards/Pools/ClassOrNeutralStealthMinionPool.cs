using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Pools;

public abstract class ClassOrNeutralStealthMinionPool : DiscoverPoolCard
{
	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral"))
				&& c.HasTag(GameTag.STEALTH))
			.Select(c => new Card(c));
	}
}
