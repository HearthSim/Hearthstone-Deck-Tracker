using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;

public class AzureOathstone: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.NonCollectible.Mage.AzureQueenSindragosa_AzureOathstoneToken;
	protected override bool FilterCard(Card card) => card.IsDragon();

	protected override bool ResurrectsMultipleCards() => true;
}
