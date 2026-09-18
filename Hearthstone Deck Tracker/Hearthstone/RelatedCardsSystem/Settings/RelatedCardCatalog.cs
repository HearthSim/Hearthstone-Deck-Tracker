using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Settings;

/// <summary>
/// Everything the related cards options page can configure: every card registered in
/// <see cref="RelatedCardsManager.RelatedCards"/>, with the ids of one card folded together.
/// </summary>
/// <remarks>
/// There is no list of related cards in the settings code. The manager already finds every
/// <see cref="ICardWithRelatedCards"/> by reflection, and this reads from it, so adding a related
/// card needs nothing here.
///
/// A card often exists under several ids (its Core, Vanilla and Caverns of Time reprints, or a token
/// twin). Those are one card to the user, so they share one row and one override. HDT has no
/// canonical-card id to group by, so a card's identity here is its name, type and stats: that keeps
/// reprints together while same-named cards that really differ (a hero power and a minion, or a
/// minion and the differently-statted token it becomes) stay apart.
/// </remarks>
public static class RelatedCardCatalog
{
	private static readonly object Lock = new();

	// Its own instance rather than Core.Game's: the catalog only reads ids, and going through
	// Core.Game would construct the whole game (and its overlay window) just to list cards, which
	// also makes this unusable from a headless test.
	private static readonly RelatedCardsManager Manager = new();

	private static IReadOnlyList<RelatedCardDescriptor>? _descriptors;
	private static IReadOnlyList<string>? _missingCardIds;
	private static Dictionary<string, IReadOnlyList<string>>? _variantsById;
	private static HashSet<string>? _knownCardIds;

	/// <summary>
	/// Persistence keys of every configurable related card, one per registered id. Independent of the
	/// card database, so it can be asked for without the catalog (or the database) being ready.
	/// </summary>
	public static HashSet<string> KnownCardIds
	{
		get
		{
			lock(Lock)
				return _knownCardIds ??= new HashSet<string>(Manager.RelatedCards.Keys.Where(id => !string.IsNullOrEmpty(id)));
		}
	}

	/// <summary>One descriptor per card, however many ids it has.</summary>
	public static IReadOnlyList<RelatedCardDescriptor> Descriptors
	{
		get
		{
			EnsureBuilt();
			return _descriptors!;
		}
	}

	/// <summary>
	/// Registered card ids the card database could not resolve. Always empty in a healthy build; a
	/// guard test asserts this, which is what stops the other catalog assertions passing vacuously
	/// when a card is silently skipped below.
	/// </summary>
	public static IReadOnlyList<string> MissingCardIds
	{
		get
		{
			EnsureBuilt();
			return _missingCardIds!;
		}
	}

	/// <summary>
	/// All ids that are the same card as <paramref name="cardId"/>, itself included and the
	/// representative first. An id the catalog does not know is just itself.
	/// </summary>
	public static IReadOnlyList<string> GetVariantIds(string cardId)
	{
		EnsureBuilt();
		return _variantsById!.TryGetValue(cardId, out var ids) ? ids : new[] { cardId };
	}

	private static string VariantKey(Card card) =>
		$"{card.Name}|{card.Type}|{card.Cost}|{card.Attack}|{card.Health}";

	private static void EnsureBuilt()
	{
		lock(Lock)
		{
			if(_descriptors == null)
				Build();
		}
	}

	private static void Build()
	{
		var missing = new List<string>();
		var cards = new List<Card>();

		// Ordinal order so the result (and which id represents a card) never depends on the order
		// reflection happens to return types in.
		foreach(var cardId in KnownCardIds.OrderBy(id => id, StringComparer.Ordinal))
		{
			var card = Database.GetCardFromId(cardId);
			if(card == null)
			{
				Log.Error($"Could not find related card {cardId} in the card database");
				missing.Add(cardId);
				continue;
			}
			cards.Add(card);
		}

		var descriptors = new List<RelatedCardDescriptor>();
		var variantsById = new Dictionary<string, IReadOnlyList<string>>();

		foreach(var group in cards.GroupBy(VariantKey))
		{
			// A collectible version is the one worth showing; ties fall back to the id for stability.
			var ordered = group
				.OrderByDescending(Database.IsActualCard)
				.ThenBy(c => c.Id, StringComparer.Ordinal)
				.ToList();
			var ids = ordered.Select(c => c.Id).ToArray();

			descriptors.Add(new RelatedCardDescriptor(ordered[0], ids));
			foreach(var id in ids)
				variantsById[id] = ids;
		}

		// Ordering is deliberately not applied to the descriptors: the display name is live, so a
		// card-language change would leave a cached order stale. The view model sorts at bind time.
		_missingCardIds = missing;
		_variantsById = variantsById;
		_descriptors = descriptors;
	}
}
