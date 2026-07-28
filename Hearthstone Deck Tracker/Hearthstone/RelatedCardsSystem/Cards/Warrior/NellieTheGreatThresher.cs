using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Colossal +1. Battlecry: Discover 3 Pirates to crew Nellie's Ship!"
public class NellieTheGreatThresher : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.NellieTheGreatThresher;
	public override int Picks() => 3;
	public override int EventCount() => 3;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && (c.IsClass(playerClass) || c.IsClass("Neutral")) && c.IsPirate())
			.Select(c => new Card(c));
	}
}
