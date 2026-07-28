using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Add two random spells from other classes that cost (5) or more to your hand."
public class Jackpot : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.Jackpot;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL } && c.Cost >= 5 && !c.IsClass(playerClass) && !c.IsClass("Neutral"))
			.Select(c => new Card(c));
	}
}

public class JackpotCore : Jackpot
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.JackpotCore;
}
