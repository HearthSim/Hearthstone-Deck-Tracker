#region

using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Plugins;
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
			Client.Release = Helper.GetCurrentVersion().ToVersionString(true);
		}

		private static readonly RavenClient Client = new RavenClient("https://0a6c07cee8d141f0bee6916104a02af4:883b339db7b040158cdfc42287e6a791@app.getsentry.com/80405");

		public static string CaptureException(Exception ex)
		{
			var plugins = PluginManager.Instance.Plugins.Where(x => x.IsEnabled).ToList();
			ex.Data.Add("active-plugins", plugins.Any() ? string.Join(", ", plugins.Select(x => x.NameAndVersion)) : "none");

			var exception = new SentryEvent(ex);
#if(SQUIRREL)
			exception.Tags.Add("squirrel", "true");
#else
			exception.Tags.Add("squirrel", "false");
#endif
			exception.Tags.Add("hearthstone", Helper.GetHearthstoneBuild()?.ToString());
			return Client.Capture(exception);
		}
	}
}
