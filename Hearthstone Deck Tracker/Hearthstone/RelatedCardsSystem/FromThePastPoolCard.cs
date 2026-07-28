using System.Linq;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

/// <summary>
/// Base class for "from the past" pools (TIME_TRAVEL-set mechanic): the pool is exactly
/// the cards that are NOT Standard-legal, i.e. the inverse of the default legality check.
/// The membership rule is format-independent — "the past" is the same card set whether
/// the game is Standard, Wild, or Twist — and matches the predicate the ICardGenerator
/// implementations for these cards already use.
/// <para/>
/// Do not share a GetCardPool across the past/present boundary: the shared base-pool
/// cache is keyed by the pool's declaring type and does not know about this legality
/// override, so a present-day card inheriting a past pool (or vice versa) would poison
/// the cache. Past pools always declare their own GetCardPool.
/// </summary>
public abstract class FromThePastPoolCard : DiscoverPoolCard
{
	protected override bool IsInLegalPool(Card card, GameType gt, FormatType format) =>
		Helper.WildOnlySets.Contains(card.Set);
}
