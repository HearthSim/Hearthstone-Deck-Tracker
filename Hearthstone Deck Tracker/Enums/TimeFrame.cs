#region

using System.ComponentModel;

#endregion

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum TimeFrame
	{
		[Description("Today")]
		Today,

		[Description("Yesterday")]
		Yesterday,

		[Description("Last 24 Hours")]
		Last24Hours,

		[Description("This Week")]
		ThisWeek,

		[Description("Previous Week")]
		PreviousWeek,

		[Description("Last 7 Days")]
		Last7Days,

		[Description("This Month")]
		ThisMonth,

		[Description("Previous Month")]
		PreviousMonth,

		[Description("This Year")]
		ThisYear,

		[Description("Previous Year")]
		PreviousYear,

		[Description("All Time")]
		AllTime
	}

	public enum DisplayedTimeFrame
	{
		[Description("Today")]
		Today,

		[Description("Week")]
		ThisWeek,

		[Description("Season")]
		CurrentSeason,

		[Description("All Time")]
		AllTime,

		[Description("Custom")]
		Custom
	}
}