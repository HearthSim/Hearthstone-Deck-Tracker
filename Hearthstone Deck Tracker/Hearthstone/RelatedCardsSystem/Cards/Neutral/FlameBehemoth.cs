using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Get two random Magnetic Mechs. They cost (2) less."
public class FlameBehemoth : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.FlameBehemoth;
	public override int Picks() => 1;
	public override int EventCount() => 2;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.HasRace(Race.MECHANICAL) && c.HasTag(GameTag.MODULAR))
			.Select(c => new Card(c));
	}
}
