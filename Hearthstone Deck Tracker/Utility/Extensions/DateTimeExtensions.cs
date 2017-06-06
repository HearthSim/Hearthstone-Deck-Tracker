using System;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class DateTimeExtensions
	{
		public static long ToUnixTime(this DateTime time) 
			=> Math.Max(0, (long)(time.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds);
	}
}
