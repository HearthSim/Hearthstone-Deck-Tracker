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
using Squirrel;

namespace Hearthstone_Deck_Tracker.Utility.Updating
{
	internal static partial class Updater
	{
		private static bool _useChinaMirror = CultureInfo.CurrentCulture.Name == "zh-CN";
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

		private const string DevReleaseUrl = "https://github.com/HearthSim/HDT-dev-builds";
		private const string LiveReleaseUrl = "https://github.com/HearthSim/HDT-Releases";
		private const string LiveAsiaReleaseUrl = "https://hdt-downloads-asia.s3-accelerate.dualstack.amazonaws.com";

		private static async Task<UpdateManager> GetUpdateManager(bool dev)
		{
			if(dev)
				return await UpdateManager.GitHubUpdateManager(DevReleaseUrl);
			if(_useChinaMirror)
				return new UpdateManager(LiveAsiaReleaseUrl);
			return await UpdateManager.GitHubUpdateManager(LiveReleaseUrl, prerelease: Config.Instance.CheckForBetaUpdates);
		}

		public static async Task StartupUpdateCheck(SplashScreenWindow splashScreenWindow)
		{
			try
			{
				Log.Info("Checking for updates");
				_lastUpdateCheck = DateTime.Now;
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
						updated = await SquirrelUpdate(mgr, splashScreenWindow);
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
				catch
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

		private const int UpdateCheckTimeout     =  2_000;
		private const int InitialDownloadTimeout =  3_000;
		private const int TotalDownloadTimeout   = 20_000;

		private static async Task<bool> SquirrelUpdate(UpdateManager mgr, SplashScreenWindow? splashScreenWindow, bool ignoreDelta = false)
		{
			try
			{
				Log.Info($"Checking for updates (ignoreDelta={ignoreDelta})");
				var updateInfoTask = mgr.CheckForUpdate(ignoreDelta);

				if(splashScreenWindow != null)
				{
					// If it takes more than two seconds to get the update info
					// i.e. the RELEASES file, just update in the background.
					var task = await Task.WhenAny(updateInfoTask, Task.Delay(UpdateCheckTimeout));
					if(task != updateInfoTask)
					{
						Log.Warn($"Update check took longer than {UpdateCheckTimeout}ms, showing app");
						splashScreenWindow.SkipUpdate = true;
					}
				}

				var updateInfo = await updateInfoTask;
				if(!updateInfo.ReleasesToApply.Any())
				{
					Log.Info("No new update available");
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
				if(splashScreenWindow != null)
				{
					var start = DateTime.Now;

					var showProgress = false;
					var progressPct = 0;
					void OnProgress(int progress)
					{
						progressPct = progress;
						// Only show progress if we detemined the update is fast
						// enough to avoid a confusing splashscreen that goes from
						// "updating" to the app, which then shows "update available"
						// shortly after.
						if(showProgress)
							splashScreenWindow.Updating(progress);
					}

					var downloadTask = mgr.DownloadReleases(updateInfo.ReleasesToApply, OnProgress);

					// If the first 10 percent take longer than 3 seconds show
					// the app and continue the download in the background.
					while(DateTime.Now.Subtract(start).TotalMilliseconds < InitialDownloadTimeout)
					{
						if(progressPct >= 10)
						{
							showProgress = true;
							break;
						}
						await Task.Delay(16);
					}

					if(showProgress)
						splashScreenWindow.Updating(progressPct);
					else
					{
						Log.Warn($"Downloading the update is too slow, showing app");
						splashScreenWindow.SkipUpdate = true;
					}

					// If the rest of download takes longer than 20 seconds in
					// total also show the app and continue in the background.
					var task = await Task.WhenAny(downloadTask, Task.Delay(TotalDownloadTimeout));
					if(task != downloadTask)
					{
						Log.Warn($"Downloading the update is too slow, showing app");
						splashScreenWindow.SkipUpdate = true;
					}

					await downloadTask;
					splashScreenWindow.Updating(100);
				}
				else
				{
					await mgr.DownloadReleases(updateInfo.ReleasesToApply);
				}

				Log.Info("Applying releases");
				if(splashScreenWindow != null)
				{
					await mgr.ApplyReleases(updateInfo, splashScreenWindow.Installing);
					splashScreenWindow.Installing(100);
				}
				else
				{
					await mgr.ApplyReleases(updateInfo);
				}

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
				else
					Log.Error(ex);
				return await SquirrelUpdate(mgr, splashScreenWindow, true);
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
