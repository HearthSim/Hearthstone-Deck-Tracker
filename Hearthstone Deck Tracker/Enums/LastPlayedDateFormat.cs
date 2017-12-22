using System.ComponentModel;

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum LastPlayedDateFormat
	{
		[Description("dd/MM/yyyy")]
		DayMonthYear,
		[Description("MM/dd/yyyy")]
		MonthDayYear
	}
}
