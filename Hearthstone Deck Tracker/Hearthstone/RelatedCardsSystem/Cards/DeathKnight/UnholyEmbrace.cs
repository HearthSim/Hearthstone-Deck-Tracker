using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

// "Spend up to 10 Corpses and add an Unholy Rune card to your hand for each."
public class UnholyEmbrace : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Deathknight.UnholyEmbrace;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.IsClass(playerClass) && c.HasTag((GameTag)2198))
			.Select(c => new Card(c)); // Unholy rune
	}
}
