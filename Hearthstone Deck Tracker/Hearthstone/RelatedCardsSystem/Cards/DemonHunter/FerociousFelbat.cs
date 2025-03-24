using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;

public class FerociousFelbat: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Demonhunter.FerociousFelbat;

	protected override bool FilterCard(Card card) => card is { Mechanics: not null } && card.Mechanics.Contains("Deathrattle") && card.Cost >= 5;

	protected override bool ResurrectsMultipleCards() => false;
}
