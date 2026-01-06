#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using HearthMirror;
using Hearthstone_Deck_Tracker.Controls.Stats;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
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
using WPFLocalizeExtension.Engine;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using System.Linq;
using System.Net.Http;
using System.Windows.Interop;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Live;
using Hearthstone_Deck_Tracker.Stats;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public static class Core
	{
		private const int UpdateDelay = 16;
		private static TrayIcon? _trayIcon;
		private static OverlayWindow? _overlay;
		private static int _updateRequestsPlayer;
		private static int _updateRequestsOpponent;
		private static DateTime _startUpTime;
		private static bool _updateOverlay = true;
		private static readonly LogWatcherManager LogWatcherManger = new();

		// Should be global to application. Always use this one instead of
		// instantiating a new HttpClient.
		public static readonly HttpClient HttpClient = new(new HttpClientHandler
			{ AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate });

		internal static GameV2? _game;
		public static GameV2 Game => _game ??= new GameV2();
		private static MainWindow? _mainWindow;
		public static MainWindow MainWindow => _mainWindow ??= new MainWindow();


		public static bool Initialized { get; private set; }

		public static TrayIcon TrayIcon => _trayIcon ??= new TrayIcon();

		public static OverlayWindow Overlay => _overlay ??= new OverlayWindow(Game);

		public static bool IsShuttingDown { get; private set; }

		internal static event Action<bool>? GameIsRunningChanged;

#pragma warning disable 1998
		public static async void Initialize()
#pragma warning restore 1998
		{
			LocalizeDictionary.Instance.Culture = CultureInfo.GetCultureInfo("en-US");
			_startUpTime = DateTime.UtcNow;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
			ServicePointManager.DefaultConnectionLimit = 30;
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

			HearthDb.Config.AutoLoadCardDefs = false; // Needs to be set before accessing HearthDb.Cards

			if(!Config.IsLoaded)
			{
				// We may load the config earlier as part of squirrel init
				Config.Load();
			}

			var forceSoftwareRendering = !Config.Instance.UseHardwareAcceleration;
			if(forceSoftwareRendering == null)
			{
				// avoid using hardware acceleration if the render capability is low. This solves the most common OOM crashes
				// more info: https://stackoverflow.com/questions/7737372/wpf-crash-with-intel-hd-video-cards and https://stackoverflow.com/questions/4951058/software-rendering-mode-wpf/4951250#4951250
				var renderingTier = RenderCapability.Tier >> 16;
				// force Intel Arc gpus to use software rendering
				var isIntelGpu = Helper.IsIntelGpu();
				forceSoftwareRendering = renderingTier == 0 || isIntelGpu;

			}

			if (forceSoftwareRendering.Value)
			{
				RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
			}


			var splashScreenWindow = new SplashScreenWindow();
			splashScreenWindow.ShowConditional();

			ConfigManager.Run();
			if(Config.Instance.GoogleAnalytics)
				HSReplayNetClientAnalytics.Initialize();

			if(ConfigManager.PreviousVersion == null)
				Config.SetInitialLanguage();

			Log.Info($"HDT: {Helper.GetCurrentVersion()}, Operating System: {Helper.GetWindowsVersion()}, .NET Framework: {Helper.GetInstalledDotNetVersion()}");
#if(SQUIRREL)
			if(Config.Instance.CheckForUpdates)
			{
				var updateCheck = Updater.StartupUpdateCheck();
				while(!updateCheck.IsCompleted)
				{
					await Task.Delay(16);
					if(Updater.Status.SkipStartupCheck)
						break;
				}
			}
#endif
			Log.Initialize();
			Reflection.LogDebugMessage += msg => Log.Debug("HearthMirror RPC[client]: " + msg);
			Reflection.LogMessage += msg => Log.Info("HearthMirror RPC [client]: " + msg);

			Reflection.OnMemoryReading += (methodName, successCount, failureCount) =>
			{
				Influx.OnMemoryReading(methodName, successCount, failureCount);
			};

			Reflection.OnIpcServerExit += exitCode => {
				string mode = "NULL";
				if (_game?.CurrentMode != null)
				{
					mode = _game.CurrentMode.ToString();
				}
				Influx.OnHearthMirrorExit(exitCode, mode);
			};

			Reflection.StdErr += (sender, args) => {
				if(args.Data != null && args.Data.Trim() != "")
				{
					Log.Info("HearthMirror RPC [stderr]: " + args.Data);
				}
			};

			Reflection.StdOut += (sender, args) =>
			{
				if(args.Data != null && args.Data.Trim() != "")
				{
					Log.Info("HearthMirror RPC [stdout]: " + args.Data);
				}
			};

			Reflection.AccessDenied += OnAccessDenied;

			Reflection.Exception += e => Log.Warn("HearthMirror Exception: " + e);

			LocUtil.UpdateCultureInfo();
			Helper.UpdateCardLanguage();

			CardDefsManager.InitialDefsLoaded += () =>
			{
				// Defer starting log watchers until we have at least the initial card defs.
				// Especially if HDT is started in the middle of the game, there maybe edge cases
				// where reactions to log events unnecessarily fail due to cards being unknown.
				_ = LogWatcherManger.Start(Game);
			};
			CardDefsManager.CardsChanged += () =>
			{
				UpdatePlayerCards(true);
				UpdateOpponentCards(true);
			};
			// Load initial card defs
			CardDefsManager.EnsureLatestCardDefs();

			var newUser = ConfigManager.PreviousVersion == null;
			LogConfigUpdater.Run().Forget();
			LogConfigWatcher.Start();

			UITheme.InitializeTheme().Forget();
			ThemeManager.Run();
			ResourceMonitor.Run();
			Game.SecretsManager.OnSecretsChanged += cards => Overlay.ShowSecrets(cards);
			MainWindow.Show();
			splashScreenWindow.Close();

			if(Config.Instance.DisplayHsReplayNoteLive && ConfigManager.PreviousVersion != null && ConfigManager.PreviousVersion < new Version(1, 1, 0))
				MainWindow.FlyoutHsReplayNote.IsOpen = true;

			if(ConfigManager.UpdatedVersion != null)
			{
#if(!SQUIRREL)
				Updater.Cleanup();
#endif
				if(ConfigManager.ShouldShowUpdateNotes())
				{
					MainWindow.UpdateNotesControl.LoadReleaseNotes();
					MainWindow.FlyoutUpdateNotes.IsOpen = true;
					MainWindow.UpdateNotesControl.SetHighlight(ConfigManager.PreviousVersion);
				}

#if(SQUIRREL)
				// Once per update, update the remote. We do this in case e.g. GitHub was down and switched
				// every user to a different remote.
				SquirrelConnection.FindBestRemote();
#endif

#if(SQUIRREL && !DEV)
				if(Config.Instance.CheckForDevUpdates && !Config.Instance.AllowDevUpdates.HasValue)
					MainWindow.ShowDevUpdatesMessage();
#endif
			}
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

			PluginManager.Instance.LoadPluginsFromDefaultPath();
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
				ShowRestartRequiredMessageAsync().Forget();
				Overlay.ShowRestartRequiredWarning();
			}

			Remote.Config.Load();
			HotKeyManager.Load();

			if(Helper.HearthstoneDirExists && Config.Instance.StartHearthstoneWithHDT && !Game.IsRunning)
				HearthstoneRunner.StartHearthstone().Forget();

			HSReplayNetHelper.UpdateAccount().Forget();

			if(Config.Instance.BattlegroundsSessionRecapWindowOnStart)
				Windows.BattlegroundsSessionWindow.Show();

			Helper.EnsureClientLogConfig();

			Initialized = true;

			Influx.OnAppStart(
				Helper.GetCurrentVersion(),
				newUser,
				HSReplayNetOAuth.IsFullyAuthenticated,
				HSReplayNetOAuth.AccountData?.IsPremium ?? false,
				(int)(DateTime.UtcNow - _startUpTime).TotalSeconds,
				PluginManager.Instance.Plugins.Count,
				Config.Instance.CleanShutdown,
				Updater.Status.SkipStartupCheck,
				forceSoftwareRendering.Value
			);

			Config.Instance.CleanShutdown = false;
			Config.Save();
		}

		private static bool _notifyAccessDenied = true;
		private static async void OnAccessDenied()
		{
			if(!_notifyAccessDenied)
				return;
			_notifyAccessDenied = false;
			Log.Warn("Hearthstone is probably running with elevated permissions. Stopping all watchers.");
			LogWatcherManger.IsEnabled = false;
			await LogWatcherManger.Stop(true);
			Influx.OnUnevenPermissions();
			_ = MainWindow.ShowMessage(LocUtil.Get("MessageDialogs_Permissions_Title"), LocUtil.Get("MessageDialogs_Permissions_Description"));
			MainWindow.ActivateWindow();
		}

		internal static async Task Shutdown()
		{
			await PrepareShutdown();
			Application.Current.Shutdown();
		}

		private static async Task PrepareShutdown()
		{
			try
			{
				Log.Info("Shutting down...");
				Influx.OnAppExit(Helper.GetCurrentVersion(), _mainWindow?.IsStatsOverviewInitialized ?? false);
				LiveDataManager.Stop();
				_updateOverlay = false;
				var logWatcher = LogWatcherManger.Stop(true);

				//wait for update to finish, might otherwise crash when overlay gets disposed
				for(var i = 0; i < 100; i++)
				{
					if(IsShuttingDown)
						break;
					await Task.Delay(50);
				}

				await logWatcher;

				Windows.CloseAll();
				Overlay.Close();
				MainWindow.Close();

				TrayIcon.NotifyIcon.Visible = false;

				Config.Instance.CleanShutdown = true;
				Config.Save();
				DeckList.Save();
				DeckStatsList.Save();
				PluginManager.SavePluginsSettings();
				PluginManager.Instance.UnloadPlugins();
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
		}

		private static bool _restarting;
		internal static async void RestartApplication()
		{
			if(_restarting)
				return;
			_restarting = true;
			Log.Info("Restarting...");

#if(SQUIRREL)
			await PrepareShutdown();
			Squirrel.UpdateManager.RestartApp();
#else
			// This event is fired by Application.Shutdown
			Application.Current.Exit += (_, _) =>
			{
				Process.Start(Application.ResourceAssembly.Location);
			};
			await Shutdown();
#endif
		}

		private static async Task ShowRestartRequiredMessageAsync()
		{
			MainWindow.ActivateWindow();
			while(MainWindow.Visibility != Visibility.Visible || MainWindow.WindowState == WindowState.Minimized)
				await Task.Delay(100);
			await MainWindow.ShowMessageAsync("Hearthstone restart required", "The log.config file has been updated. HDT may not work properly until Hearthstone has been restarted.");
		}

		private static async void UpdateOverlayAsync()
		{
#if(!SQUIRREL)
			if(Config.Instance.CheckForUpdates)
				Updater.CheckForUpdates(true);
#endif
			var hsForegroundChanged = false;
			while(_updateOverlay)
			{
				if(Config.Instance.CheckForUpdates)
					Updater.CheckForUpdates();
				if(User32.GetHearthstoneWindow() != IntPtr.Zero)
				{
					if(Game.CurrentRegion == Region.UNKNOWN)
					{
						//game started
						Helper.VerifyHearthstonePath();

						Helper.UpdateCardLanguage();

						AssetDownloaders.cardImageDownloader?.InvalidateCachedAssets();
						AssetDownloaders.cardTileDownloader?.InvalidateCachedAssets();
						AssetDownloaders.cardPortraitDownloader?.InvalidateCachedAssets();
						AssetDownloaders.heroImageDownloader?.InvalidateCachedAssets();

						var ok = Helper.EnsureClientLogConfig();
						if(!ok)
						{
							ShowRestartRequiredMessageAsync().Forget();
							Overlay.ShowRestartRequiredWarning();
						}
						Game.CurrentRegion = await Helper.GetCurrentRegion();
						if(Game.CurrentRegion != Region.UNKNOWN)
						{
							BackupManager.Run();
							Game.MetaData.HearthstoneBuild = null;
						}
						//Watchers.ExperienceWatcher.Run();
						Watchers.SceneWatcher.Run();
						Watchers.UiWatcher.Run();

						Remote.Config.Load();
						Remote.Mercenaries.Load();
						Remote.LiveSecrets.Load();

						Reflection.StartIpcClient();

						CardDefsManager.EnsureLatestCardDefs();
					}
					Overlay.UpdateVisibility(); // Always run, this handles the game being in background, etc

					if(!Game.IsRunning)
					{
						Overlay.HookGameWindow();
						Overlay.UpdatePosition(); // Only needs to be called once, HookGameWindow will trigger subsequent position updates.
						Overlay.Update(true);
						Windows.CapturableOverlay?.UpdateContentVisibility();
					}

					if(Overlay.IsVisible)
						Overlay.UpdateBattlegroundsOverlay();

					TrayIcon.MenuItemStartHearthstone.Visible = false;

					Game.IsRunning = true;
					GameIsRunningChanged?.Invoke(true);

					Helper.GameWindowState = User32.GetHearthstoneWindowState();
					Windows.CapturableOverlay?.Update();
					if(User32.IsHearthstoneInForeground() && Helper.GameWindowState != WindowState.Minimized)
					{
						if(hsForegroundChanged)
						{
							Overlay.OnHearthstoneFocused();
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
					GameIsRunningChanged?.Invoke(false);
					Overlay.UnhookGameWindow();
					Overlay.ShowOverlay(false);
					Overlay.UpdateVisibilities();
					Watchers.Stop();
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
					LogWatcherManger.IsEnabled = true;
					_notifyAccessDenied = true;
					await Reset();
					Game.IsInMenu = true;
					Game.InvalidateMatchInfoCache();
					Overlay.HideRestartRequiredWarning();
					Helper.ClearCachedHearthstoneBuild();
					TurnTimer.Instance.Stop();
					Tier7Trial.Clear();
					Overlay.Tier7PreLobbyViewModel.Reset();
					Overlay.ArenaPreDraftViewModel.Reset();
					Overlay.BattlegroundsHeroPickingViewModel.Reset();
					Overlay.BattlegroundsQuestPickingViewModel.Reset();
					Overlay.BattlegroundsTrinketPickingViewModel.Reset();
					Overlay.HideBattlegroundsHeroPanel();
					Overlay.HideBattlegroundsTimewarpPanel();
					Overlay.ConstructedMulliganGuideViewModel.Reset();
					Overlay.ConstructedMulliganGuidePreLobbyViewModel.Reset();
					Overlay.BattlegroundsCompsGuidesVM.Reset();
					Overlay.ChinaModuleVM.Reset();
					Overlay.HideMulliganToast(false);

					TrayIcon.MenuItemStartHearthstone.Visible = true;

					Reflection.StopIpcClient();

					if(Config.Instance.CloseWithHearthstone)
						MainWindow.Close();
				}

				await Task.Delay(UpdateDelay);
			}
			IsShuttingDown = true;
		}

		private static bool _resetting;
		public static async Task Reset(bool updateUI = true)
		{
			if(_resetting)
			{
				Log.Warn("Reset already in progress.");
				return;
			}
			_resetting = true;
			var stoppedReader = await LogWatcherManger.Stop();
			Game.Reset(updateUI: updateUI);
			await Task.Delay(1000);
			if(stoppedReader)
				LogWatcherManger.Start(Game).Forget();
			if(updateUI)
			{
				Overlay.HideSecrets();
				Overlay.Update(false);
				UpdatePlayerCards(true);
			}
			_resetting = false;
		}

		internal static async void UpdatePlayerCards(bool reset = false)
		{
			_updateRequestsPlayer++;
			await Task.Delay(100);
			_updateRequestsPlayer--;
			if(_updateRequestsPlayer > 0)
				return;

			if(Game.GameTime.Time == DateTime.MinValue && !reset)
				return;

			var dredged = Game.Player.Deck.Where(x => x.Info.DeckIndex != 0).OrderByDescending(x => x.Info.DeckIndex);
			Card toCard (Entity entity)
			{
				var card = (Card)entity.Card.Clone();
				card.ControllerPlayer = Game.Player;
				card.DeckListIndex = entity.Info.DeckIndex;
				return card;
			}
			var top = dredged.Where(x => x.Info.DeckIndex > 0).Select(toCard).ToList();
			var bottom = dredged.Where(x => x.Info.DeckIndex < 0).Select(toCard).ToList();
			Overlay.UpdatePlayerCards(new List<Card>(Game.Player.PlayerCardList), reset, top, bottom, new List<Sideboard>(Game.Player.PlayerSideboardsDict));
			if(Windows.PlayerWindow.IsVisible)
				await Windows.PlayerWindow.UpdatePlayerCards(new List<Card>(Game.Player.PlayerCardList), reset, top, bottom, new List<Sideboard>(Game.Player.PlayerSideboardsDict));
		}

		internal static async void UpdateOpponentCards(bool reset = false)
		{
			_updateRequestsOpponent++;
			await Task.Delay(100);
			_updateRequestsOpponent--;
			if(_updateRequestsOpponent > 0)
				return;

			if(Game.GameTime.Time == DateTime.MinValue && !reset)
				return;

			var cardWithRelatedCards = Game.RelatedCardsManager.GetCardsOpponentMayHave(Game.Opponent, Game.CurrentGameType, Game.CurrentFormatType).ToList();
			if(Game.IsArenaMatch)
			{
				var arenaPackages = Game.ArenaPackagesManager.GetOpponentsPackageCards(Game.Opponent.OpponentCardList);
				Overlay.UpdateOpponentCards(new List<Card>(Game.Opponent.OpponentCardList), cardWithRelatedCards, arenaPackages, reset);
			}
			else
			{
				Overlay.UpdateOpponentCards(new List<Card>(Game.Opponent.OpponentCardList), cardWithRelatedCards, reset);
			}

			if(Windows.OpponentWindow.IsVisible)
				Windows.OpponentWindow.UpdateOpponentCards(new List<Card>(Game.Opponent.OpponentCardList), reset);
		}

		internal static void ResetPlayerResourcesWidgets()
		{
			Overlay.ResetPlayerResourcesWidgets(Game.Player.MaxHealth, Game.Player.MaxMana, Game.Player.MaxHandSize);
		}

		internal static void UpdatePlayerResourcesWidget()
		{
			Overlay.UpdatePlayerResourcesWidget(Game.Player.MaxHealth, Game.Player.MaxMana, Game.Player.MaxHandSize);
		}

		internal static void UpdateOpponentResourcesWidget()
		{
			var shouldShowCorpsesLeft = Game.Opponent.HasDeathKnightTourist;
			Overlay.UpdateOpponentResourcesWidget(Game.Opponent.MaxHealth, Game.Opponent.MaxMana, Game.Opponent.MaxHandSize, shouldShowCorpsesLeft ? Game.Opponent.CorpsesLeft : null);
		}

		public static async void HandleRewind(DateTime playTime, DateTime rewindTime)
		{
			await LogWatcherManger.HandleRewind(playTime, rewindTime);
		}

		public static void HandleGameEnd()
		{
			LogWatcherManger.IgnoredTimeRanges.Clear();
		}

		public static class Windows
		{
			private static PlayerWindow? _playerWindow;
			private static OpponentWindow? _opponentWindow;
			private static BattlegroundsSessionWindow? _bgsSessionWindow;
			private static TimerWindow? _timerWindow;

			public static PlayerWindow PlayerWindow => _playerWindow ??= new PlayerWindow(Game);
			public static OpponentWindow OpponentWindow => _opponentWindow ??= new OpponentWindow(Game);
			public static BattlegroundsSessionWindow BattlegroundsSessionWindow => _bgsSessionWindow ??= new BattlegroundsSessionWindow();
			public static TimerWindow TimerWindow => _timerWindow ??= new TimerWindow(Config.Instance);
			public static CapturableOverlayWindow? CapturableOverlay;

			internal static void CloseAll()
			{
				_timerWindow?.Close();
				_playerWindow?.Close();
				_opponentWindow?.Close();
				_bgsSessionWindow?.Close();
				CapturableOverlay?.Close();
			}
		}
	}
}
