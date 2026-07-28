using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Neutral;

// "Battlecry: Get a random Colossal minion from the past."
public class PrimordialLord : FromThePastPoolCard
{
	public override string GetCardId() => HearthDb.CardIds.Collectible.Neutral.PrimordialLord;
	public override int Picks() => 1;
	public override bool IsWithReplacement() => true;

	protected override IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format)
	{
		return HearthDb.Cards.Collectible.Values
			.Where(c => c is { Type: CardType.MINION } && c.HasTag(GameTag.COLOSSAL))
			.Select(c => new Card(c));
	}

	// FilterGenerationPool drops COLOSSAL cards, which would empty this pool — the whole
	// pool is Colossal minions, so skip the deck-dependent gating entirely.
	protected override IEnumerable<Card> FilterPool(IEnumerable<Card> pool, List<Card> deck) => pool;
}
