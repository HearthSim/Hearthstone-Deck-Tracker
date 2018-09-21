using System;
using Hearthstone_Deck_Tracker.Utility.Twitch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Utility.Twitch
{
	[TestClass]
	public class TwitchApiHelperTests
	{
		private const string BaseUrl = "https://twitch.tv/videos/42";

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void VodCreatedInFuture_ThrowsException()
		{
			var vodCreatedAt = DateTime.Now;
			var matchStart = vodCreatedAt.AddHours(-1);
			TwitchApiHelper.GenerateTwitchVodUrl(BaseUrl, vodCreatedAt, matchStart);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException))]
		public void InvalidBaseUrl_ThrowsException()
		{
			var vodCreatedAt = DateTime.Now;
			var matchStart = vodCreatedAt.AddHours(1);
			TwitchApiHelper.GenerateTwitchVodUrl("", vodCreatedAt, matchStart);
		}

		[TestMethod]
		public void MatchAtBeginningOfVod_ValidUrl()
		{
			var vodCreatedAt = DateTime.Now;
			var url = TwitchApiHelper.GenerateTwitchVodUrl(BaseUrl, vodCreatedAt, vodCreatedAt);
			Assert.AreEqual(url, $"{BaseUrl}?t=0h0m0s");
		}

		[TestMethod]
		public void MatchAfterBeginningOfVod_ValidUrl()
		{
			var vodCreatedAt = DateTime.Now;
			var matchStart = vodCreatedAt.AddHours(2).AddMinutes(17).AddSeconds(44);
			var url = TwitchApiHelper.GenerateTwitchVodUrl(BaseUrl, vodCreatedAt, matchStart);
			Assert.AreEqual($"{BaseUrl}?t=2h17m44s", url);
		}
	}
}
