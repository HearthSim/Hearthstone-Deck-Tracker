using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Add a random Warrior minion, spell, and weapon to your hand."
public class WarCacheCorePlaceholder : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.WarCacheCorePlaceholder;
	public override int Picks() => 1;
	public override int EventCount() => 3;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		var minions = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsClass("Warrior"))
			.Select(c => new Card(c));
		var spells = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL } && c.IsClass("Warrior"))
			.Select(c => new Card(c));
		var weapons = HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.WEAPON } && c.IsClass("Warrior"))
			.Select(c => new Card(c));
		return minions.Concat(spells).Concat(weapons);
	}
}

public class WarCacheLegacy : WarCacheCorePlaceholder
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.WarCacheLegacy;
}
