#if(SQUIRREL)
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Squirrel;

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
			if(!force && !ShouldCheckForUpdates())
				return;
			_lastUpdateCheck = DateTime.Now;
			try
			{
				bool updated;
				using(var mgr = await GetUpdateManager(false))
					updated = await SquirrelUpdate(mgr, false);

				if(!updated && Config.Instance.CheckForDevUpdates)
				{
					using(var mgr = await GetUpdateManager(true))
						updated = await SquirrelUpdate(mgr, false);
				}

				if(updated)
				{
					_updateCheckDelay = new TimeSpan(1, 0, 0);
					Status.StatusBarVisibility = Visibility.Visible;
				}
			}
			catch(WebException ex)
			{
				SquirrelConnection.FindBestRemote();
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

		private static async Task<UpdateManager> GetUpdateManager(bool dev)
		{
			if(dev)
				return await UpdateManager.GitHubUpdateManager(DevReleaseUrl);

			var (remote, url) = SquirrelConnection.GetCurrentRemote();
			return remote switch
			{
				SquirrelRemote.Github => await UpdateManager.GitHubUpdateManager(url),
				_ => new UpdateManager(url)
			};
		}

		public static void SquirrelInit()
		{
			try
			{
				// We don't know what url we want to use at this point, and it should
				// not matter. We only want this instance of the UpdateManager to
				// manage the local installation. We will instantiate a different one
				// later on for actual updates.
				using var initMgr = new UpdateManager("INVALID_PATH");

				RegistryHelper.SetExecutablePath(Path.Combine(initMgr.RootAppDirectory, "Update.exe"));
				RegistryHelper.SetExecutableArgs("--processStart \"HearthstoneDeckTracker.exe\"");

				// Should be safe, but we really want to make sure not to crash here.
				try
				{
					Config.Load();
				}
				catch
				{
					// It's fine.
				}

				SquirrelAwareApp.HandleEvents(
					v =>
					{
						initMgr.CreateShortcutForThisExe();
						if(Config.Instance.StartWithWindows)
							RegistryHelper.SetRunKey();
					},
					v =>
					{
						initMgr.CreateShortcutForThisExe();
						FixStub();
						if(Config.Instance.StartWithWindows)
							RegistryHelper.SetRunKey();
					},
					onAppUninstall: v =>
					{
						initMgr.RemoveShortcutForThisExe();

						BattlegroundsChinaModule.RemoveHDTToolsTask()
							.ConfigureAwait(false)
							.GetAwaiter()
							.GetResult();

						if(Config.Instance.StartWithWindows)
							RegistryHelper.DeleteRunKey();
					},
					onFirstRun: CleanUpInstallerFile
				);
			}
			catch(Exception ex)
			{
				// This isn't good, and most likely means something about
				// the installation is wrong, but HDT should work regardless.
				Log.Error(ex);
			}
		}

		public static async Task StartupUpdateCheck()
		{
			try
			{
				Log.Info($"Checking for updates, using {Config.Instance.SquirrelRemote}");
				_lastUpdateCheck = DateTime.Now;

				async Task<bool> Update(bool dev)
				{
					var mgrTask = GetUpdateManager(dev);
					var task = await Task.WhenAny(mgrTask, Task.Delay(UpdateCheckTimeout));
					if(task != mgrTask)
					{
						Log.Warn($"UpdateManager{(dev ? " (dev)" : "")} init took longer than {UpdateCheckTimeout}ms{(Status.SkipStartupCheck ? "" : ", showing app")}");
						Status.SkipStartupCheck = true;
						SquirrelConnection.FindBestRemote();
					}
					using var mgr = await mgrTask;
					return await SquirrelUpdate(mgr, true);
				}

				var updated = await Update(false);
				if(!updated && Config.Instance.CheckForDevUpdates)
					updated = await Update(true);

				if(updated)
				{
					if(Status.SkipStartupCheck)
					{
						Log.Info("Update complete, showing update bar");
						Status.StatusBarVisibility = Visibility.Visible;
					}
					else
					{
						Log.Info("Update complete, restarting");
						UpdateManager.RestartApp();
					}
				}
			}
			catch(WebException ex)
			{
				SquirrelConnection.FindBestRemote();
				Status.OnFailed(ex);
				Log.Error(ex);
			}
			catch(Exception ex)
			{
				Status.OnFailed(ex);
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

		private static async Task<bool> SquirrelUpdate(UpdateManager mgr, bool isStartupCheck, bool ignoreDelta = false)
		{
			try
			{
				Status.UpdaterState = UpdaterState.Checking;
				Log.Info($"Checking for updates (ignoreDelta={ignoreDelta})");
				var updateInfoTask = mgr.CheckForUpdate(ignoreDelta);

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
						SquirrelConnection.FindBestRemote();
					}
				}

				var updateInfo = await updateInfoTask;
				if(!updateInfo.ReleasesToApply.Any())
				{
					Log.Info("No new update available");
					Status.UpdaterState = UpdaterState.None;
					return false;
				}

				var latest = updateInfo.ReleasesToApply.LastOrDefault()
					?.Version;
				var current = mgr.CurrentlyInstalledVersion();
				if(latest <= current)
				{
					Log.Info(
						$"Installed version ({current}) is greater than latest release found ({latest}). Not downloading updates.");
					Status.UpdaterState = UpdaterState.None;
					return false;
				}

				Log.Info(
					$"Downloading {updateInfo.ReleasesToApply.Count} {(ignoreDelta ? "" : "delta ")}releases, latest={latest?.Version}");
				if(isStartupCheck)
				{
					Status.OnDownloadProgressChanged(0);
					var downloadTask = mgr.DownloadReleases(
						updateInfo.ReleasesToApply,
						Status.OnDownloadProgressChanged);

					var task = await Task.WhenAny(downloadTask,
						Task.Delay(InitialDownloadTimeout));
					if(task != downloadTask && Status.UpdateProgress < 10)
					{
						Log.Warn(
							$"Downloading the update is slow{(Status.SkipStartupCheck ? "" : ", showing app")}");
						Status.SkipStartupCheck = true;
						SquirrelConnection.FindBestRemote();
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
						SquirrelConnection.FindBestRemote();
					}

					await downloadTask;
					Status.OnDownloadProgressChanged(100);
				}
				else
				{
					await mgr.DownloadReleases(updateInfo.ReleasesToApply);
				}

				Log.Info("Applying releases");
				if(isStartupCheck)
				{
					await mgr.ApplyReleases(updateInfo,
						Status.OnInstallProgressChanged);
					Status.OnInstallProgressChanged(100);
				}
				else
				{
					await mgr.ApplyReleases(updateInfo);
				}

				await mgr.CreateUninstallerRegistryEntry();
				Status.OnInstalled();
				Log.Info("Done");
				return true;
			}
			catch(WebException ex)
			{
				SquirrelConnection.FindBestRemote();
				Log.Error(ex);
				Status.OnFailed(ex);
				return false;
			}
			catch(Exception ex)
			{
				if(ignoreDelta)
				{
					Status.OnFailed(ex);
					return false;
				}
				if(ex is Win32Exception)
					Log.Info(
						"Not able to apply deltas, downloading full release");
				else
					Log.Error(ex);
				return await SquirrelUpdate(mgr, isStartupCheck, true);
			}
			finally
			{
				Status.UpdateProgress = 0;
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
