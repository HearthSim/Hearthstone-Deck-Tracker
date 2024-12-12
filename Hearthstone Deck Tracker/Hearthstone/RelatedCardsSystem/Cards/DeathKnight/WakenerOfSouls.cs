using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DeathKnight;

public class WakenerOfSouls: ResurrectionCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Deathknight.WakenerOfSouls;

	protected override bool FilterCard(Card card) => card is { Mechanics: not null }
	                                                 && card.Id != HearthDb.CardIds.Collectible.Deathknight.WakenerOfSouls
	                                                 && card.Mechanics.Contains("Deathrattle");

	protected override bool ResurrectsMultipleCards() => throw new System.NotImplementedException();
}
