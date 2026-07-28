namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public interface ICardWithRelatedCardsSummary : ICardWithRelatedCards
{
	/// <summary>
	/// Cards drawn per sampling event (the batch size per Discover/summon/cast invocation).
	/// 3 for a standard Discover, 1 for a random single-card generation effect.
	/// </summary>
	int Picks();

	/// <summary>
	/// Number of independent repetitions of the sampling event.
	/// Default is 1.  Override to 2 for "Discover 2 minions", 8 for "Cast 8 random spells", etc.
	/// </summary>
	int EventCount();

	/// <summary>
	/// When true, each draw within an event is independent (binomial/with-replacement model).
	/// Use for random summons and casts where the full pool is available for every draw.
	/// When false (default), an event draws <see cref="Picks"/> unique cards simultaneously
	/// without replacement (hypergeometric model), as in a standard Discover.
	/// </summary>
	bool IsWithReplacement();
}
