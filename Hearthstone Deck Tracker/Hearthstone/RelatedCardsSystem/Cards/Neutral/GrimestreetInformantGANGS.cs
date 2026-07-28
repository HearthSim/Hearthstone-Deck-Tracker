using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a Hunter, Paladin, or Warrior card."
public class GrimestreetInformantGANGS : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.GrimestreetInformantGANGS;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.IsClass("Hunter") || c.IsClass("Paladin") || c.IsClass("Warrior"))
			.Select(c => new Card(c));
	}
}

public class GrimestreetInformantWONDERS : GrimestreetInformantGANGS
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.GrimestreetInformantWONDERS;
}
