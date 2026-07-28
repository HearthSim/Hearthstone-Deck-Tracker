namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

/// <summary>
/// Encapsulates the sampling parameters that drive keyword-probability calculations for
/// a card's generation or Discover effect.
/// </summary>
public readonly struct PickConfig
{
	/// <summary>
	/// Cards drawn per sampling event (the batch size).
	/// <list type="bullet">
	/// <item>3 for a standard Discover — the player sees 3 unique cards simultaneously.</item>
	/// <item>1 for a random summon or random cast — one card per invocation.</item>
	/// </list>
	/// </summary>
	public int BatchSize { get; }

	/// <summary>
	/// Number of independent repetitions of the sampling event.
	/// <list type="bullet">
	/// <item>1 — single Discover, single summon.</item>
	/// <item>2 — "Discover 2 minions" (two sequential Discovers from the full pool).</item>
	/// <item>8 — "Cast 8 random spells" (eight independent casts).</item>
	/// </list>
	/// </summary>
	public int EventCount { get; }

	/// <summary>
	/// When <see langword="true"/>, every draw within an event samples the full pool
	/// independently (binomial model). Use for random summons and random casts where the
	/// same card can appear multiple times.
	/// <para/>
	/// When <see langword="false"/>, an event draws <see cref="BatchSize"/> unique cards
	/// without replacement (hypergeometric model), as in a Discover showing 3 distinct
	/// cards from the pool simultaneously.
	/// </summary>
	public bool IsWithReplacement { get; }

	public PickConfig(int batchSize, int eventCount, bool isWithReplacement)
	{
		BatchSize = batchSize;
		EventCount = eventCount;
		IsWithReplacement = isWithReplacement;
	}
}
