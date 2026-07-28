using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Get a random Naga and a random spell. They cost (2) less."
public class NaturalTalent : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Shaman.NaturalTalent;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		var nagas = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsNaga())
			.Select(c => new Card(c));
		var spells = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL })
			.Select(c => new Card(c));
		return nagas.Concat(spells);
	}
}
