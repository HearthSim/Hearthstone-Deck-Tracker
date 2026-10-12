#if(VELOPACK)
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace Hearthstone_Deck_Tracker.Utility.Updating
{
	internal static partial class Updater
	{
		private const int UpdateCheckTimeout     =  2_000;
		private const int InitialDownloadTimeout =  3_000;
		private const int TotalDownloadTimeout   = 20_000;

		private static TimeSpan _updateCheckDelay = new TimeSpan(0, 20, 0);
		private static bool ShouldCheckForUpdates()
			=> Config.Instance.CheckForUpdates && DateTime.Now - _lastUpdateCheck >= _updateCheckDelay;

		public static async void CheckForUpdates(bool force = false)
		{
			// An update has already been downloaded and installed, and is just waiting for a
			// restart. Checking again would reset the status bar (UpdaterState.Checking/None
			// both hide the "click to update" message) while leaving it visible but empty.
			if(Status.UpdaterState == UpdaterState.Available)
				return;
			if(!force && !ShouldCheckForUpdates())
				return;
			_lastUpdateCheck = DateTime.Now;
			try
			{
				var mgr = GetUpdateManager(false);
				var updateInfo = await SquirrelUpdate(mgr, false, false);

				if(updateInfo == null && Config.Instance.CheckForDevUpdates)
				{
					mgr = GetUpdateManager(true);
					updateInfo = await SquirrelUpdate(mgr, true, false);
				}

				if(updateInfo != null)
				{
					_updateCheckDelay = new TimeSpan(1, 0, 0);
					Status.StatusBarVisibility = Visibility.Visible;
				}
			}
			catch(HttpRequestException ex)
			{
				FindBestMirror();
				Status.OnFailed(ex);
				Log.Error(ex);
			}
			catch(Exception ex)
			{
				Status.OnFailed(ex);
				Log.Error(ex);
			}
		}

		private const string DevReleaseUrl = "https://github.com/HearthSim/HDT-dev-builds";

		private static UpdateManager GetUpdateManager(bool dev)
		{
			if(dev)
				return new UpdateManager(new GithubSource(DevReleaseUrl, null, false));

			var (mirror, url) = GetCurrentMirror();
			return mirror switch
			{
				UpdateMirror.Github => new UpdateManager(new GithubSource(url, null, false)),
				_ => new UpdateManager(url)
			};
		}

		private static void UpdateRunKey()
		{
			var locator = VelopackLocator.Current;
			if(locator.CurrentlyInstalledVersion == null || locator.IsPortable || locator.AppContentDir == null)
				return;
			RegistryHelper.SetExecutablePath(Path.Combine(locator.AppContentDir, "HearthstoneDeckTracker.exe"));
			if(Config.Instance.StartWithWindows)
				RegistryHelper.SetRunKey();
		}

		// Squirrel's stub starts the newest app-* folder directly, which no longer exists after the migration
		private static void ReplaceSquirrelStub()
		{
			var locator = VelopackLocator.Current;
			if(locator.CurrentlyInstalledVersion == null || locator.IsPortable || locator.RootAppDir == null)
				return;
			var squirrelStub = Path.Combine(locator.RootAppDir, "HearthstoneDeckTracker.exe");
			// named after --packTitle in build-scripts/package.ps1
			var velopackStub = Path.Combine(locator.RootAppDir, "Hearthstone Deck Tracker.exe");
			if(!File.Exists(squirrelStub) || !File.Exists(velopackStub))
				return;
			if(File.ReadAllBytes(squirrelStub).SequenceEqual(File.ReadAllBytes(velopackStub)))
				return;
			File.Copy(velopackStub, squirrelStub, true);
			Log.Info("Replaced the Squirrel stub with a copy of the Velopack stub");
		}

		public static void SquirrelInit()
		{
			try
			{
				// Should be safe, but we really want to make sure not to crash here.
				try
				{
					Config.Load();
				}
				catch
				{
					// It's fine.
				}

				// applying here runs before the single instance check, and Update.exe force-stops a running HDT (see ApplyPendingUpdate)
				VelopackApp.Build()
					.SetAutoApplyOnStartup(false)
					.SetAppUserModelId(App.AppUserModelId)
					.OnAfterInstallFastCallback(v => UpdateRunKey())
					.OnAfterUpdateFastCallback(v => UpdateRunKey())
					.OnBeforeUninstallFastCallback(v =>
						{
							if(Config.Instance.StartWithWindows)
								RegistryHelper.DeleteRunKey();
							BattlegroundsChinaModule.RemoveHDTToolsTask()
								.ConfigureAwait(false)
								.GetAwaiter()
								.GetResult();
						}
					)
					.Run();

				// also repoint run keys that still launch through Squirrel's Update.exe
				UpdateRunKey();
				ReplaceSquirrelStub();
			}
			catch(Exception ex)
			{
				// This isn't good, and most likely means something about
				// the installation is wrong, but HDT should work regardless.
				Log.Error(ex);
			}
		}

		public static void ApplyPendingUpdate()
		{
			var mgr = GetUpdateManager(false);
			if(mgr.UpdatePendingRestart is not { } pending)
				return;
			Log.Info($"Applying downloaded update {pending.Version}");
			ApplyUpdateAndRestart(mgr, pending);
		}

		public static async Task StartupUpdateCheck()
		{
			try
			{
				Log.Info($"Checking for updates, using {Config.Instance.SquirrelRemote}");
				_lastUpdateCheck = DateTime.Now;
				var mgr = GetUpdateManager(false);

				var updateInfo = await SquirrelUpdate(mgr, false, true);
				if(updateInfo == null && Config.Instance.CheckForDevUpdates)
				{
					mgr = GetUpdateManager(true);
					updateInfo = await SquirrelUpdate(mgr, true, true);
				}

				if(updateInfo != null)
				{
					if(Status.SkipStartupCheck)
					{
						Log.Info("Update complete, showing update bar");
						Status.StatusBarVisibility = Visibility.Visible;
					}
					else
					{
						Log.Info("Update complete, restarting");
						ApplyUpdateAndRestart(mgr, updateInfo);
					}
				}
			}
			catch(HttpRequestException ex)
			{
				FindBestMirror();
				Status.OnFailed(ex);
				Log.Error(ex);
			}
			catch(Exception ex)
			{
				Status.OnFailed(ex);
				Log.Error(ex);
			}
		}

		private static async Task<UpdateInfo?> SquirrelUpdate(UpdateManager mgr, bool dev, bool isStartupCheck)
		{
			try
			{
				Status.UpdaterState = UpdaterState.Checking;
				Log.Info($"Checking for updates");
				var updateInfoTask = mgr.CheckForUpdatesAsync();

				if(isStartupCheck)
				{
					// If it takes more than two seconds to get the update info
					// i.e. the RELEASES file, just update in the background.
					var task = await Task.WhenAny(updateInfoTask,
						Task.Delay(UpdateCheckTimeout));
					if(task != updateInfoTask)
					{
						Log.Warn(
							$"Update check took longer than {UpdateCheckTimeout}ms{(Status.SkipStartupCheck ? "" : ", showing app")}");
						Status.SkipStartupCheck = true;
						if(!dev)
							FindBestMirror();
					}
				}

				var updateInfo = await updateInfoTask;
				if(updateInfo == null)
				{
					Log.Info("No new update available");
					Status.UpdaterState = UpdaterState.None;
					return null;
				}

				if(isStartupCheck)
				{
					Status.OnDownloadProgressChanged(0);
					var downloadTask = mgr.DownloadUpdatesAsync(
						updateInfo,
						Status.OnDownloadProgressChanged
					);

					var task = await Task.WhenAny(downloadTask,
						Task.Delay(InitialDownloadTimeout));
					if(task != downloadTask && Status.UpdateProgress < 10)
					{
						Log.Warn(
							$"Downloading the update is slow{(Status.SkipStartupCheck ? "" : ", showing app")}");
						Status.SkipStartupCheck = true;
						if(!dev)
							FindBestMirror();
					}

					// If the rest of download takes longer than 20 seconds in
					// total also show the app and continue in the background.
					task = await Task.WhenAny(downloadTask,
						Task.Delay(TotalDownloadTimeout));
					if(task != downloadTask)
					{
						Log.Warn(
							$"Downloading the update is taking longer than {TotalDownloadTimeout}ms{(Status.SkipStartupCheck ? "" : ", showing app")}");
						Status.SkipStartupCheck = true;
						if(!dev)
							FindBestMirror();
					}

					await downloadTask;
					Status.OnDownloadProgressChanged(100);
				}
				else
				{
					await mgr.DownloadUpdatesAsync(updateInfo);
				}

				Status.OnInstalled();
				Log.Info("Done");
				return updateInfo;
			}
			catch(HttpRequestException ex)
			{
				if(!dev)
					FindBestMirror();
				Log.Error(ex);
				Status.OnFailed(ex);
				return null;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				Status.OnFailed(ex);
				return null;
			}
			finally
			{
				Status.UpdateProgress = 0;
			}
		}

		public static void RestartApp()
		{
			Log.Info("Restarting...");
			var mgr = GetUpdateManager(false);
			if(mgr.UpdatePendingRestart is { } pending)
				ApplyUpdateAndRestart(mgr, pending);
			else
			{
				UpdateExe.Start(waitPid: VelopackLocator.Current.Process.GetCurrentProcessId());
				Environment.Exit(0);
			}
		}

		private static void ApplyUpdateAndRestart(UpdateManager mgr, VelopackAsset asset)
		{
			// silent also hides Update.exe's error dialogs, so a failed apply only shows up in Velopack's log
			mgr.WaitExitThenApplyUpdates(asset, silent: true, restart: true);
			Environment.Exit(0);
		}
	}
}

#endif
