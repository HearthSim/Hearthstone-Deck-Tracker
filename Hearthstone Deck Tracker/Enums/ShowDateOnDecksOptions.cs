using System.ComponentModel;

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum ShowDateOnDecksOptions
	{
		[LocDescription("Enum_Show_No_Date")]
		showNoDate,
		[LocDescription("Enum_Last_Time_Played")]
		showLastPlayedDate,
		[LocDescription("Enum_Last_Time_Edited")]
		showLastEditedDate
	}
}
