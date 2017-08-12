using System;
using System.Globalization;

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
	public static class DateTimeHelper
	{
		public static DateTime StartOfWeek
		{
			get
			{
				var today = DateTime.Today;
				var differenceToFirstDayOfWeek = today.DayOfWeek - CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
				if (differenceToFirstDayOfWeek < 0)
					differenceToFirstDayOfWeek += 7;
				return today.AddDays(-differenceToFirstDayOfWeek);
			}
		}
	}
}
