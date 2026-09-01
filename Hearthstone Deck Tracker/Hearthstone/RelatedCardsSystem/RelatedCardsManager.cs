using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;

public class RelatedCardsManager
{
	public const int LargePoolThreshold = 20;

	private Dictionary<string, ICardWithRelatedCards>? _relatedCards;
	private Dictionary<string, ICardWithHighlight>? _highlightCards;
	private Dictionary<string, ISpellSchoolTutor>? _spellSchoolTutorCards;
	private Dictionary<string, ICardGenerator>? _cardGeneratorCards;

	public Dictionary<string, ICardWithRelatedCards> RelatedCards => _relatedCards ??= InitializeRelatedCards();
	public Dictionary<string, ICardWithHighlight> HighlightCards  => _highlightCards ??= InitializeHighlightCards();
	public Dictionary<string, ISpellSchoolTutor> SpellSchoolTutorCards  => _spellSchoolTutorCards ??= InitializeSpellSchoolTutorCards();
	public Dictionary<string, ICardGenerator> CardGeneratorCards  => _cardGeneratorCards ??= InitializeCardGeneratorCards();

	public static Dictionary<string, HashSet<string>>? RelatedCardsSummaryKeywords;

	private Dictionary<string, ICardWithRelatedCards> InitializeRelatedCards()
	{
		var (relatedCardsDict, highlightCardsDict, spellSchoolTutorCardsDict, generatorsDict ) = InitializeCards();
		_highlightCards = highlightCardsDict;
		_spellSchoolTutorCards = spellSchoolTutorCardsDict;
		_cardGeneratorCards = generatorsDict;
		return relatedCardsDict;
	}

	private Dictionary<string, ICardWithHighlight> InitializeHighlightCards()
	{
		var (relatedCardsDict, highlightCardsDict, spellSchoolTutorCardsDict, generatorsDict ) = InitializeCards();
		_relatedCards = relatedCardsDict;
		_spellSchoolTutorCards = spellSchoolTutorCardsDict;
		_cardGeneratorCards = generatorsDict;
		return highlightCardsDict;
	}

	private Dictionary<string, ISpellSchoolTutor> InitializeSpellSchoolTutorCards()
	{
		var (relatedCardsDict, highlightCardsDict, spellSchoolTutorCardsDict, generatorsDict ) = InitializeCards();
		_relatedCards = relatedCardsDict;
		_highlightCards = highlightCardsDict;
		_cardGeneratorCards = generatorsDict;
		return spellSchoolTutorCardsDict;
	}

	private Dictionary<string, ICardGenerator> InitializeCardGeneratorCards()
	{
		var (relatedCardsDict, highlightCardsDict, spellSchoolTutorCardsDict, generatorsDict ) = InitializeCards();
		_relatedCards = relatedCardsDict;
		_highlightCards = highlightCardsDict;
		_spellSchoolTutorCards = spellSchoolTutorCardsDict;
		return generatorsDict;
	}

	private (
		Dictionary<string, ICardWithRelatedCards>,
		Dictionary<string, ICardWithHighlight>,
		Dictionary<string, ISpellSchoolTutor>,
		Dictionary<string, ICardGenerator> )
		InitializeCards()
	{
		System.Type[] allTypes;
		try
		{
			allTypes = Assembly.GetAssembly(typeof(ICard)).GetTypes();
		}
		catch(ReflectionTypeLoadException ex)
		{
			allTypes = ex.Types.Where(t => t != null).ToArray();
		}
		var cards = allTypes.Where(t => t.IsClass && !t.IsAbstract && typeof(ICard).IsAssignableFrom(t));

		var relatedCardsDict = new Dictionary<string, ICardWithRelatedCards>();
		var highlightCardsDict = new Dictionary<string, ICardWithHighlight>();
		var spellSchoolTutorCardsDict = new Dictionary<string, ISpellSchoolTutor>();
		var cardGeneratorCardsDict = new Dictionary<string, ICardGenerator>();


		foreach(var card in cards)
		{
			var cardInstance = Activator.CreateInstance(card) as ICard;

			if(cardInstance is ICardWithRelatedCards relatedCard)
			{
				relatedCardsDict[relatedCard.GetCardId()] = relatedCard;
			}

			if(cardInstance is ICardWithHighlight highlightCard)
			{
				highlightCardsDict[highlightCard.GetCardId()] = highlightCard;
			}

			if(cardInstance is ISpellSchoolTutor tutor)
			{
				spellSchoolTutorCardsDict[tutor.GetCardId()] = tutor;
			}

			if(cardInstance is ICardGenerator generator)
			{
				cardGeneratorCardsDict[generator.GetCardId()] = generator;
			}
		}

		return (relatedCardsDict, highlightCardsDict, spellSchoolTutorCardsDict, cardGeneratorCardsDict);
	}

	public ICardWithHighlight? GetCardWithHighlight(string cardId)
	{
		return HighlightCards.TryGetValue(cardId, out var card) ? card : null;
	}

	public ICardWithRelatedCards? GetCardWithRelatedCards(string cardId)
	{
		return RelatedCards.TryGetValue(cardId, out var card) ? card : null;
	}

	public ISpellSchoolTutor? GetSpellSchoolTutor(string cardId)
	{
		return SpellSchoolTutorCards.TryGetValue(cardId, out var card) ? card : null;
	}

	public IEnumerable<Card> GetCardsOpponentMayHave(Player opponent, GameType gameType, FormatType format)
	{
		return RelatedCards.Values.Where(card => card.ShouldShowForOpponent(opponent) && card.IsCardLegal(gameType, format))
			.Select(card =>
			{
				var c =  Database.GetCardFromId(card.GetCardId());
				if(c != null)
				{
					// Used for related cards tooltip
					c.ControllerPlayer = opponent;
				}
				return c;
			}).WhereNotNull();
	}

	public static int TryGetRelatedCardsSummary(
		List<Card?> relatedCards,
		PickConfig pickConfig,
		out Dictionary<string, string>? result,
		out PoolStatistics? statistics,
		bool usePercentages = true)
	{
		result = null;
		statistics = null;

		// Small pools still get a summary — the tooltip shows the card grid and the
		// statistics side by side when the pool has LargePoolThreshold cards or fewer.
		if(relatedCards.Count == 0)
			return 0;

		// Pre-size to input count — actual filled count tracked separately.
		var costValues   = new int[relatedCards.Count];
		var attackValues = new int[relatedCards.Count];
		var healthValues = new int[relatedCards.Count];
		int cardCount = 0, minionCount = 0;

		// Distribution bar charts render for every tier. The keyword listing is a
		// premium feature: RelatedCardsSummaryKeywords is only populated for premium
		// users, so free users still compute medians/bars but never a keyword summary.
		var keywords = RelatedCardsSummaryKeywords;
		var helper = keywords != null ? new Dictionary<string, int>() : null;

		foreach(var card in relatedCards)
		{
			if(card == null) continue;
			costValues[cardCount++] = card.Cost;
			if(card.TypeEnum != CardType.SPELL)
			{
				attackValues[minionCount]   = card.Attack;
				healthValues[minionCount++] = card.Health;
			}
			if(keywords != null)
				foreach(var kvp in keywords)
					if(kvp.Value.Contains(card.Id))
						helper![kvp.Key] = helper.TryGetValue(kvp.Key, out var cnt) ? cnt + 1 : 1;
		}

		if(cardCount == 0) return 0;

		if(helper is { Count: > 0 })
		{
			result = new Dictionary<string, string>();
			foreach(var kvp in helper.OrderByDescending(x => x.Value))
			{
				var displayText = usePercentages
					? FormatPercentage(CalculatePercentage(kvp.Value, cardCount, pickConfig))
					: $"{kvp.Value}";
				result[LocalizeKeywordName(kvp.Key)] = displayText;
			}
		}

		var medianCost   = CalculateMedian(costValues, cardCount);
		var medianAttack = minionCount > 0 ? (double?)CalculateMedian(attackValues, minionCount) : null;
		var medianHealth = minionCount > 0 ? (double?)CalculateMedian(healthValues, minionCount) : null;

		statistics = new PoolStatistics(
			FormatMedian(medianCost),
			medianAttack.HasValue ? FormatMedian(medianAttack.Value) : null,
			medianHealth.HasValue ? FormatMedian(medianHealth.Value) : null,
			BuildBars(costValues,   cardCount,   medianCost),
			medianAttack.HasValue ? BuildBars(attackValues, minionCount, medianAttack.Value) : null,
			medianHealth.HasValue ? BuildBars(healthValues, minionCount, medianHealth.Value) : null
		);

		return cardCount;
	}

	/// <summary>
	/// Summary for relative-cost (evolve/devolve) pools: each bucket is the slice of the
	/// pool at one resulting cost, drawn from <c>DrawCount</c> times (once per target that
	/// resolved to it). Bars and medians are computed over the union of the buckets —
	/// "what can I get" — while keyword percentages use the per-bucket mixture.
	/// </summary>
	/// <param name="affectsAllTargets">
	/// True when every target is transformed (Evolve): buckets combine as independent
	/// draws. False when exactly one unknown candidate is affected (Mutate, Bamboozle):
	/// the percentage is the draw-count-weighted average over the buckets.
	/// </param>
	public static int TryGetBucketedRelatedCardsSummary(
		IReadOnlyList<(List<Card> Pool, int DrawCount)> buckets,
		int batchSize,
		bool isWithReplacement,
		bool affectsAllTargets,
		out Dictionary<string, string>? result,
		out PoolStatistics? statistics,
		bool usePercentages = true)
	{
		result = null;
		statistics = null;

		var unionCount = 0;
		foreach(var (pool, _) in buckets)
			unionCount += pool.Count;
		if(unionCount == 0)
			return 0;

		var costValues   = new int[unionCount];
		var attackValues = new int[unionCount];
		var healthValues = new int[unionCount];
		int cardCount = 0, minionCount = 0;

		var keywords = RelatedCardsSummaryKeywords;
		var totalMatches = keywords != null ? new Dictionary<string, int>() : null;
		var bucketMatches = keywords != null ? new Dictionary<string, int[]>() : null;

		for(var b = 0; b < buckets.Count; b++)
		{
			foreach(var card in buckets[b].Pool)
			{
				if(card == null) continue;
				costValues[cardCount++] = card.Cost;
				if(card.TypeEnum != CardType.SPELL)
				{
					attackValues[minionCount]   = card.Attack;
					healthValues[minionCount++] = card.Health;
				}
				if(keywords != null)
					foreach(var kvp in keywords)
						if(kvp.Value.Contains(card.Id))
						{
							totalMatches![kvp.Key] = totalMatches.TryGetValue(kvp.Key, out var cnt) ? cnt + 1 : 1;
							if(!bucketMatches!.TryGetValue(kvp.Key, out var perBucket))
							{
								perBucket = new int[buckets.Count];
								bucketMatches[kvp.Key] = perBucket;
							}
							perBucket[b]++;
						}
			}
		}

		if(cardCount == 0) return 0;

		if(totalMatches is { Count: > 0 })
		{
			result = new Dictionary<string, string>();
			foreach(var kvp in totalMatches.OrderByDescending(x => x.Value))
			{
				var displayText = usePercentages
					? FormatPercentage(CalculateBucketedPercentage(bucketMatches![kvp.Key], buckets, batchSize, isWithReplacement, affectsAllTargets))
					: $"{kvp.Value}";
				result[LocalizeKeywordName(kvp.Key)] = displayText;
			}
		}

		var medianCost   = CalculateMedian(costValues, cardCount);
		var medianAttack = minionCount > 0 ? (double?)CalculateMedian(attackValues, minionCount) : null;
		var medianHealth = minionCount > 0 ? (double?)CalculateMedian(healthValues, minionCount) : null;

		statistics = new PoolStatistics(
			FormatMedian(medianCost),
			medianAttack.HasValue ? FormatMedian(medianAttack.Value) : null,
			medianHealth.HasValue ? FormatMedian(medianHealth.Value) : null,
			BuildBars(costValues,   cardCount,   medianCost),
			medianAttack.HasValue ? BuildBars(attackValues, minionCount, medianAttack.Value) : null,
			medianHealth.HasValue ? BuildBars(healthValues, minionCount, medianHealth.Value) : null
		);

		return cardCount;
	}

	public static string LocalizeKeywordName(string rawKeyword)
	{
		var key = $"TheOutfinder_Keyword_{rawKeyword}";
		var localized = LocUtil.Get(key, useCardLanguage: true);
		return string.IsNullOrEmpty(localized) ? rawKeyword : localized;
	}

	private static string FormatPercentage(double value) => value.ToString("0.#", LocUtil.Culture) + "%";

	private static string FormatMedian(double value) =>
		value == Math.Floor(value) ? ((int)value).ToString() : value.ToString("0.#", LocUtil.Culture);

	private static double CalculateMedian(int[] values, int count)
	{
		if(count == 0) return 0;
		var scratch = new int[count];
		Array.Copy(values, scratch, count);
		Array.Sort(scratch);
		var mid = count / 2;
		return count % 2 == 0 ? (scratch[mid - 1] + scratch[mid]) / 2.0 : scratch[mid];
	}

	private const int StandardCap = 7;  // catch-all for the normal 0-based window  → "7+"
	private const int HighCostCap  = 10; // catch-all for shifted high-cost windows  → "10+"
	private const double MaxBarHeight = 40;

	private static IReadOnlyList<StatBar> BuildBars(int[] values, int count, double median)
	{
		if(count == 0) return Array.Empty<StatBar>();

		var minVal = Math.Max(0, values[0]);
		var maxVal = Math.Max(0, values[0]);
		for(var i = 1; i < count; i++)
		{
			var v = Math.Max(0, values[i]);
			if(v < minVal) minVal = v;
			if(v > maxVal) maxVal = v;
		}

		// Standard window (0-based, "7+"): single-cost pools and pools with min=0.
		// Shifted window (minVal-based, "10+"): multi-cost high-cost pools so each
		// individual value gets its own bar instead of collapsing into a single spike.
		// The start is clamped to the cap: a pool whose cheapest/smallest card is already
		// past it (e.g. costs 12 and 20) has nothing left to spread out and collapses into
		// the single "10+" bucket, instead of producing an empty or negative window.
		var bucketStart = (minVal == maxVal || minVal < 1) ? 0 : Math.Min(minVal, HighCostCap);
		var cap = bucketStart == 0 ? StandardCap : HighCostCap;
		var bucketCount = cap - bucketStart + 1; // start=0→8, start=5→6, start=10→1

		var freq = new int[bucketCount];
		for(var i = 0; i < count; i++)
		{
			var offset = Math.Max(0, values[i]) - bucketStart;
			freq[offset < 0 ? 0 : offset < bucketCount - 1 ? offset : bucketCount - 1]++;
		}

		var maxCount = 0;
		for(var i = 0; i < bucketCount; i++)
			if(freq[i] > maxCount) maxCount = freq[i];

		if(maxCount == 0) return Array.Empty<StatBar>();

		var adjustedMedian = median - bucketStart;
		var medianBucket = adjustedMedian >= bucketCount - 1
			? bucketCount - 1
			: (int)Math.Round(Math.Max(0, adjustedMedian), MidpointRounding.AwayFromZero);

		var bars = new StatBar[bucketCount];
		for(var i = 0; i < bucketCount; i++)
		{
			var label = i < bucketCount - 1 ? (bucketStart + i).ToString() : $"{cap}+";
			bars[i] = new StatBar(label, MaxBarHeight * freq[i] / maxCount, i == medianBucket);
		}
		return bars;
	}

	/// <summary>
	/// Returns the probability (0–100) that at least one keyword match appears across all
	/// sampling events described by <paramref name="config"/>.
	///
	/// Two sampling models:
	///
	/// WithReplacement (binomial) — used for random summons/casts where every draw
	/// starts from the full pool:
	///   P(no match per event) = ((total − target) / total) ^ BatchSize
	///
	/// WithoutReplacement (hypergeometric) — used for Discover, where a batch of
	/// BatchSize unique cards is drawn simultaneously:
	///   P(no match per event) = ∏(i=0..BatchSize−1) (total−target−i) / (total−i)
	///
	/// Both paths then raise per-event probability to EventCount to account for
	/// multiple independent events, then apply the complement rule.
	/// </summary>
	private static float CalculatePercentage(int target, int total, PickConfig config)
	{
		if(total <= 0 || target <= 0)
			return 0f;
		if(target >= total)
			return 100f;

		var pNoMatchPerEvent = PerEventNoMatchProbability(target, total, config.BatchSize, config.IsWithReplacement);

		// Raise to EventCount: independent events each have the same per-event probability.
		var pNoMatchAllEvents = Math.Pow(pNoMatchPerEvent, config.EventCount);

		return (float)((1.0 - pNoMatchAllEvents) * 100.0);
	}

	private static double PerEventNoMatchProbability(int target, int total, int batchSize, bool isWithReplacement)
	{
		if(total <= 0 || target <= 0)
			return 1.0;
		if(target >= total)
			return 0.0;

		if(isWithReplacement)
		{
			// Binomial: each of BatchSize draws is independent from the full pool.
			// P(no match in one draw) = (total − target) / total
			// P(no match across all BatchSize draws) = that value ^ BatchSize
			return Math.Pow((double)(total - target) / total, batchSize);
		}

		// Hypergeometric: draw BatchSize unique cards without replacement.
		// P(no match) = C(total−target, BatchSize) / C(total, BatchSize)
		//             = ∏(i=0..BatchSize−1) (total−target−i) / (total−i)
		var pNoMatch = 1.0;
		for(int i = 0; i < batchSize; i++)
		{
			if(total - target - i <= 0)
			{
				// Remaining draws are guaranteed to hit a match.
				return 0.0;
			}
			pNoMatch *= (double)(total - target - i) / (total - i);
		}
		return pNoMatch;
	}

	/// <summary>
	/// Probability (0–100) that at least one keyword match appears across the bucket
	/// draws. Each bucket b contributes DrawCount_b independent events sampling only
	/// that bucket's pool.
	///
	/// AffectsAllTargets (Evolve — every target transforms):
	///   P(no match) = ∏_b PerEventNoMatch(b) ^ DrawCount_b
	///
	/// Single unknown target (Mutate, Bamboozle — exactly one candidate is affected,
	/// uniformly at random):
	///   P(match) = Σ_b (DrawCount_b / totalDraws) · (1 − PerEventNoMatch(b))
	/// </summary>
	private static float CalculateBucketedPercentage(
		int[] matchesPerBucket,
		IReadOnlyList<(List<Card> Pool, int DrawCount)> buckets,
		int batchSize,
		bool isWithReplacement,
		bool affectsAllTargets)
	{
		if(affectsAllTargets)
		{
			var pNoMatch = 1.0;
			for(var b = 0; b < buckets.Count; b++)
			{
				var perEvent = PerEventNoMatchProbability(matchesPerBucket[b], buckets[b].Pool.Count, batchSize, isWithReplacement);
				pNoMatch *= Math.Pow(perEvent, buckets[b].DrawCount);
			}
			return (float)((1.0 - pNoMatch) * 100.0);
		}

		var totalDraws = 0;
		for(var b = 0; b < buckets.Count; b++)
			totalDraws += buckets[b].DrawCount;
		if(totalDraws == 0)
			return 0f;

		var pMatch = 0.0;
		for(var b = 0; b < buckets.Count; b++)
		{
			var perEvent = PerEventNoMatchProbability(matchesPerBucket[b], buckets[b].Pool.Count, batchSize, isWithReplacement);
			pMatch += (double)buckets[b].DrawCount / totalDraws * (1.0 - perEvent);
		}
		return (float)(pMatch * 100.0);
	}

	public static async Task LoadRelatedCardsSummaryKeywords()
	{
		if(!OutfinderTrial.HasAccess)
		{
			RelatedCardsSummaryKeywords = null;
			return;
		}

		try
		{
			var keywords = await MakeRelatedCardsSummaryKeywordsRequest();
			if(keywords.Count == 0)
				return;

			RelatedCardsSummaryKeywords = keywords;
		}
		catch (Exception e)
		{
			Log.Error($"Error fetching Related Cards Summary Keywords: {e.Message}");
		}
	}

	private static async Task<Dictionary<string, HashSet<string>>> MakeRelatedCardsSummaryKeywordsRequest()
	{
		// A trial token is only set when riding a mulligan guide trial (ranked). Premium
		// users and arena trials have no token: the parameterless overload routes premium
		// to the authenticated endpoint and a free arena trial to the arena endpoint.
		var token = OutfinderTrial.Token;

		var raw = token != null
			? await ApiWrapper.GetDiscoverPoolKeywords(token)
			: await ApiWrapper.GetDiscoverPoolKeywords();

		if(raw == null)
			return new Dictionary<string, HashSet<string>>();

		return raw.ToDictionary(kvp => kvp.Key, kvp => new HashSet<string>(kvp.Value, StringComparer.Ordinal));
	}

}
