#region

using System.ComponentModel;

#endregion

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum DisplayedStats
	{
		[Description("All")]
		All,

		[Description("Selected")]
		Selected,

		[Description("Latest")]
		Latest,

		[Description("Selected Major")]
		SelectedMajor,

		[Description("Latest Major")]
		LatestMajor
	}
}