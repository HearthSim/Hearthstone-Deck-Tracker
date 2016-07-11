#region

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;
#if(SQUIRREL)
	using Squirrel;
#endif

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class Updater
	{
		private static DateTime _lastUpdateCheck;
		private static bool _showingUpdateMessage;
		private static bool TempUpdateCheckDisabled { get; set; }
		public static StatusBarHelper StatusBar { get; } = new StatusBarHelper();
		private static GitHub.Release _release;

		public static async void CheckForUpdates(bool force = false)
		{
#if(SQUIRREL)
			using(var mgr = await UpdateManager.GitHubUpdateManager("https://github.com/Epix37/HDT-Releases", prerelease: Config.Instance.CheckForBetaUpdates))
			{
				var release = await mgr.UpdateApp();
				if(release != null)
					StatusBar.Visibility = Visibility.Visible;
			}
#else
			if(!force)
			{
				if(!Config.Instance.CheckForUpdates || TempUpdateCheckDisabled || Core.Game.IsRunning || _showingUpdateMessage
				   || (DateTime.Now - _lastUpdateCheck) < new TimeSpan(0, 10, 0))
					return;
			}
			_lastUpdateCheck = DateTime.Now;
			_release = await GetLatestRelease(false);
			if(_release != null)
			{
				StatusBar.Visibility = Visibility.Visible;
				ShowNewUpdateMessage(false);
			}
			else if(Config.Instance.CheckForBetaUpdates)
			{
				_release = await GetLatestRelease(true);
				if(_release != null)
					ShowNewUpdateMessage(true);
			}
#endif

		}

		private static async void ShowNewUpdateMessage(bool beta)
		{
			if(_showingUpdateMessage)
				return;
			_showingUpdateMessage = true;
			
			var settings = new MessageDialogs.Settings {AffirmativeButtonText = "Download", NegativeButtonText = "Not now"};
			if(_release == null)
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
				var betaString = beta ? " BETA" : "";
				var result =
					await
					Core.MainWindow.ShowMessageAsync("New" + betaString + " Update available!", "Press \"Download\" to automatically download.",
					                                 MessageDialogStyle.AffirmativeAndNegative, settings);

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
#if(SQUIRREL)
			UpdateManager.RestartApp();
#else
			Log.Info("Starting update...");
			if(_release == null || (DateTime.Now - _lastUpdateCheck) > new TimeSpan(0, 10, 0))
				_release = await GetLatestRelease(Config.Instance.CheckForBetaUpdates);
			if(_release == null)
			{
				Log.Error("Could not get latest version. Not updating.");
				return;
			}
			try
			{
				Process.Start("HDTUpdate.exe", $"{Process.GetCurrentProcess().Id} {_release.Assets[0].Url}");
				Core.MainWindow.Close();
				Application.Current.Shutdown();
			}
			catch(Exception ex)
			{
				Log.Error("Error starting updater\n" + ex);
				Helper.TryOpenUrl($"{_release.Assets[0].Url}");
			}
#endif
		}

#if(!SQUIRREL)
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

		private static async Task<GitHub.Release> GetLatestRelease(bool beta)
		{
			var currentVersion = Helper.GetCurrentVersion();
			if(currentVersion == null)
				return null;
			return await GitHub.CheckForUpdate("HearthSim", "Hearthstone-Deck-Tracker", currentVersion, beta);
		}
#endif
	}

	public class StatusBarHelper : INotifyPropertyChanged
	{
		private Visibility _visibility = Visibility.Collapsed;

		public Visibility Visibility
		{
			get { return _visibility; }
			set
			{
				_visibility = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}