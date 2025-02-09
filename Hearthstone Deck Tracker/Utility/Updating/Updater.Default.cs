#if(!SQUIRREL)
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker.Utility.Updating
{
	internal static partial class Updater
	{
		private static bool _showingUpdateMessage;
		private static GitHub.Release? _release;
		private static bool TempUpdateCheckDisabled { get; set; }

		private static bool ShouldCheckForUpdates()
			=> Config.Instance.CheckForUpdates && !TempUpdateCheckDisabled && !Core.Game.IsRunning && !_showingUpdateMessage
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
				ShowNewUpdateMessage();
			}
		}

		private static async void ShowNewUpdateMessage()
		{
			if(_showingUpdateMessage)
				return;
			_showingUpdateMessage = true;

			var settings = new MessageDialogs.Settings {AffirmativeButtonText = LocUtil.Get("Button_Download"), NegativeButtonText = LocUtil.Get("Button_Notnow")};
			if(_release == null)
			{
				_showingUpdateMessage = false;
				return;
			}
			try
			{
				await Task.Delay(10000);
				// TODO: Find a better way to interact with the MainWindow
				Core.MainWindow.ActivateWindow();
				while(Core.MainWindow.Visibility != Visibility.Visible || Core.MainWindow.WindowState == WindowState.Minimized)
					await Task.Delay(100);
				var updateString = LocUtil.Get("MainWindow_StatusBarUpdate_NewUpdateAvailable");
				var result = await Core.MainWindow.ShowMessageAsync(updateString, LocUtil.Get("MainWindow_ShowMessage_UpdateDialog"), MessageDialogStyle.AffirmativeAndNegative, settings);

				if(result == MessageDialogResult.Affirmative)
					StartUpdate();
				else
					TempUpdateCheckDisabled = true;

				_showingUpdateMessage = false;
			}
			catch(Exception e)
			{
				_showingUpdateMessage = false;
				Log.Error("Error showing new update message\n" + e);
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
