using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
