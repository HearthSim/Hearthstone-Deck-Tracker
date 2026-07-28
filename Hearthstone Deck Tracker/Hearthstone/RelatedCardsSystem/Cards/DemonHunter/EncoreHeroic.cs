using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

// "Get a random Outcast card."
public class EncoreHeroic : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Demonhunter.EncoreHeroic;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c.HasTag(GameTag.OUTCAST))
			.Select(c => new Card(c));
	}
}
