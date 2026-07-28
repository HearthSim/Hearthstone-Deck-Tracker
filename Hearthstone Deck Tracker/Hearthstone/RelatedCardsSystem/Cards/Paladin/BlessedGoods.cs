using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Paladin;

// "Discover a Secret, weapon, or Divine Shield minion."
public class BlessedGoods : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Paladin.BlessedGoods;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		var secrets = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral")) && c.HasTag(GameTag.SECRET))
			.Select(c => new Card(c));
		var weapons = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.WEAPON } && (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
		var divineShieldMinions = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION }
				&& (c.IsClass(playerClass) || c.IsClass("Neutral")) && c.HasTag(GameTag.DIVINE_SHIELD))
			.Select(c => new Card(c));
		return secrets.Concat(weapons).Concat(divineShieldMinions);
	}
}
