using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Shaman;

// "Rush After this attacks, summon a Neutral Murloc."
public class CoilfangEliteToken : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Shaman.CoilfangEliteToken;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.IsMurloc() && c.IsClass("Neutral"))
			.Select(c => new Card(c));
	}
}
