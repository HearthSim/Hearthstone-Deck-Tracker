using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;
using Application = System.Windows.Application;
using Region = Hearthstone_Deck_Tracker.Enums.Region;

namespace Hearthstone_Deck_Tracker
{
    public static class Core
    {
        private static TrayIcon _trayIcon;
        private static OverlayWindow _overlay;
        public static Version Version { get; set; }
        public static GameV2 Game { get; set; }
        public static MainWindow MainWindow { get; set; }
        public static bool Initialized { get; private set; }

        public static TrayIcon TrayIcon
        {
            get { return _trayIcon ?? (_trayIcon = new TrayIcon()); }
        }

        public static OverlayWindow Overlay
        {
            get { return _overlay ?? (_overlay = new OverlayWindow(Game)); }
        }

        internal static bool UpdateOverlay { get; set; }
        internal static bool Update { get; set; }
        internal static bool CanShutdown { get; set; }

        public static void Initialize()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Config.Load();
            ConfigManager.Run();
            Logger.Initialzie();
            Helper.UpdateAppTheme();
            var splashScreenWindow = new SplashScreenWindow();
            splashScreenWindow.Show();
            Game = new GameV2();
            if (!HearthStatsAPI.LoadCredentials() && Config.Instance.ShowLoginDialog)
            {
                var loginWindow = new LoginWindow();
                splashScreenWindow.Close();
                loginWindow.ShowDialog();
                if (!loginWindow.LoginResult)
                {
                    Application.Current.Shutdown();
                    return;
                }
                splashScreenWindow = new SplashScreenWindow();
                splashScreenWindow.Show();
            }
            MainWindow = new MainWindow();
            MainWindow.LoadConfigSettings();
            MainWindow.Show();
            splashScreenWindow.Close();

            if (ConfigManager.UpdatedVersion != null)
            {
                Updater.Cleanup();
                MainWindow.FlyoutUpdateNotes.IsOpen = true;
                MainWindow.UpdateNotesControl.LoadUpdateNotes();
            }
            NetDeck.CheckForChromeExtention();
            DataIssueResolver.Run();

            if (Helper.HearthstoneDirExists)
            {
                if (Helper.UpdateLogConfig && Game.IsRunning)
                {
                    MainWindow.ShowMessageAsync("Restart Hearthstone",
                        "This is either your first time starting HDT or the log.config file has been updated. Please restart Hearthstone, for HDT to work properly.");
                }
                LogReaderManager.Start(Game);
            }
            else
                MainWindow.ShowHsNotInstalledMessage();

            Helper.CopyReplayFiles();
            BackupManager.Run();

            if (Config.Instance.PlayerWindowOnStart)
                Windows.PlayerWindow.Show();
            if (Config.Instance.OpponentWindowOnStart)
                Windows.OpponentWindow.Show();
            if (Config.Instance.TimerWindowOnStartup)
                Windows.TimerWindow.Show();

            if (Config.Instance.HearthStatsSyncOnStart && HearthStatsAPI.IsLoggedIn)
                HearthStatsManager.SyncAsync(background: true);

            PluginManager.Instance.LoadPlugins();
            MainWindow.Options.OptionsTrackerPlugins.Load();
            PluginManager.Instance.StartUpdateAsync();

            UpdateOverlayAsync();
            NewsUpdater.UpdateAsync();
            Initialized = true;
        }

        private static async void UpdateOverlayAsync()
        {
            if (Config.Instance.CheckForUpdates)
                Updater.CheckForUpdates(true);
            var hsForegroundChanged = false;
            var useNoDeckMenuItem = TrayIcon.NotifyIcon.ContextMenu.MenuItems.IndexOfKey("startHearthstone");
            UpdateOverlay = Helper.HearthstoneDirExists;
            while (UpdateOverlay)
            {
                if (User32.GetHearthstoneWindow() != IntPtr.Zero)
                {
                    if (Game.CurrentRegion == Region.UNKNOWN)
                    {
                        //game started
                        Game.CurrentRegion = Helper.GetCurrentRegion();
                    }
                    Overlay.UpdatePosition();

                    if (Config.Instance.CheckForUpdates)
                        Updater.CheckForUpdates();

                    if (!Game.IsRunning)
                        Overlay.Update(true);

                    MainWindow.BtnStartHearthstone.Visibility = Visibility.Collapsed;
                    TrayIcon.NotifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Visible = false;

                    Game.IsRunning = true;
                    if (User32.IsHearthstoneInForeground())
                    {
                        if (hsForegroundChanged)
                        {
                            Overlay.Update(true);
                            if (Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
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
                    else if (!hsForegroundChanged)
                    {
                        if (Config.Instance.WindowsTopmostIfHsForeground && Config.Instance.WindowsTopmost)
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
                    if (Game.IsRunning)
                    {
                        //game was closed
                        Logger.WriteLine("Exited game", "UpdateOverlayLoop");
                        Game.CurrentRegion = Region.UNKNOWN;
                        Logger.WriteLine("Reset region", "UpdateOverlayLoop");
                        //HsLogReaderV2.Instance.ClearLog();
                        Game.Reset();
                        if (DeckList.Instance.ActiveDeck != null)
                            Game.SetPremadeDeck((Deck) DeckList.Instance.ActiveDeck.Clone());
                        await LogReaderManager.Restart();

                        MainWindow.BtnStartHearthstone.Visibility = Visibility.Visible;
                        TrayIcon.NotifyIcon.ContextMenu.MenuItems[useNoDeckMenuItem].Visible = true;

                        if (Config.Instance.CloseWithHearthstone)
                            MainWindow.Close();
                    }
                    Game.IsRunning = false;
                }

                if (Config.Instance.NetDeckClipboardCheck.HasValue && Config.Instance.NetDeckClipboardCheck.Value
                    && Initialized
                    && !User32.IsHearthstoneInForeground())
                    NetDeck.CheckForClipboardImport();

                await Task.Delay(Config.Instance.UpdateDelay);
            }
            CanShutdown = true;
        }


        public static class Windows
        {
            private static PlayerWindow _playerWindow;
            private static OpponentWindow _opponentWindow;
            private static TimerWindow _timerWindow;
            private static StatsWindow _statsWindow;

            public static PlayerWindow PlayerWindow
            {
                get { return _playerWindow ?? (_playerWindow = new PlayerWindow(Game)); }
            }

            public static OpponentWindow OpponentWindow
            {
                get { return _opponentWindow ?? (_opponentWindow = new OpponentWindow(Game)); }
            }

            public static TimerWindow TimerWindow
            {
                get { return _timerWindow ?? (_timerWindow = new TimerWindow(Config.Instance)); }
            }

            public static StatsWindow StatsWindow
            {
                get { return _statsWindow ?? (_statsWindow = new StatsWindow()); }
            }
        }
    }
}