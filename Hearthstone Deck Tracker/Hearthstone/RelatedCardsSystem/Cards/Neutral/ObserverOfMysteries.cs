using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Cast 2 random Secrets. At the start of your turn, destroy them."
public class ObserverOfMysteries : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.ObserverOfMysteries;

	// Two Secrets cast at once; Secrets in play must be unique, so model as one batch of
	// distinct draws (no replacement) rather than two independent events.
	public override int Picks() => 2;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL } && c.HasTag(GameTag.SECRET))
			.Select(c => new Card(c));
	}
}
