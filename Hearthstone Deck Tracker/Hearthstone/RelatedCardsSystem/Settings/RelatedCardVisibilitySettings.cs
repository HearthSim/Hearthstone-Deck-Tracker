using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;

namespace Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Settings;

/// <summary>
/// Reads and writes the per-card overrides for the opponent's "Related Cards" list, stored in
/// <see cref="Config.RelatedCardVisibilityOverrides"/>.
/// </summary>
/// <remarks>
/// The list is rebuilt on every opponent card update and looks up every registered related card, so
/// lookups have to be O(1): the config list is projected into a dictionary that is rebuilt only when
/// something actually changes. The visibility mode is shared with the counters
/// (<see cref="CounterVisibility"/>): Auto keeps the card's own heuristic.
/// </remarks>
public sealed class RelatedCardVisibilitySettings
{
	public static RelatedCardVisibilitySettings Instance { get; } = new();

	private RelatedCardVisibilitySettings()
	{
	}

	private Dictionary<string, RelatedCardVisibilityOverride>? _lookup;

	private static List<RelatedCardVisibilityOverride> Overrides => Config.Instance.RelatedCardVisibilityOverrides;

	public Action SaveConfig { get; set; } = Config.Save;

	private Dictionary<string, RelatedCardVisibilityOverride> Lookup
	{
		get
		{
			if(_lookup != null)
				return _lookup;

			_lookup = new Dictionary<string, RelatedCardVisibilityOverride>();
			foreach(var entry in Overrides)
			{
				if(!string.IsNullOrEmpty(entry.CardId))
					_lookup[entry.CardId] = entry;
			}
			return _lookup;
		}
	}

	public void Invalidate() => _lookup = null;

	/// <summary>
	/// The override for a card, which is shared by every id of that card (see
	/// <see cref="RelatedCardCatalog.GetVariantIds"/>). Any id's entry counts, not just the
	/// representative's, so a card that later gains another id, or an entry saved before an id
	/// joined its group, keeps applying.
	/// </summary>
	public CounterVisibility GetOpponent(string cardId)
	{
		// Checked first: it is the common case (nothing customised), and it keeps the catalog, and
		// with it the card database, out of the picture for anyone who never used this setting.
		if(Lookup.Count == 0)
			return CounterVisibility.Auto;

		foreach(var id in RelatedCardCatalog.GetVariantIds(cardId))
		{
			if(Lookup.TryGetValue(id, out var entry))
				return entry.Opponent;
		}
		return CounterVisibility.Auto;
	}

	/// <summary>
	/// Sets the override for every id of the card. It is stored once, under the representative id;
	/// entries under the card's other ids are folded into it, which also cleans up after the group
	/// changed shape.
	/// </summary>
	public void SetOpponent(string cardId, CounterVisibility value)
	{
		if(string.IsNullOrEmpty(cardId))
			return;

		var ids = RelatedCardCatalog.GetVariantIds(cardId);
		var changed = false;

		foreach(var other in ids.Skip(1))
			changed |= Remove(other);

		var representative = ids[0];
		if(value == CounterVisibility.Auto)
		{
			changed |= Remove(representative);
		}
		else if(Lookup.TryGetValue(representative, out var entry))
		{
			if(entry.Opponent != value)
			{
				entry.Opponent = value;
				changed = true;
			}
		}
		else
		{
			entry = new RelatedCardVisibilityOverride { CardId = representative, Opponent = value };
			Overrides.Add(entry);
			Lookup[representative] = entry;
			changed = true;
		}

		if(!changed)
			return;

		SaveConfig();
		OnChanged();
	}

	private bool Remove(string cardId)
	{
		if(!Lookup.TryGetValue(cardId, out var entry))
			return false;

		Overrides.Remove(entry);
		Lookup.Remove(cardId);
		return true;
	}

	/// <summary>
	/// Final decision for one card: the user's override wins, otherwise the card's own heuristic.
	/// Legality is deliberately not part of this — it is a hard gate the caller applies first, so a
	/// forced card can still never be one that cannot exist in the current format.
	/// </summary>
	public static bool Resolve(CounterVisibility mode, Func<bool> heuristic) => mode switch
	{
		CounterVisibility.Disabled => false,
		CounterVisibility.Enabled => true,
		_ => heuristic(),
	};

	public bool HasAnyOverride => Overrides.Any(x => !x.IsEmpty);

	/// <summary>
	/// Cards explicitly set to <paramref name="visibility"/> for the opponent, one id per card. Auto returns nothing:
	/// it is the absence of an entry, not a value. Entries for cards this build does not know about
	/// are skipped, so a stale config cannot leak unknown ids into metrics.
	/// </summary>
	public IEnumerable<string> GetOpponentCardIds(CounterVisibility visibility)
	{
		// Checked before touching the catalog: it instantiates every related card, which a user who
		// never changed this setting should not pay for just because metrics were collected.
		if(visibility == CounterVisibility.Auto || Overrides.Count == 0)
			return Enumerable.Empty<string>();

		// One id per card: report the representative, and only if it is what the card resolves to, so
		// a card is counted once however many ids or stale entries it has.
		var known = RelatedCardCatalog.KnownCardIds;
		return Overrides
			.Where(x => known.Contains(x.CardId))
			.Select(x => RelatedCardCatalog.GetVariantIds(x.CardId)[0])
			.Distinct()
			.Where(id => GetOpponent(id) == visibility);
	}

	public void ResetAll()
	{
		var known = RelatedCardCatalog.KnownCardIds;
		var removed = Overrides.RemoveAll(x => string.IsNullOrEmpty(x.CardId) || known.Contains(x.CardId));
		if(removed == 0)
			return;

		Invalidate();
		SaveConfig();
		OnChanged();
	}

	public event EventHandler? Changed;

	private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
