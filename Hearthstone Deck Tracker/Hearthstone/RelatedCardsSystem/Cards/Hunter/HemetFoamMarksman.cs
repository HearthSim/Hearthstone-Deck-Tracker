using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "After a friendly Beast dies, get a random Legendary Beast from the past. It costs (2) less."
public class HemetFoamMarksman : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.HemetFoamMarksman;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Rarity: Rarity.LEGENDARY } && c.IsBeast())
			.Select(c => new Card(c));
	}
}
