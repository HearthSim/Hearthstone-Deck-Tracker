#region

using System;
using System.IO;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.Utility.LogConfig.LogConfigConstants;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.LogConfig
{
	internal class LogConfigWatcher
	{
		private static FileSystemWatcher _fileWatcher;

		public static void Start()
		{
			try
			{
				_fileWatcher = new FileSystemWatcher(HearthstoneAppData, LogConfigFile)
				{
					EnableRaisingEvents = true
				};
				_fileWatcher.Changed += (sender, args) => LogConfigUpdater.Run().Forget();
				_fileWatcher.Deleted += (sender, args) => LogConfigUpdater.Run().Forget();
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		public static void Pause()
		{
			if(_fileWatcher != null)
				_fileWatcher.EnableRaisingEvents = false;
		}

		public static void Continue()
		{
			if(_fileWatcher != null)
				_fileWatcher.EnableRaisingEvents = true;
		}
	}
}
