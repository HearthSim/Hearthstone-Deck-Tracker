#if(SQUIRREL)
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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

		private static bool _useChinaMirror = CultureInfo.CurrentCulture.Name == "zh-CN";
		private static ReleaseUrls _releaseUrls;
		private static TimeSpan _updateCheckDelay = new TimeSpan(0, 20, 0);
		private static bool ShouldCheckForUpdates()
			=> Config.Instance.CheckForUpdates && DateTime.Now - _lastUpdateCheck >= _updateCheckDelay;

		public static async void CheckForUpdates(bool force = false)
		{
			if(!force && !ShouldCheckForUpdates())
				return;
			_lastUpdateCheck = DateTime.Now;
			try
			{
				bool updated;
				using(var mgr = await GetUpdateManager(false))
					updated = await SquirrelUpdate(mgr, null);

				if(!updated && Config.Instance.CheckForDevUpdates)
				{
					using(var mgr = await GetUpdateManager(true))
						updated = await SquirrelUpdate(mgr, null);
				}

				if(updated)
				{
					_updateCheckDelay = new TimeSpan(1, 0, 0);
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
			if(_releaseUrls != null)
				return _releaseUrls.GetReleaseUrl(release);
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
			_releaseUrls = JsonConvert.DeserializeObject<ReleaseUrls>(fileContent);
			var url = _releaseUrls.GetReleaseUrl(release);
			Log.Info($"using '{release}' release: {url}");
			return url;
		}

		private static async Task<UpdateManager> GetUpdateManager(bool dev)
		{
			if(dev)
				return await UpdateManager.GitHubUpdateManager(await GetReleaseUrl("dev"));
			if(_useChinaMirror)
				return new UpdateManager(await GetReleaseUrl("live-china"));
			return await UpdateManager.GitHubUpdateManager(await GetReleaseUrl("live"), prerelease: Config.Instance.CheckForBetaUpdates);
		}

		public static async Task StartupUpdateCheck(SplashScreenWindow splashScreenWindow)
		{
			try
			{
				Log.Info("Checking for updates");
				bool updated;
				using(var mgr = await GetUpdateManager(false))
				{
					RegistryHelper.SetExecutablePath(Path.Combine(mgr.RootAppDirectory, "Update.exe"));
					RegistryHelper.SetExecutableArgs("--processStart \"HearthstoneDeckTracker.exe\"");
					SquirrelAwareApp.HandleEvents(
						v =>
						{
							mgr.CreateShortcutForThisExe();
							if(Config.Instance.StartWithWindows)
								RegistryHelper.SetRunKey();
						},
						v =>
						{
							mgr.CreateShortcutForThisExe();
							FixStub();
							if(Config.Instance.StartWithWindows)
								RegistryHelper.SetRunKey();
						},
						onAppUninstall: v =>
						{
							mgr.RemoveShortcutForThisExe();
							if(Config.Instance.StartWithWindows)
								RegistryHelper.DeleteRunKey();
						},
						onFirstRun: CleanUpInstallerFile
						);
					updated = await SquirrelUpdate(mgr, splashScreenWindow);
				}

				if(!updated && Config.Instance.CheckForDevUpdates)
				{
					using(var mgr = await GetUpdateManager(true))
						updated = await SquirrelUpdate(mgr, null);
				}

				if(updated)
				{
					if(splashScreenWindow.SkipUpdate)
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

		public static void FixStub()
		{
			var dir = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;
			var stubPath = Path.Combine(dir, "HearthstoneDeckTracker_ExecutionStub.exe");
			if(File.Exists(stubPath))
			{
				var newStubPath = Path.Combine(Directory.GetParent(dir).FullName, "HearthstoneDeckTracker.exe");
				try
				{
					File.Move(stubPath, newStubPath);
				}
				catch(Exception e)
				{
					Log.Error("Could not move ExecutionStub");
				}
			}
		}

		private static void CleanUpInstallerFile()
		{
			try
			{
				var file = Path.Combine(Config.AppDataPath, "HDT-Installer.exe");
				if(File.Exists(file))
					File.Delete(file);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		private static async Task<bool> SquirrelUpdate(UpdateManager mgr, SplashScreenWindow splashScreenWindow, bool ignoreDelta = false)
		{
			try
			{
				Log.Info($"Checking for updates (ignoreDelta={ignoreDelta})");
				splashScreenWindow?.StartSkipTimer();
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
				if(IsRevisionIncrement(current?.Version, latest?.Version))
				{
					Log.Info($"Latest update ({latest}) is revision increment. Updating in background.");
					if(splashScreenWindow != null)
						splashScreenWindow.SkipUpdate = true;
				}
				splashScreenWindow?.ShowConditional();
				Log.Info($"Downloading {updateInfo.ReleasesToApply.Count} {(ignoreDelta ? "" : "delta ")}releases, latest={latest?.Version}");
				if(splashScreenWindow != null)
					await mgr.DownloadReleases(updateInfo.ReleasesToApply, splashScreenWindow.Updating);
				else
					await mgr.DownloadReleases(updateInfo.ReleasesToApply);
				splashScreenWindow?.Updating(100);
				Log.Info("Applying releases");
				if(splashScreenWindow != null)
					await mgr.ApplyReleases(updateInfo, splashScreenWindow.Installing);
				else
					await mgr.ApplyReleases(updateInfo);
				splashScreenWindow?.Installing(100);
				await mgr.CreateUninstallerRegistryEntry();
				Log.Info("Done");
				return true;
			}
			catch(WebException ex)
			{
				Log.Error(ex);
				if(!_useChinaMirror)
				{
					_useChinaMirror = true;
					Log.Warn("Now using china mirror");
					return await SquirrelUpdate(mgr, splashScreenWindow, ignoreDelta);
				}
				return false;
			}
			catch(Exception ex)
			{
				if(ignoreDelta)
					return false;
				if(ex is Win32Exception)
					Log.Info("Not able to apply deltas, downloading full release");
				return await SquirrelUpdate(mgr, splashScreenWindow, true);
			}
		}

		private static bool IsRevisionIncrement(Version current, Version latest)
		{
			if(current == null || latest == null)
				return false;
			return current.Major == latest.Major && current.Minor == latest.Minor && current.Build == latest.Build
					&& current.Revision < latest.Revision;
		}

		internal static void StartUpdate()
		{
			Log.Info("Restarting...");
			UpdateManager.RestartApp();
		}
	}
}

#endif
