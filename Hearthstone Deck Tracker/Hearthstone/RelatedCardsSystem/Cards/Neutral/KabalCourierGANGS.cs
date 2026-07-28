using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a Mage, Priest, or Warlock card."
public class KabalCourierGANGS : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.KabalCourierGANGS;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.IsClass("Mage") || c.IsClass("Priest") || c.IsClass("Warlock"))
			.Select(c => new Card(c));
	}
}

public class KabalCourierWONDERS : KabalCourierGANGS
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.KabalCourierWONDERS;
}
