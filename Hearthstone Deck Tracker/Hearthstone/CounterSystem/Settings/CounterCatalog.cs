using System;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;

public static class CounterCatalog
{
	private static IReadOnlyList<CounterDescriptor>? _descriptors;
	private static List<CounterFailure>? _failedTypes;

	public static IReadOnlyList<CounterDescriptor> Descriptors
	{
		get
		{
			if(_descriptors == null)
				Build();
			return _descriptors!;
		}
	}

	/// <summary>
	/// Types that could not be described. Always empty in a healthy build — a guard test asserts
	/// this, which is what stops the other catalog tests from passing vacuously when a broken
	/// counter is silently skipped below.
	/// </summary>
	public static IReadOnlyList<CounterFailure> FailedTypes
	{
		get
		{
			if(_descriptors == null)
				Build();
			return _failedTypes!;
		}
	}

	private static void Build()
	{
		var descriptors = new List<CounterDescriptor>();
		var failures = new List<CounterFailure>();

		foreach(var type in CounterTypeProvider.GetCounterTypes())
		{
			try
			{
				if(Activator.CreateInstance(type, true, Core.Game) is not BaseCounter probe)
					throw new InvalidOperationException($"{type.Name} is not a {nameof(BaseCounter)}");

				var descriptor = new CounterDescriptor(probe);

				// Touch the display name here so a counter that throws while describing itself is
				// reported as a failure rather than blowing up later during data binding.
				_ = descriptor.DisplayName;

				descriptors.Add(descriptor);
			}
			catch(Exception ex)
			{
				Log.Error($"Could not describe counter {type.Name}: {ex}");
				failures.Add(new CounterFailure(type, ex));
			}
		}

		// Ordering is deliberately not applied here: DisplayName is live, so a card-language change
		// would leave a cached order stale. The view model sorts at bind time instead.
		_failedTypes = failures;
		_descriptors = descriptors;
	}

	public class CounterFailure
	{
		public CounterFailure(Type type, Exception error)
		{
			Type = type;
			Error = error;
		}

		public Type Type { get; }
		public Exception Error { get; }
	}
}
