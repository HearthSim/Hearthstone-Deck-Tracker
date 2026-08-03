using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility
{
	/// <summary>
	/// Settings whose names are not known at compile time, so they cannot be declared as bool
	/// properties. Both settings converters append these to the array they emit.
	/// </summary>
	public interface IVMDynamicSettings
	{
		IEnumerable<string> DynamicEnabledSettings { get; }
		IEnumerable<string> DynamicDisabledSettings { get; }
	}
}
