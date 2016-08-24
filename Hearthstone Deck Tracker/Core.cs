#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Controls.Stats;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.HotKeys;
using Hearthstone_Deck_Tracker.Utility.LogConfig;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;
using Hearthstone_Deck_Tracker.Utility.Themes;
using Hearthstone_Deck_Tracker.Utility.Updating;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public static class Core
	{
		internal const int UpdateDelay = 100;
		private static TrayIcon _trayIcon;
		private static OverlayWindow _overlay;
		private static Overview _statsOverview;
		private static int _updateRequestsPlayer;
		private static int _updateRequestsOpponent;
		private static DateTime _startUpTime;
		public static Version Version { get; set; }
		public static GameV2 Game { get; set; }
		public static MainWindow MainWindow { get; set; }

		public static Overview StatsOverview => _statsOverview ?? (_statsOverview = new Overview());

		public static bool Initialized { get; private set; }

		public static TrayIcon TrayIcon => _trayIcon ?? (_trayIcon = new TrayIcon());

		public static OverlayWindow Overlay => _overlay ?? (_overlay = new OverlayWindow(Game));

		internal static bool UpdateOverlay { get; set; } = true;
		internal static bool Update { get; set; }
		internal static bool CanShutdown { get; set; }

		public static async void Initialize()
		{
			_startUpTime = DateTime.UtcNow;
			Log.Info($"HDT: {Helper.GetCurrentVersion()}, Operating System: {Helper.GetWindowsVersion()}, .NET Framework: {Helper.GetInstalledDotNetVersion()}");
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
			Config.Load();
			var splashScreenWindow = new SplashScreenWindow();
#if(SQUIRREL)
			if(Config.Instance.CheckForUpdates)
			{
				var updateCheck = Updater.StartupUpdateCheck(splashScreenWindow);
				while(!updateCheck.IsCompleted)
				{
					await Task.Delay(500);
					if(splashScreenWindow.SkipUpdate)
						break;
				}
			}
#endif
			splashScreenWindow.ShowConditional();
			Log.Initialize();
			ConfigManager.Run();
			var newUser = ConfigManager.PreviousVersion == null;
			LogConfigUpdater.Run().Forget();
			LogConfigWatcher.Start();
			Helper.UpdateAppTheme();
			ThemeManager.Run();
			ResourceMonitor.Run();
			Game = new GameV2();
			LoginType loginType;
			var loggedIn = HearthStatsAPI.LoadCredentials();
			if(!loggedIn && Config.Instance.ShowLoginDialog)
			{
				var loginWindow = new LoginWindow();
				splashScreenWindow.Close();
				loginWindow.ShowDialog();
				if(loginWindow.LoginResult == LoginType.None)
				{
					Application.Current.Shutdown();
					return;
				}
				loginType = loginWindow.LoginResult;
				splashScreenWindow = new SplashScreenWindow();
				splashScreenWindow.ShowConditional();
			}
			else
				loginType = loggedIn ? LoginType.AutoLogin : LoginType.AutoGuest;
			MainWindow = new MainWindow();
			MainWindow.LoadConfigSettings();
			if(Config.Instance.ReselectLastDeckUsed)
			{
				MainWindow.SelectLastUsedDeck();
				Config.Instance.ReselectLastDeckUsed = false;
				Config.Save();
			}
			MainWindow.Show();
			splashScreenWindow.Close();

			if(Config.Instance.DisplayHsReplayNote)
				MainWindow.FlyoutHsReplayNote.IsOpen = true;

			if(ConfigManager.UpdatedVersion != null)
			{
#if(!SQUIRREL)
				Updater.Cleanup();
#endif
				MainWindow.FlyoutUpdateNotes.IsOpen = true;
				MainWindow.UpdateNotesControl.SetHighlight(ConfigManager.PreviousVersion);
				MainWindow.UpdateNotesControl.LoadUpdateNotes();
			}
			NetDeck.CheckForChromeExtention();
			DataIssueResolver.Run();

#if(!SQUIRREL)
			Helper.CopyReplayFiles();
#endif
			BackupManager.Run();

			if(Config.Instance.PlayerWindowOnStart)
				Windows.PlayerWindow.Show();
			if(Config.Instance.OpponentWindowOnStart)
				Windows.OpponentWindow.Show();
			if(Config.Instance.TimerWindowOnStartup)
				Windows.TimerWindow.Show();

			if(Config.Instance.HearthStatsSyncOnStart && HearthStatsAPI.IsLoggedIn)
				HearthStatsManager.SyncAsync(background: true);

			PluginManager.Instance.LoadPlugins();
			MainWindow.Options.OptionsTrackerPlugins.Load();
			PluginManager.Instance.StartUpdateAsync();

			UpdateOverlayAsync();

			if(Config.Instance.ShowCapturableOverlay)
			{
				Windows.CapturableOverlay = new CapturableOverlayWindow();
				Windows.CapturableOverlay.Show();
			}

			if(LogConfigUpdater.LogConfigUpdateFailed)
				MainWindow.ShowLogConfigUpdateFailedMessage().Forget();
			else if(LogConfigUpdater.LogConfigUpdated && Game.IsRunning)
			{
				MainWindow.ShowMessageAsync("Hearthstone restart required", "The log.config file has been updated. HDT may not work properly until Hearthstone has been restarted.");
				Overlay.ShowRestartRequiredWarning();
			}
			LogReaderManager.Start(Game).Forget();

			NewsUpdater.UpdateAsync();
			HotKeyManager.Load();

			if(Helper.HearthstoneDirExists && Config.Instance.StartHearthstoneWithHDT && !Game.IsRunning)
				Helper.StartHearthstoneAsync().Forget();

			ApiWrapper.UpdateAccountStatus().Forget();

			Initialized = true;

			Influx.OnAppStart(Helper.GetCurrentVersion(), loginType, newUser, (int)(DateTime.UtcNow - _startUpTime).TotalSeconds);
		}

		private static async void UpdateOverlayAsync()
		{
#if(!SQUIRREL)
			if(Config.Instance.CheckForUpdates)
				Updater.CheckForUpdates(true);
#endif
			var hsForegroundChanged = false;
			var useNoDeckMenuItem = TrayIcon.NotifyIcon.ContextMenu.MenuItems.IndexOfKey("startHearthstone");
			while(UpdateOverlay)
			{
				if(Config.Instance.CheckForUpdates)
					Updater.CheckForUpdates();
				if(User32.GetHearthstoneWindow() != IntPtr.Zero)
				{
					if(Game.CurrentRegion == Region.UNKNOWN)
					{
						//game started
						Game.CurrentRegion = Helper.GetCurrentRegion();
						if(Game.CurrentRegion != Region.UNKNOWN)
						{
							BackupManager.Run();
							Game.MetaData.HearthstoneBuild = null;
						}
					}
					Overlay.UpdatePosition();

					if(!Game.IsRunning)
					{
						Overlay.Update(true);
						Windows.CapturableOverlay?.UpdateContentVisibility();
					}

					MainWindow.BtnStartHearthstone.Visibility = Visibility.Collapsed;
					TrayIcon.NotifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Visible = false;

					Game.IsRunning = true;

					Helper.GameWindowState = User32.GetHearthstoneWindowState();
					Windows.CapturableOverlay?.Update();
					if(User32.IsHearthstoneInForeground() && Helper.GameWindowState != WindowState.Minimized)
					{
						if(hsForegroundChanged)
						{
							Overlay.Update(true);
							if(Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
							{
								//if player topmost is set to true before opponent:
								//clicking on the playerwindow and back to hs causes the playerwindow to be behind hs.
								//other way around it works for both windows... what?
								Windows.OpponentWindow.Topmost = true;
								Windows.PlayerWindow.Topmost = true;
								Windows.TimerWindow.Topmost = true;
							}
							hsForegroundChanged = false;
						}
					}
					else if(!hsForegroundChanged)
					{
						if(Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
						{
							Windows.PlayerWindow.Topmost = false;
							Windows.OpponentWindow.Topmost = false;
							Windows.TimerWindow.Topmost = false;
						}
						hsForegroundChanged = true;
					}
				}
				else if(Game.IsRunning)
				{
					Game.IsRunning = false;
					Overlay.ShowOverlay(false);
					if(Windows.CapturableOverlay != null)
					{
						Windows.CapturableOverlay.UpdateContentVisibility();
						await Task.Delay(100);
						Windows.CapturableOverlay.ForcedWindowState = WindowState.Minimized;
						Windows.CapturableOverlay.WindowState = WindowState.Minimized;
					}
					Log.Info("Exited game");
					Game.CurrentRegion = Region.UNKNOWN;
					Log.Info("Reset region");
					await Reset();
					Game.IsInMenu = true;
					Game.InvalidateMatchInfoCache();
					Overlay.HideRestartRequiredWarning();
					Helper.ClearCachedHearthstoneBuild();
					TurnTimer.Instance.Stop();

					MainWindow.BtnStartHearthstone.Visibility = Visibility.Visible;
					TrayIcon.NotifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Visible = true;

					if(Config.Instance.CloseWithHearthstone)
						MainWindow.Close();
				}

				if(Config.Instance.NetDeckClipboardCheck.HasValue && Config.Instance.NetDeckClipboardCheck.Value && Initialized
				   && !User32.IsHearthstoneInForeground())
					NetDeck.CheckForClipboardImport();

				await Task.Delay(UpdateDelay);
			}
			CanShutdown = true;
		}

		private static bool _resetting;
		public static async Task Reset()
		{

			if(_resetting)
			{
				Log.Warn("Reset already in progress.");
				return;
			}
			_resetting = true;
			var stoppedReader = await LogReaderManager.Stop();
			Game.Reset();
			if(DeckList.Instance.ActiveDeck != null)
			{
				Game.IsUsingPremade = true;
				MainWindow.UpdateMenuItemVisibility();
			}
			await Task.Delay(1000);
			if(stoppedReader)
				LogReaderManager.Restart();
			Overlay.HideSecrets();
			Overlay.Update(false);
			UpdatePlayerCards(true);
			_resetting = false;
		}

		internal static async void UpdatePlayerCards(bool reset = false)
		{
			_updateRequestsPlayer++;
			await Task.Delay(100);
			_updateRequestsPlayer--;
			if(_updateRequestsPlayer > 0)
				return;
			Overlay.UpdatePlayerCards(new List<Card>(Game.Player.PlayerCardList), reset);
			if(Windows.PlayerWindow.IsVisible)
				Windows.PlayerWindow.UpdatePlayerCards(new List<Card>(Game.Player.PlayerCardList), reset);
		}

		internal static async void UpdateOpponentCards(bool reset = false)
		{
			_updateRequestsOpponent++;
			await Task.Delay(100);
			_updateRequestsOpponent--;
			if(_updateRequestsOpponent > 0)
				return;
			Overlay.UpdateOpponentCards(new List<Card>(Game.Opponent.OpponentCardList), reset);
			if(Windows.OpponentWindow.IsVisible)
				Windows.OpponentWindow.UpdateOpponentCards(new List<Card>(Game.Opponent.OpponentCardList), reset);
		}


		public static class Windows
		{
			private static PlayerWindow _playerWindow;
			private static OpponentWindow _opponentWindow;
			private static TimerWindow _timerWindow;
			private static StatsWindow _statsWindow;

			public static PlayerWindow PlayerWindow => _playerWindow ?? (_playerWindow = new PlayerWindow(Game));
			public static OpponentWindow OpponentWindow => _opponentWindow ?? (_opponentWindow = new OpponentWindow(Game));
			public static TimerWindow TimerWindow => _timerWindow ?? (_timerWindow = new TimerWindow(Config.Instance));
			public static StatsWindow StatsWindow => _statsWindow ?? (_statsWindow = new StatsWindow());
			public static CapturableOverlayWindow CapturableOverlay;
		}
	}
}