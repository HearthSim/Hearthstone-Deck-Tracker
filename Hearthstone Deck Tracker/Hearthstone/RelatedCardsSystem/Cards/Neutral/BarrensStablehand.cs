using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Summon a random Beast."
public class BarrensStablehand : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.BarrensStablehandLegacy;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsBeast())
			.Select(c => new Card(c));
	}
}

public class BarrensStablehandCore : BarrensStablehand
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.BarrensStablehandCorePlaceholder;
}
