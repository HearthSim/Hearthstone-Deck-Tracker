using System.ComponentModel;

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum FilterDeckMode
	{
		[Description("ENUM_WithDeck")]
		WithDeck,
		[Description("ENUM_WithoutDeck")]
		WithoutDeck,
		[Description("ENUM_All")]
		All
	}
}
