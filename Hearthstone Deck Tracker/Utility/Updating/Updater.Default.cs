#if(!SQUIRREL)
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Updating
{
	internal static partial class Updater
	{
		private static GitHub.Release? _release;

		private static bool ShouldCheckForUpdates()
			=> Config.Instance.CheckForUpdates && Status.UpdaterState != UpdaterState.Available && !Core.Game.IsRunning
			   && DateTime.Now - _lastUpdateCheck >= new TimeSpan(0, 10, 0);

		public static async void CheckForUpdates(bool force = false)
		{
			if(!force && !ShouldCheckForUpdates())
				return;
			_lastUpdateCheck = DateTime.Now;
			_release = await GetLatestRelease();
			if(_release != null)
			{
				Status.UpdaterState = UpdaterState.Available;
				Status.StatusBarVisibility = Visibility.Visible;
			}
		}

		internal static async void StartUpdate()
		{
			Log.Info("Starting update...");
			if(_release == null || DateTime.Now - _lastUpdateCheck > new TimeSpan(0, 10, 0))
				_release = await GetLatestRelease();
			if(_release == null)
			{
				Log.Error("Could not get latest version. Not updating.");
				return;
			}
			var url = _release.Assets?[0]?.Url;
			if(string.IsNullOrEmpty(url))
			{
				Log.Error("Could not find url for latest version. Not updating.");
				return;
			}
			try
			{
				Process.Start("HDTUpdate.exe", $"{Process.GetCurrentProcess().Id} {url}");
				await Core.Shutdown();
			}
			catch(Exception ex)
			{
				Log.Error("Error starting updater\n" + ex);
				Helper.TryOpenUrl(url!);
			}
		}

		public static void Cleanup()
		{
			try
			{
				if(File.Exists("HDTUpdate_new.exe"))
				{
					if(File.Exists("HDTUpdate.exe"))
						File.Delete("HDTUpdate.exe");
					File.Move("HDTUpdate_new.exe", "HDTUpdate.exe");
				}
			}
			catch(Exception e)
			{
				Log.Error("Error updating updater\n" + e);
			}
			try
			{
				//updater used pre v0.9.6
				if(File.Exists("Updater.exe"))
					File.Delete("Updater.exe");
			}
			catch(Exception e)
			{
				Log.Error("Error deleting Updater.exe\n" + e);
			}
		}

		private static async Task<GitHub.Release?> GetLatestRelease()
		{
			var currentVersion = Helper.GetCurrentVersion();
			if(currentVersion == null)
				return null;
			return await GitHub.CheckForUpdate("HearthSim", "Hearthstone-Deck-Tracker", currentVersion);
		}
	}
}

#endif
