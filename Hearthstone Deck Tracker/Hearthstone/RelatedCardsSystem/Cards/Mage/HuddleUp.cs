using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Fill your board with random Naga."
public class HuddleUp : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.HuddleUp;
	public override int Picks() => 1;
	public override int EventCount() => BoardFill.PlayerSlots;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsNaga())
			.Select(c => new Card(c));
	}
}
