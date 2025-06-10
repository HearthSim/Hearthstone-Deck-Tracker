using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Warlock;

public class ImpKingRafaam: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.ImpKingRafaam;

	protected override bool FilterCard(Card card) => card.GetTag(GameTag.IMP) > 0;

	protected override bool ResurrectsMultipleCards() => true;
}

public class ImpKingRafaamInfused: ImpKingRafaam
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Warlock.ImpKingRafaam_ImpKingRafaamToken;
}

public class ImpKingRafaamCorePlaceholder: ImpKingRafaam
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Warlock.ImpKingRafaamCorePlaceholder;
}
