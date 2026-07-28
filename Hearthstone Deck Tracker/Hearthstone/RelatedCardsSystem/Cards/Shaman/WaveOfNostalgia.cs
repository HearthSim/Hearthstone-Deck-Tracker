using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Transform ALL minions into random Legendary ones from the past."
public class WaveOfNostalgia : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.WaveOfNostalgia;

	// Transforms an unpredictable number of minions (both boards); model as a single
	// representative draw, like other board-wide effects.
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Rarity: Rarity.LEGENDARY })
			.Select(c => new Card(c));
	}
}
