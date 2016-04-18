#region

using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.HsReplay.Constants;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class HsReplayUpdater
	{
		private static Version GetCurrentVersion()
		{
			var version = new Version();
			if(!File.Exists(VersionFilePath))
				return version;
			try
			{
				using(var sr = new StreamReader(VersionFilePath))
					Version.TryParse(sr.ReadToEnd(), out version);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			return version;
		}

		private static bool _updating;
		internal static async Task Update()
		{
			if(_updating)
			{
				while(_updating)
					Task.Delay(500);
				return;
			}
			_updating = true;
			var currentVersion = GetCurrentVersion();
			Log.Info($"[{currentVersion}] Checking for updates...");
			var release = await GitHub.CheckForUpdate("Epix37", "HSReplayFreezer", currentVersion);
			if(release == null)
			{
				Log.Info("Up to date.");
				return;
			}
			Log.Info($"Found new update: {release.Tag}, downloading...");
			var filePath = await GitHub.DownloadRelease(release, HsReplayPath);
			if(filePath == null)
			{
				Log.Info("No new update found.");
				return;
			}
			Log.Info("Finished downloading. Unpacking...");
			try
			{
				using(var fs = new FileInfo(filePath).OpenRead())
				{
					var archive = new ZipArchive(fs, ZipArchiveMode.Read);
					archive.ExtractToDirectory(HsReplayPath, true);
				}
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			try
			{
				File.Delete(filePath);
				Log.Info("All done.");
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			_updating = false;
		}
	}
}