#region

using Garlic;

#endregion

//using Garlic;

namespace Hearthstone_Deck_Tracker.Analytics
{
	internal class Analytics
	{
		private const string Domain = "app.hsdecktracker.net";
		private const string GACode = "UA-68659282-3";
		private static readonly AnalyticsSession Session = new AnalyticsSession(Domain, GACode);
		private static IAnalyticsPageViewRequest _pageViewRequest;

		public static void TrackEvent(string category, string action, string label = "", string value = "")
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			_pageViewRequest?.SendEvent(category, action, label, value);
		}

		public static void TrackPageView(string page, string title)
		{
			if(!Config.Instance.GoogleAnalytics)
				return;
			_pageViewRequest = Session.CreatePageViewRequest(page, title);
			_pageViewRequest.Send();
		}
	}
}