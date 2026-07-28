using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Battlecry: If a friendly Undead died after your last turn, Discover an Unholy Rune card."
public class NecroticMortician : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.NecroticMortician;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.IsClass(playerClass) && c.HasTag((GameTag)2198))
			.Select(c => new Card(c));
	}
}

public class NecroticMorticianCore : NecroticMortician
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.NecroticMorticianCore;
}
