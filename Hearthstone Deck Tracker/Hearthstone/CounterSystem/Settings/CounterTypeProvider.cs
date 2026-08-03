using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;

/// <summary>
/// The single enumeration of counter types in the codebase.
/// </summary>
/// <remarks>
/// Both <see cref="CounterManager"/> (runtime instances) and <see cref="CounterCatalog"/> (options
/// UI metadata) go through here. Keeping it to one query is what makes adding a new counter
/// zero-touch: there is no list to forget to update, because there is no list.
/// </remarks>
public static class CounterTypeProvider
{
	private static IReadOnlyList<Type>? _counterTypes;
	private static HashSet<string>? _knownCounterIds;

	public static IReadOnlyList<Type> GetCounterTypes() => _counterTypes ??= FindCounterTypes();

	/// <summary>Persistence keys (<c>Type.Name</c>) of every counter this build knows about.</summary>
	public static HashSet<string> GetKnownCounterIds() =>
		_knownCounterIds ??= new HashSet<string>(GetCounterTypes().Select(t => t.Name));

	private static IReadOnlyList<Type> FindCounterTypes()
	{
		Type[] allTypes;
		try
		{
			allTypes = typeof(BaseCounter).Assembly.GetTypes();
		}
		catch(ReflectionTypeLoadException ex)
		{
			allTypes = ex.Types.Where(t => t != null).Select(t => t!).ToArray();
		}

		return allTypes
			.Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseCounter)))
			.ToList();
	}
}
