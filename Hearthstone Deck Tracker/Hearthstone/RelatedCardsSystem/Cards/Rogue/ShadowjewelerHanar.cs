using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Rogue;

// "After you play a Secret, Discover a Secret from a different class."
public class ShadowjewelerHanar : DiscoverPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.ShadowjewelerHanar;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.SPELL }
				&& !c.IsClass(playerClass) && !c.IsClass("Neutral") && c.HasTag(GameTag.SECRET))
			.Select(c => new Card(c));
	}
}

public class ShadowjewelerHanarCorePlaceholder : ShadowjewelerHanar
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Rogue.ShadowjewelerHanarCorePlaceholder;
}
