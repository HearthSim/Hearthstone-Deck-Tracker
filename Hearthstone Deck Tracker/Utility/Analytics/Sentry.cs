#region

using System;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using SharpRaven;
using SharpRaven.Data;

#endregion


namespace Hearthstone_Deck_Tracker.Utility.Analytics
{
	internal class Sentry
	{
		static Sentry()
		{
			Client.Release = Helper.GetCurrentVersion().ToVersionString();
		}

		private static readonly RavenClient Client = new RavenClient("https://0a6c07cee8d141f0bee6916104a02af4:883b339db7b040158cdfc42287e6a791@app.getsentry.com/80405");

		public static string CaptureException(Exception ex) => Client.Capture(new SentryEvent(ex));
	}
}