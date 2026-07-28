using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Battlecry: Spend a Corpse to Discover a Blood Rune card."
public class Hematurge : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.Hematurge;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.IsClass(playerClass) && c.HasTag((GameTag)2196))
			.Select(c => new Card(c));
	}
}

public class HematurgeCore : Hematurge
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.HematurgeCore;
}
