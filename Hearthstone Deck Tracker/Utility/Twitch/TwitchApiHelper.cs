using System;

namespace Hearthstone_Deck_Tracker.Utility.Twitch
{
	public static class TwitchApiHelper
	{
		/// <summary>
		/// Generate a Twitch VOD timestamp based on the vod creation and match start times.
		/// </summary>
		/// <param name="baseUrl">Base url of the Twitch VOD.</param>
		/// <param name="vodCreatedAt">Time the vod was created according to the API.</param>
		/// <param name="matchStart">Trusted time for when the match was started.</param>
		/// <exception cref="ArgumentException">
		///		Throws ArgumentException if `vodCreatedAt` is later than `matchStart`.
		/// </exception>
		/// <returns>Formatted url for the game in the Twitch VOD</returns>
		public static string GenerateTwitchVodUrl(string baseUrl, DateTime vodCreatedAt, DateTime matchStart)
		{
			if(string.IsNullOrEmpty(baseUrl))
				throw new ArgumentException("`baseUrl` can not be empty");
			var utcCreated = TimeZoneInfo.ConvertTimeToUtc(vodCreatedAt);
			var utcMatchStart = TimeZoneInfo.ConvertTimeToUtc(matchStart);
			if(utcCreated > utcMatchStart)
				throw new ArgumentException("Vod must have been created in the past");
			var diff = utcMatchStart - utcCreated;
			return $"{baseUrl}?t={diff.Hours}h{diff.Minutes}m{diff.Seconds}s";
		}
	}
}
