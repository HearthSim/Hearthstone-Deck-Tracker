#region

using System.IO;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using static Hearthstone_Deck_Tracker.Utility.LogConfig.LogConfigConstants;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.LogConfig
{
	internal class LogConfigWatcher
	{
		private static FileSystemWatcher FileWatcher { get; } = new FileSystemWatcher(HearthstoneAppData, LogConfigFile)
		{
			EnableRaisingEvents = true
		};

		public static void Start()
		{
			FileWatcher.Changed += (sender, args) => LogConfigUpdater.Run().Forget();
			FileWatcher.Deleted += (sender, args) => LogConfigUpdater.Run().Forget();
		}

		public static void Pause() => FileWatcher.EnableRaisingEvents = false;

		public static void Continue() => FileWatcher.EnableRaisingEvents = true;
	}
}