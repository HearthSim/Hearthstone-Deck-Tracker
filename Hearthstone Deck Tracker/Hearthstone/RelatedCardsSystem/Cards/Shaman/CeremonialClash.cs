using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Summon a random 3, 2, and 1-Cost minion. Overload: (1)"
public class CeremonialClash : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.CeremonialClash;
	public override int Picks() => 1;
	public override int EventCount() => 3;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && (c.Cost == 1 || c.Cost == 2 || c.Cost == 3))
			.Select(c => new Card(c));
	}
}
