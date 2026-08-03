using System;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;

/// <summary>
/// Reads and writes the per-counter visibility overrides stored in
/// <see cref="Config.CounterVisibilityOverrides"/>.
/// </summary>
/// <remarks>
/// Lookups have to be O(1): every counter value change re-runs GetVisibleCounters(), which resolves
/// the override for all ~146 counter instances. The config list is therefore projected into a
/// dictionary that is rebuilt only when something actually changes.
/// </remarks>
public sealed class CounterVisibilitySettings
{
	public static CounterVisibilitySettings Instance { get; } = new();

	private CounterVisibilitySettings()
	{
	}

	private Dictionary<string, CounterVisibilityOverride>? _lookup;

	private static List<CounterVisibilityOverride> Overrides => Config.Instance.CounterVisibilityOverrides;

	public Action SaveConfig { get; set; } = Config.Save;

	private Dictionary<string, CounterVisibilityOverride> Lookup
	{
		get
		{
			if(_lookup != null)
				return _lookup;

			_lookup = new Dictionary<string, CounterVisibilityOverride>();
			foreach(var entry in Overrides)
			{
				if(!string.IsNullOrEmpty(entry.CounterId))
					_lookup[entry.CounterId] = entry;
			}
			return _lookup;
		}
	}

	public void Invalidate() => _lookup = null;

	public CounterVisibility Get(string counterId, bool isPlayer) =>
		Lookup.TryGetValue(counterId, out var entry) ? entry.Get(isPlayer) : CounterVisibility.Auto;

	public void Set(string counterId, bool isPlayer, CounterVisibility value)
	{
		if(string.IsNullOrEmpty(counterId) || Get(counterId, isPlayer) == value)
			return;

		if(Lookup.TryGetValue(counterId, out var entry))
		{
			entry.Set(isPlayer, value);

			if(entry.IsEmpty)
			{
				Overrides.Remove(entry);
				Lookup.Remove(counterId);
			}
		}
		else
		{
			entry = new CounterVisibilityOverride { CounterId = counterId };
			entry.Set(isPlayer, value);
			Overrides.Add(entry);
			Lookup[counterId] = entry;
		}

		SaveConfig();
		OnChanged();
	}

	public bool HasAnyOverride => Overrides.Any(x => !x.IsEmpty);

	/// <summary>
	/// Counters explicitly set to <paramref name="visibility"/> for one side. Auto returns nothing:
	/// it is the absence of an entry, not a value. Entries for counters this build does not know
	/// about are skipped, so a stale config cannot leak unknown ids into metrics.
	/// </summary>
	public IEnumerable<string> GetCounterIds(bool isPlayer, CounterVisibility visibility)
	{
		if(visibility == CounterVisibility.Auto)
			return Enumerable.Empty<string>();

		var known = CounterTypeProvider.GetKnownCounterIds();
		return Overrides
			.Where(x => known.Contains(x.CounterId) && x.Get(isPlayer) == visibility)
			.Select(x => x.CounterId);
	}

	public void ResetAll()
	{
		var known = CounterTypeProvider.GetKnownCounterIds();
		var removed = Overrides.RemoveAll(x => string.IsNullOrEmpty(x.CounterId) || known.Contains(x.CounterId));
		if(removed == 0)
			return;

		Invalidate();
		SaveConfig();
		OnChanged();
	}

	public event EventHandler? Changed;

	private void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
