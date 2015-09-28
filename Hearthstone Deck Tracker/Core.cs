using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Windows;
using Application = System.Windows.Application;

namespace Hearthstone_Deck_Tracker
{
    public static class Core
    {
        private static MainWindow _mainWindow;
        private static TrayIcon _trayIcon;
        private static OverlayWindow _overlay;

        public static Version Version { get; set; }
        public static GameV2 Game { get; set; }

        public static MainWindow MainWindow
        {
            get { return _mainWindow ?? (_mainWindow = new MainWindow()); }
        }

        public static TrayIcon TrayIcon
        {
            get { return _trayIcon ?? (_trayIcon = new TrayIcon()); }
        }

        public static OverlayWindow Overlay
        {
            get { return _overlay ?? (_overlay = new OverlayWindow(Game)); }
        }

        public static void Initialize()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Config.Load();
            Logger.Initialzie();
            var splashScreenWindow = new SplashScreenWindow();
            splashScreenWindow.Show();
            Game = new GameV2();
            API.Core.Game = Game;
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
            MainWindow.Show();
            splashScreenWindow.Close();
            MainWindow.LoadMainWindow();

            if (Config.Instance.PlayerWindowOnStart)
                Windows.PlayerWindow.Show();
            if (Config.Instance.OpponentWindowOnStart)
                Windows.OpponentWindow.Show();
            if (Config.Instance.TimerWindowOnStartup)
                Windows.TimerWindow.Show();
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

    public class TrayIcon
    {
        private NotifyIcon _notifyIcon;

        public NotifyIcon NotifyIcon
        {
            get
            {
                if (_notifyIcon == null) Initialize();
                return _notifyIcon;
            }
        }

        public void Initialize()
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon(@"Images/HearthstoneDeckTracker16.ico"),
                Visible = true,
                ContextMenu = new ContextMenu(),
                Text =
                    "Hearthstone Deck Tracker v" + (Helper.GetCurrentVersion() ?? new Version("0.0")).ToVersionString()
            };

            var startHearthstonMenuItem = new MenuItem("Start Launcher/Hearthstone",
                (sender, args) => Helper.StartHearthstoneAsync());
            startHearthstonMenuItem.Name = "startHearthstone";
            _notifyIcon.ContextMenu.MenuItems.Add(startHearthstonMenuItem);

            var useNoDeckMenuItem = new MenuItem("Use no deck", (sender, args) => UseNoDeckContextMenu());
            useNoDeckMenuItem.Name = "useNoDeck";
            _notifyIcon.ContextMenu.MenuItems.Add(useNoDeckMenuItem);

            var autoSelectDeckMenuItem = new MenuItem("Autoselect deck",
                (sender, args) => AutoDeckDetectionContextMenu());
            autoSelectDeckMenuItem.Name = "autoSelectDeck";
            _notifyIcon.ContextMenu.MenuItems.Add(autoSelectDeckMenuItem);

            var classCardsFirstMenuItem = new MenuItem("Class cards first",
                (sender, args) => SortClassCardsFirstContextMenu());
            classCardsFirstMenuItem.Name = "classCardsFirst";
            _notifyIcon.ContextMenu.MenuItems.Add(classCardsFirstMenuItem);

            _notifyIcon.ContextMenu.MenuItems.Add("Show", (sender, args) => Core.MainWindow.ActivateWindow());
            _notifyIcon.ContextMenu.MenuItems.Add("Exit", (sender, args) => Core.MainWindow.Close());
            _notifyIcon.MouseClick += (sender, args) =>
            {
                if (args.Button == MouseButtons.Left)
                    Core.MainWindow.ActivateWindow();
            };
        }


        private void AutoDeckDetectionContextMenu()
        {
            var enable = (bool) GetContextMenuProperty("autoSelectDeck", "Checked");
            Core.MainWindow.AutoDeckDetection(!enable);
        }

        private void UseNoDeckContextMenu()
        {
            var enable = (bool) GetContextMenuProperty("useNoDeck", "Checked");
            if (enable)
                Core.MainWindow.SelectLastUsedDeck();
            else
                Core.MainWindow.SelectDeck(null, true);
        }

        private int IndexOfKeyContextMenuItem(string key)
        {
            return NotifyIcon.ContextMenu.MenuItems.IndexOfKey(key);
        }

        public void SetContextMenuProperty(string key, string property, object value)
        {
            var menuItemInd = IndexOfKeyContextMenuItem(key);
            object target = NotifyIcon.ContextMenu.MenuItems[menuItemInd];
            target.GetType().GetProperty(property).SetValue(target, value);
        }

        private object GetContextMenuProperty(string key, string property)
        {
            var menuItemInd = IndexOfKeyContextMenuItem(key);
            object target = NotifyIcon.ContextMenu.MenuItems[menuItemInd];
            return target.GetType().GetProperty(property).GetValue(target, null);
        }

        private void SortClassCardsFirstContextMenu()
        {
            var enable = (bool) GetContextMenuProperty("classCardsFirst", "Checked");
            Core.MainWindow.SortClassCardsFirst(!enable);
        }
    }
}