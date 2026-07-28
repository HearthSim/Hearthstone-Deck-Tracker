using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "Get two random 1-Cost spells from other classes."
public class PettyTheft : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.PettyTheft;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL, Cost: 1 } && !c.IsClass(playerClass) && !c.IsClass("Neutral"))
			.Select(c => new Card(c));
	}
}
