#region

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Controls.Stats;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Controls.Information;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.HotKeys;
using Hearthstone_Deck_Tracker.Utility.LogConfig;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;
using Hearthstone_Deck_Tracker.Utility.Themes;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public static class Core
	{
		private static TrayIcon _trayIcon;
		private static OverlayWindow _overlay;
		private static Overview _statsOverview;
		private static int _updateRequestsPlayer;
		private static int _updateRequestsOpponent;
		public static Version Version { get; set; }
		public static GameV2 Game { get; set; }
		public static MainWindow MainWindow { get; set; }

		public static Overview StatsOverview => _statsOverview ?? (_statsOverview = new Overview());

		public static bool Initialized { get; private set; }

		public static TrayIcon TrayIcon => _trayIcon ?? (_trayIcon = new TrayIcon());

		public static OverlayWindow Overlay => _overlay ?? (_overlay = new OverlayWindow(Game));

		internal static bool UpdateOverlay { get; set; }
		internal static bool Update { get; set; }
		internal static bool CanShutdown { get; set; }

		public static void Initialize()
		{
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
			var newUser = !Directory.Exists(Config.AppDataPath);
			Config.Load();
			Log.Initialize();
			ConfigManager.Run();
			LogConfigUpdater.Run().Forget();
			LogConfigWatcher.Start();
			Helper.UpdateAppTheme();
			ThemeManager.Run();
			var splashScreenWindow = new SplashScreenWindow();
			splashScreenWindow.ShowConditional();
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

			if(ConfigManager.UpdatedVersion != null)
			{
				Updater.Cleanup();
				MainWindow.FlyoutUpdateNotes.IsOpen = true;
				MainWindow.UpdateNotesControl.SetHighlight(ConfigManager.PreviousVersion);
				MainWindow.UpdateNotesControl.LoadUpdateNotes();
			}
			NetDeck.CheckForChromeExtention();
			DataIssueResolver.Run();

			Helper.CopyReplayFiles();
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

			if(Helper.HearthstoneDirExists)
			{
				if(LogConfigUpdater.LogConfigUpdateFailed)
					MainWindow.ShowLogConfigUpdateFailedMessage().Forget();
				else if(LogConfigUpdater.LogConfigUpdated && Game.IsRunning)
				{
					MainWindow.ShowMessageAsync("Hearthstone restart required", "The log.config file has been updated. HDT may not work properly until Hearthstone has been restarted.");
					Overlay.ShowRestartRequiredWarning();
				}
				LogReaderManager.Start(Game);
			}
			else
				MainWindow.ShowHsNotInstalledMessage().Forget();

			NewsUpdater.UpdateAsync();
			HotKeyManager.Load();

			if(Helper.HearthstoneDirExists && Config.Instance.StartHearthstoneWithHDT && !Game.IsRunning)
				Helper.StartHearthstoneAsync();

			HsReplayManager.Setup().Forget();

			Initialized = true;

			Analytics.Analytics.TrackPageView($"/app/v{Helper.GetCurrentVersion().ToVersionString()}/{loginType.ToString().ToLower()}{(newUser ? "/new" : "")}", "");
		}

		private static async void UpdateOverlayAsync()
		{
			if(Config.Instance.CheckForUpdates)
				Updater.CheckForUpdates(true);
			var hsForegroundChanged = false;
			var useNoDeckMenuItem = TrayIcon.NotifyIcon.ContextMenu.MenuItems.IndexOfKey("startHearthstone");
			UpdateOverlay = Helper.HearthstoneDirExists;
			while(UpdateOverlay)
			{
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

					if(Config.Instance.CheckForUpdates)
						Updater.CheckForUpdates();

					if(!Game.IsRunning)
						Overlay.Update(true);

					MainWindow.BtnStartHearthstone.Visibility = Visibility.Collapsed;
					TrayIcon.NotifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Visible = false;

					Game.IsRunning = true;
					if(User32.IsHearthstoneInForeground())
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
				else
				{
					Overlay.ShowOverlay(false);
					if(Game.IsRunning)
					{
						Log.Info("Exited game");
						Game.CurrentRegion = Region.UNKNOWN;
						Log.Info("Reset region");
						await Reset();
						Game.IsInMenu = true;
						Overlay.HideRestartRequiredWarning();

						MainWindow.BtnStartHearthstone.Visibility = Visibility.Visible;
						TrayIcon.NotifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Visible = true;

						if(Config.Instance.CloseWithHearthstone)
							MainWindow.Close();
					}
					Game.IsRunning = false;
				}

				if(Config.Instance.NetDeckClipboardCheck.HasValue && Config.Instance.NetDeckClipboardCheck.Value && Initialized
				   && !User32.IsHearthstoneInForeground())
					NetDeck.CheckForClipboardImport();

				await Task.Delay(Config.Instance.UpdateDelay);
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
			var cards = Game.Player.PlayerCardList;
			Overlay.UpdatePlayerCards(cards, reset);
			Windows.PlayerWindow.UpdatePlayerCards(cards, reset);
		}

		internal static async void UpdateOpponentCards(bool reset = false)
		{
			_updateRequestsOpponent++;
			await Task.Delay(100);
			_updateRequestsOpponent--;
			if(_updateRequestsOpponent > 0)
				return;
			var cards = Game.Opponent.OpponentCardList;
			Overlay.UpdateOpponentCards(cards, reset);
			Windows.OpponentWindow.UpdateOpponentCards(cards, reset);
		}


		public static class Windows
		{
			private static PlayerWindow _playerWindow;
			private static OpponentWindow _opponentWindow;
			private static TimerWindow _timerWindow;
			private static StatsWindow _statsWindow;
			private static StatsWindow_New _newStatsWindow;

			public static PlayerWindow PlayerWindow => _playerWindow ?? (_playerWindow = new PlayerWindow(Game));
			public static OpponentWindow OpponentWindow => _opponentWindow ?? (_opponentWindow = new OpponentWindow(Game));
			public static TimerWindow TimerWindow => _timerWindow ?? (_timerWindow = new TimerWindow(Config.Instance));
			public static StatsWindow StatsWindow => _statsWindow ?? (_statsWindow = new StatsWindow());
			public static StatsWindow_New NewStatsWindow => _newStatsWindow ?? (_newStatsWindow = new StatsWindow_New());
		}
	}
}