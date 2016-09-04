#region

using System.ComponentModel;

#endregion

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum TimeFrame
	{
		[LocDescription("Enum_TimeFrame_Today")]
		Today,

		[LocDescription("Enum_TimeFrame_Yesterday")]
		Yesterday,

		[LocDescription("Enum_TimeFrame_Last24Hours")]
		Last24Hours,

		[LocDescription("Enum_TimeFrame_ThisWeek")]
		ThisWeek,

		[LocDescription("Enum_TimeFrame_PreviousWeek")]
		PreviousWeek,

		[LocDescription("Enum_TimeFrame_Last7Days")]
		Last7Days,

		[LocDescription("Enum_TimeFrame_ThisMonth")]
		ThisMonth,

		[LocDescription("Enum_TimeFrame_PreviousMonth")]
		PreviousMonth,

		[LocDescription("Enum_TimeFrame_ThisYear")]
		ThisYear,

		[LocDescription("Enum_TimeFrame_PreviousYear")]
		PreviousYear,

		[LocDescription("Enum_TimeFrame_AllTime")]
		AllTime
	}

	public enum DisplayedTimeFrame
	{
		[LocDescription("Enum_DisplayedTimeFrame_Today")]
		Today,

		[LocDescription("Enum_DisplayedTimeFrame_ThisWeek")]
		ThisWeek,

		[LocDescription("Enum_DisplayedTimeFrame_CurrentSeason")]
		CurrentSeason,

		[LocDescription("Enum_DisplayedTimeFrame_LastSeason")]
		LastSeason,

		[LocDescription("Enum_DisplayedTimeFrame_CustomSeason")]
		CustomSeason,

		[LocDescription("Enum_DisplayedTimeFrame_AllTime")]
		AllTime,

		[LocDescription("Enum_DisplayedTimeFrame_Custom")]
		Custom
	}
}
