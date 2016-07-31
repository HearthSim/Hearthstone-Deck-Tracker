#if(SQUIRREL)
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using Newtonsoft.Json;
using Squirrel;

namespace Hearthstone_Deck_Tracker.Utility.Updating
{
	internal static partial class Updater
	{
		private static string _releaseUrl;

		private static bool ShouldCheckForUpdates()
			=> Config.Instance.CheckForUpdates && !Core.Game.IsRunning
				&& DateTime.Now - _lastUpdateCheck >= new TimeSpan(0, 10, 0);

		public static async void CheckForUpdates(bool force = false)
		{
			if(!force && !ShouldCheckForUpdates())
				return;
			_lastUpdateCheck = DateTime.Now;
			try
			{
				using(var mgr = await UpdateManager.GitHubUpdateManager(await GetReleaseUrl("live"), prerelease: Config.Instance.CheckForBetaUpdates))
				{
					var release = await mgr.UpdateApp();
					if(release != null)
						StatusBar.Visibility = Visibility.Visible;
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		private static async Task<string> GetReleaseUrl(string release)
		{
			if(!string.IsNullOrEmpty(_releaseUrl))
				return _releaseUrl;
			var file = Path.Combine(Config.AppDataPath, "releases.json");
			string fileContent;
			try
			{
				Log.Info("Downloading releases file");
				using(var wc = new WebClient())
					await wc.DownloadFileTaskAsync("https://hsdecktracker.net/releases.json", file);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			using(var sr = new StreamReader(file))
				fileContent = sr.ReadToEnd();
			_releaseUrl = JsonConvert.DeserializeObject<ReleaseUrls>(fileContent).GetReleaseUrl(release);
			Log.Info($"using '{release}' release: {_releaseUrl}");
			return _releaseUrl;
		}

		public static async Task StartupUpdateCheck(SplashScreenWindow splashScreenWindow)
		{
			try
			{
				Log.Info("Checking for updates");
				bool updated;
				using(var mgr = await UpdateManager.GitHubUpdateManager(await GetReleaseUrl("live"), prerelease: Config.Instance.CheckForBetaUpdates))
				{
					SquirrelAwareApp.HandleEvents(
						v => mgr.CreateShortcutForThisExe(),
						v => mgr.CreateShortcutForThisExe(),
						onAppUninstall: v => mgr.RemoveShortcutForThisExe()
						);
					updated = await SquirrelUpdate(splashScreenWindow, mgr);
				}
				if(updated)
				{
					if(splashScreenWindow.SkipWasPressed)
					{
						Log.Info("Update complete, showing update bar");
						StatusBar.Visibility = Visibility.Visible;
					}
					else
					{
						Log.Info("Update complete, restarting");
						UpdateManager.RestartApp();
					}
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		private static async Task<bool> SquirrelUpdate(SplashScreenWindow splashScreenWindow, UpdateManager mgr, bool ignoreDelta = false)
		{
			try
			{
				Log.Info($"Checking for updates (ignoreDelta={ignoreDelta})");
				splashScreenWindow.StartSkipTimer();
				var updateInfo = await mgr.CheckForUpdate(ignoreDelta);
				if(!updateInfo.ReleasesToApply.Any())
				{
					Log.Info("No new updated available");
					return false;
				}
				var latest = updateInfo.ReleasesToApply.LastOrDefault()?.Version;
				var current = mgr.CurrentlyInstalledVersion();
				if(latest <= current)
				{
					Log.Info($"Installed version ({current}) is greater than latest release found ({latest}). Not downloading updates.");
					return false;
				}
				Log.Info($"Downloading {updateInfo.ReleasesToApply.Count} {(ignoreDelta ? "" : "delta ")}releases, latest={latest?.Version}");
				await mgr.DownloadReleases(updateInfo.ReleasesToApply, splashScreenWindow.Updating);
				Log.Info("Applying releases");
				await mgr.ApplyReleases(updateInfo, splashScreenWindow.Installing);
				await mgr.CreateUninstallerRegistryEntry();
				Log.Info("Done");
				return true;
			}
			catch(Exception ex)
			{
				if(ignoreDelta)
					return false;
				if(ex is Win32Exception)
					Log.Info("Not able to apply deltas, downloading full release");
				return await SquirrelUpdate(splashScreenWindow, mgr, true);
			}
		}

		internal static void StartUpdate()
		{
			Log.Info("Restarting...");
			UpdateManager.RestartApp();
		}
	}
}

#endif