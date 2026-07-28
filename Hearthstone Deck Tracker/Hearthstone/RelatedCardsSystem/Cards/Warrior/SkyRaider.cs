using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warrior;

// "Battlecry: Add a random Pirate to your hand."
public class SkyRaider : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.SkyRaider;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsPirate())
			.Select(c => new Card(c));
	}
}

public class SkyRaiderCore : SkyRaider
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warrior.SkyRaiderCore;
}
