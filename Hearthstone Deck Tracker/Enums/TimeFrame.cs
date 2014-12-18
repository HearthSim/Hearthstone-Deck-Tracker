using System.ComponentModel;

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum TimeFrame
	{
		[Description("ENUM_Today")]
		Today,
		[Description("ENUM_Yesterday")]
		Yesterday,
		[Description("ENUM_Last24Hours")]
		Last24Hours,
		[Description("ENUM_ThisWeek")]
		ThisWeek,
		[Description("ENUM_PreviousWeek")]
		PreviousWeek,
		[Description("ENUM_Last7Days")]
		Last7Days,
		[Description("ENUM_ThisMonth")]
		ThisMonth,
		[Description("ENUM_PreviousMonth")]
		PreviousMonth,
		[Description("ENUM_ThisYear")]
		ThisYear,
		[Description("ENUM_PreviousYear")]
		PreviousYear,
		[Description("ENUM_AllTime")]
		AllTime
	}
}
