using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;

// "Summon a random 5-Cost Beast. Shuffle 2 spells into your deck that do it again when drawn."
public class BeastTripwire : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Hunter.BeastTripwire;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION, Cost: 5 } && c.IsBeast())
			.Select(c => new Card(c));
	}
}

// "Casts When Drawn Summon a random 5-Cost Beast."
public class BeastTripwireToken : BeastTripwire
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Hunter.BeastTripwire_TrippedBeastTripwireToken;
}
