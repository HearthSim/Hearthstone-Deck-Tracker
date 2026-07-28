using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Discover a Legendary Beast from any class to gain its stats. Deathrattle: Summon it."
public class BeastSpeakerTaka : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.BeastSpeakerTaka;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Rarity: Rarity.LEGENDARY } && c.IsBeast())
			.Select(c => new Card(c));
	}
}
