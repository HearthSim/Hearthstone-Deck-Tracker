#if(SQUIRREL)
using System;
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
				using(var mgr = await UpdateManager.GitHubUpdateManager(await GetReleaseUrl("hsreplay"), prerelease: Config.Instance.CheckForBetaUpdates))
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
				using(var wc = new WebClient())
					await wc.DownloadFileTaskAsync("https://hsdecktracker.net/releases.json", file);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			using(var sr = new StreamReader(file))
				fileContent = sr.ReadToEnd();
			return _releaseUrl = JsonConvert.DeserializeObject<ReleaseUrls>(fileContent).GetReleaseUrl(release);
		}

		public static async Task StartupUpdateCheck(SplashScreenWindow splashScreenWindow)
		{
			try
			{
				bool updated;
				using(var mgr = await UpdateManager.GitHubUpdateManager(await GetReleaseUrl("hsreplay"), prerelease: Config.Instance.CheckForBetaUpdates))
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
						StatusBar.Visibility = Visibility.Visible;
					else
						UpdateManager.RestartApp();
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		private static async Task<bool> SquirrelUpdate(SplashScreenWindow splashScreenWindow, UpdateManager mgr,
			bool ignoreDelta = false)
		{
			try
			{
				splashScreenWindow.StartSkipTimer();
				var updateInfo = await mgr.CheckForUpdate(ignoreDelta);
				if(!updateInfo.ReleasesToApply.Any())
					return false;
				if(updateInfo.ReleasesToApply.LastOrDefault()?.Version <= mgr.CurrentlyInstalledVersion())
					return false;
				await mgr.DownloadReleases(updateInfo.ReleasesToApply, splashScreenWindow.Updating);
				await mgr.ApplyReleases(updateInfo, splashScreenWindow.Installing);
				await mgr.CreateUninstallerRegistryEntry();
				return true;
			}
			catch(Exception)
			{
				if(!ignoreDelta)
					return await SquirrelUpdate(splashScreenWindow, mgr, true);
				return false;
			}
		}

		internal static void StartUpdate() => UpdateManager.RestartApp();
	}
}

#endif