using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Battlecry: Discover a Beast, Secret, or weapon."
public class PackKodo : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.PackKodo;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		var beasts = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsBeast() && (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
		var secrets = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& c.HasTag(GameTag.SECRET) && (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
		var weapons = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.WEAPON } && (c.IsClass(playerClass) || c.IsClass("Neutral")))
			.Select(c => new Card(c));
		return beasts.Concat(secrets).Concat(weapons);
	}
}
