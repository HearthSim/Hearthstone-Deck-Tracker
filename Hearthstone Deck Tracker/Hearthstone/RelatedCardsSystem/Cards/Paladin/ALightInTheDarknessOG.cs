using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Discover a Paladin minion. Give it +2/+2."
public class ALightInTheDarknessOG : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.ALightInTheDarknessOG;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsClass("Paladin"))
			.Select(c => new Card(c));
	}
}

public class ALightInTheDarknessWONDERS : ALightInTheDarknessOG
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.ALightInTheDarknessWONDERS;
}
