using System;
using System.IO;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Windows;

namespace Hearthstone_Deck_Tracker
{
    public static class Core
    {
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
            MainWindow = new MainWindow(Game);
            MainWindow.Show();
            splashScreenWindow.Close();
        }

        public static GameV2 Game { get; set; }
        public static MainWindow MainWindow { get; set; }
        private static OverlayWindow _overlay;
        public static OverlayWindow Overlay { get { return _overlay ?? (_overlay = new OverlayWindow(Game)); } }

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
