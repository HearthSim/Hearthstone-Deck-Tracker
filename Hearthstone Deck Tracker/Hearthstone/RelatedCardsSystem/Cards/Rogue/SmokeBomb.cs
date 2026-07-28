using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Discover a Combo, Battlecry, or Stealth minion with a Dark Gift."
public class SmokeBomb : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.SmokeBomb;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral"))
				&& (c.HasTag(GameTag.COMBO) || c.HasTag(GameTag.BATTLECRY) || c.HasTag(GameTag.STEALTH)))
			.Select(c => new Card(c));
	}
}
