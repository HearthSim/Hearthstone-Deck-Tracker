#region

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class Updater
	{
		private static DateTime _lastUpdateCheck;
		private static bool _showingUpdateMessage;
		private static bool TempUpdateCheckDisabled { get; set; }

		public static async void CheckForUpdates(bool force = false)
		{
			if(!force)
			{
				if(!Config.Instance.CheckForUpdates || TempUpdateCheckDisabled || Core.Game.IsRunning || _showingUpdateMessage
				   || (DateTime.Now - _lastUpdateCheck) < new TimeSpan(0, 10, 0))
					return;
			}
			_lastUpdateCheck = DateTime.Now;
			var newVersion = await GetLatestVersion(false);
			if(newVersion != null)
				ShowNewUpdateMessage(newVersion, false);
			else if(Config.Instance.CheckForBetaUpdates)
			{
				newVersion = await GetLatestVersion(true);
				if(newVersion != null)
					ShowNewUpdateMessage(newVersion, true);
			}
		}

		private static async void ShowNewUpdateMessage(Version newVersion, bool beta)
		{
			if(_showingUpdateMessage)
				return;
			_showingUpdateMessage = true;

			const string releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
			var settings = new MessageDialogs.Settings {AffirmativeButtonText = "Download", NegativeButtonText = "Not now"};
			if(newVersion == null)
			{
				_showingUpdateMessage = false;
				return;
			}
			try
			{
				await Task.Delay(10000);
				Core.MainWindow.ActivateWindow();
				while(Core.MainWindow.Visibility != Visibility.Visible || Core.MainWindow.WindowState == WindowState.Minimized)
					await Task.Delay(100);
				var newVersionString = $"{newVersion.Major}.{newVersion.Minor}.{newVersion.Build}";
				var betaString = beta ? " BETA" : "";
				var result =
					await
					Core.MainWindow.ShowMessageAsync("New" + betaString + " Update available!", "Press \"Download\" to automatically download.",
					                                 MessageDialogStyle.AffirmativeAndNegative, settings);

				if(result == MessageDialogResult.Affirmative)
				{
					//recheck, in case there was no immediate response to the dialog
					if((DateTime.Now - _lastUpdateCheck) > new TimeSpan(0, 10, 0))
					{
						newVersion = await GetLatestVersion(beta);
						if(newVersion != null)
							newVersionString = $"{newVersion.Major}.{newVersion.Minor}.{newVersion.Build}";
					}
					try
					{
						Process.Start("HDTUpdate.exe", $"{Process.GetCurrentProcess().Id} {newVersionString}");
						Core.MainWindow.Close();
						Application.Current.Shutdown();
					}
					catch(Exception ex)
					{
						Log.Error("Error starting updater\n" + ex);
						Helper.TryOpenUrl(releaseDownloadUrl);
					}
				}
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

		public static async Task<Version> GetLatestVersion(bool beta)
		{
			var betaString = beta ? "beta" : "live";
			var currentVersion = Helper.GetCurrentVersion();
			if(currentVersion == null)
				return null;
			Log.Info($"Checking for {betaString} updates... (current: {currentVersion})");
			try
			{
				string xml;
				using(var wc = new WebClient())
					xml = await wc.DownloadStringTaskAsync($"https://raw.githubusercontent.com/Epix37/HDT-Data/master/{betaString}-version");

				var newVersion = new Version(XmlManager<SerializableVersion>.LoadFromString(xml).ToString());
				Log.Info("Latest " + betaString + " version: " + newVersion);

				if(newVersion > currentVersion)
					return newVersion;
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			return null;
		}
	}
}