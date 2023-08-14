using System;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class DateTimeExtensions
	{
		public const double TicksPerMicroSecond = TimeSpan.TicksPerMillisecond / 1000;

		private static readonly DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

		public static long ToUnixTimeSeconds(this DateTime time) => new DateTimeOffset(time.ToUniversalTime()).ToUnixTimeSeconds();

		public static long ToUnixTimeMicroSeconds(this DateTime time) => Convert.ToInt64((new DateTimeOffset(time.ToUniversalTime()) - UnixEpoch).Ticks / TicksPerMicroSecond);
	}
}
