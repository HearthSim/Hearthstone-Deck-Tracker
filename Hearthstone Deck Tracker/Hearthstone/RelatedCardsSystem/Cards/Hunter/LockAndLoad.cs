using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Each time you cast a spell this turn, get a random Hunter card."
public class LockAndLoad : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.LockAndLoadTGT;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.IsClass("Hunter"))
			.Select(c => new Card(c));
	}
}

public class LockAndLoadCorePlaceholder : LockAndLoad
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.LockAndLoadCorePlaceholder;
}

public class LockAndLoadWONDERS : LockAndLoad
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.LockAndLoadWONDERS;
}
