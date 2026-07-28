using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

// "Summon a random 2-Cost minion. If you've Discovered this turn, summon a random 4-Cost minion instead."
public class UnearthedArtifacts : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Mage.UnearthedArtifacts;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.Cost is 2 or 4)
			.Select(c => new Card(c));
	}
}
