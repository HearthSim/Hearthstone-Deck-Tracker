using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public abstract class DiscoverPoolCard : ICardWithRelatedCardsSummary
{
	// Shared across all instances whose GetCardPool implementation is declared by the same type.
	// Siblings that inherit the same override share a cache entry; classes that vary their pool
	// via abstract properties add a variant suffix via GetPoolCacheVariant().
	// Keyed "<declaring type FullName>:<variant>" → (GameType, FormatType, class): the
	// deck-independent part of the pipeline (full card-DB scan, Card allocation, legality filter,
	// cost sort) — the expensive part. GetCardPool only receives a class, so this is shared across
	// deck states. The remaining deck-dependent work (FilterGenerationPool + dedup) is a single
	// pass over the cached pool and is recomputed per call; caching it per deck state would grow
	// without bound as the deck changes during play.
	// The class in the key is the class the pool was actually built for, which is not always the
	// player's — see the card-class fallback in GetBasePool. Keying on the effective class is what
	// keeps that fallback from poisoning the entry for cards of a different class on the same pool.
	private static readonly Dictionary<string, Dictionary<(GameType, FormatType, string), List<Card>>> _sharedBasePoolCache = new();

	// Resolved once per instance via reflection; cached to avoid repeated reflection calls.
	private string? _poolCacheKey;
	private string PoolCacheKey => _poolCacheKey ??= $"{ResolvePoolDeclaringTypeName()}:{GetPoolCacheVariant()}";

	private string ResolvePoolDeclaringTypeName()
	{
		var type = GetType();
		while(type != null && type != typeof(DiscoverPoolCard))
		{
			if(type.GetMethod("GetCardPool", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null)
				return type.FullName!;
			type = type.BaseType;
		}
		return GetType().FullName!;
	}

	// Override in subclasses whose GetCardPool outcome varies based on an abstract/virtual property
	// (e.g. a cost discriminator), so that sibling classes with different property values don't
	// collide in the shared pool cache.
	protected virtual string GetPoolCacheVariant() => "";

	// The class the game falls back to when the player's class yields an empty pool. Resolved once
	// per instance from the card itself.
	private bool _fallbackClassResolved;
	private string? _fallbackClass;
	private string? FallbackClass
	{
		get
		{
			if(!_fallbackClassResolved)
			{
				_fallbackClassResolved = true;
				var classes = Database.GetCardFromId(GetCardId())?.GetClasses()
					.Where(c => c != "Neutral").ToList();
				_fallbackClass = classes is { Count: 1 } ? classes[0] : null;
			}
			return _fallbackClass;
		}
	}

	// Override to false for pools where an empty result is the right answer rather than a signal
	// that the player's class is the wrong lens.
	protected virtual bool UseCardClassFallback => true;

	public abstract string GetCardId();
	public virtual int Picks() => 3;
	public virtual int EventCount() => 1;
	public virtual bool IsWithReplacement() => false;
	public bool ShouldShowForOpponent(Player opponent) => false;

	protected abstract IEnumerable<Card> GetCardPool(string playerClass, GameType gt, FormatType format);

	protected virtual bool IsInLegalPool(Card card, GameType gt, FormatType format) => card.IsCardLegal(gt, format);

	protected virtual IEnumerable<Card> FilterPool(IEnumerable<Card> pool, List<Card> deck) => pool.FilterGenerationPool(deck);

	public List<Card?> GetRelatedCards(Player player)
	{
		var gt = PoolContext.GetGameType();
		var format = PoolContext.GetFormatType();
		var deck = player.IsLocalPlayer ? player.PlayerCardList : player.OpponentCardList;

		// Cards cannot generate themselves. Compared by name so reprints of the generator
		// (e.g. core vs expansion version) are excluded as well. This can't go into the base
		// pool cache, which is shared between different generators with the same pool.
		var selfName = Database.GetCardFromId(GetCardId())?.Name;

		// The base pool is pre-sorted by cost, and both steps below preserve order, so the
		// result comes out cost-sorted. Filtering must run before the dedup so that the
		// surviving copy of a name is always one the deck can actually generate.
		var result = FilterPool(GetBasePool(player, gt, format)
				.Where(c => c.Name != selfName), deck)
			// removes duplicated cards (for example core + expansion version)
			.GroupBy(c => c.Name)
			.Select(g => g.First())
			.ToList();

		return result!;
	}

	private List<Card> GetBasePool(Player player, GameType gt, FormatType format)
	{
		var playerClass = player.CurrentClass ?? "";
		var basePool = GetBasePoolForClass(playerClass, gt, format);
		if(basePool.Count > 0 || !UseCardClassFallback)
			return basePool;

		// The game never offers an empty Discover: when nothing matches the player's class it
		// draws from the card's own class instead — a Mage holding Hive Map (Demon Hunter,
		// "Discover a Fel spell") is still offered Demon Hunter Fel spells. Substituting the
		// class rather than appending the card's class cards keeps each pool's own class rule
		// intact, so "class + Neutral" pools stay "class + Neutral" around the substituted class.
		// Emptiness is judged after IsInLegalPool, which makes this format-aware: a class whose
		// only matching cards have rotated is empty in Standard and falls back there, but not in Wild.
		var fallbackClass = FallbackClass;
		if(fallbackClass == null || fallbackClass == playerClass)
			return basePool;

		return GetBasePoolForClass(fallbackClass, gt, format);
	}

	// The expensive part of the pipeline: scanning the full card DB and allocating a Card wrapper
	// per candidate. GetCardPool outcomes only vary by the class they are given (never the deck),
	// so this is cached per (GameType, FormatType, class) and reused across deck states — and the
	// fallback above resolves to the very entry a player of that class would build.
	private List<Card> GetBasePoolForClass(string playerClass, GameType gt, FormatType format)
	{
		if(!_sharedBasePoolCache.TryGetValue(PoolCacheKey, out var basePoolCache))
		{
			basePoolCache = new Dictionary<(GameType, FormatType, string), List<Card>>();
			_sharedBasePoolCache[PoolCacheKey] = basePoolCache;
		}

		if(!basePoolCache.TryGetValue((gt, format, playerClass), out var basePool))
		{
			basePool = GetCardPool(playerClass, gt, format)
				.Where(c => IsInLegalPool(c, gt, format))
				.OrderBy(c => c.Cost)
				.ToList();
			basePoolCache[(gt, format, playerClass)] = basePool;
		}

		return basePool;
	}
}
