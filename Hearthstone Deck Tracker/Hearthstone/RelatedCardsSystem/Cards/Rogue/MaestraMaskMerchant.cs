using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Warlock Tourist Battlecry: Discover a Hero card from the past (from another class)."
public class MaestraMaskMerchant : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.MaestraMaskMerchant;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.HERO }
				&& !c.IsClass(playerClass) && !c.IsClass("Neutral"))
			.Select(c => new Card(c));
	}
}
