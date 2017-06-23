#region

using System.ComponentModel;

#endregion

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum FilterDeckMode
	{
		[Description("With deck")]
		WithDeck,

		[Description("Without deck")]
		WithoutDeck,

		[Description("All")]
		All
	}
}
