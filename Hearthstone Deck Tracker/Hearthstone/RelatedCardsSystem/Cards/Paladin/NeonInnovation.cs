using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Discover a Paladin Mech from the past. Give it +5/+5."
public class NeonInnovation : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.NeonInnovation;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsClass("Paladin") && c.IsMech())
			.Select(c => new Card(c));
	}
}
