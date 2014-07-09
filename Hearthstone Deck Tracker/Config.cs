using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using MahApps.Metro;

namespace Hearthstone_Deck_Tracker
{
    public class Config
    {
        public readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\HearthstoneDeckTracker";
        private string _currentLogFile;

        public string HomeDir
        {
            get { return SaveInAppData ? AppDataPath + "/" : string.Empty; }
        }

        public string ConfigPath
        {
            get { return HomeDir + "config.xml"; }
        }

        public string LogFilePath
        {
            get { return _currentLogFile ?? GetLogFileName(); }
        }

        public bool SaveInAppData = true;
        public bool GenerateLog = false;
        public int TrackerWindowLeft = -1;
        public int TrackerWindowTop = -1;
        public string LastDeck = "";
        public bool HideInBackground = false;
        public bool HideInMenu = false;
        public bool HideEnemyCards = false;
        public bool HideEnemyCardCount = false;
        public bool HideOpponentDrawChances = false;
        public bool HidePlayerCardCount = false;
        public bool HideDrawChances = false;
        public bool HidePlayerCards = false;
        public bool HideOverlay = false;
        public bool HideDecksInOverlay = false;
        public int WindowHeight = 672;
        public string HearthstoneDirectory = "";
        public bool ShowInTaskbar = false;
        public bool HighlightCardsInHand = false;
        public int OffsetX = 0;
        public int OffsetY = 0;
        public int CustomWidth = -1;
        public int CustomHeight = -1;
        public bool VisibleOverlay = false;
        public int PlayerWindowHeight = 0;
        public double PlayerWindowTop = -32000;
        public double PlayerWindowLeft = -32000;
        public int OpponentWindowHeight = 0;
        public double OpponentWindowTop = -32000;
        public double OpponentWindowLeft = -32000;
        public bool WindowsTopmost = false;
        public bool WindowsTopmostIfHsForeground = false;
        public bool WindowsOnStartup = false;
        public bool TimerWindowOnStartup = false;
        public bool TimerWindowTopmost = false;
        public bool TimerWindowTopmostIfHsForeground = false;
        public string WindowsBackgroundHex = "#696969";
        public int UpdateDelay = 100;
        public double PlayerDeckTop = 17;
        public double PlayerDeckHeight = 65;
        public double OpponentDeckHeight = 65;
        public double OpponentDeckTop = 17;
        public double PlayerDeckLeft = 99.5;
        public double OpponentDeckLeft = 0.5;
        public double OverlayOpacity = 100;
        public double PlayerOpacity = 100;
        public double OpponentOpacity = 100;
        public double OverlayPlayerScaling = 100;
        public double OverlayOpponentScaling = 100;
        public bool UseSameScaling = true;
        public bool KeepDecksVisible = true;
        public bool MinimizeToTray = false;
        public double TimerLeft = 75;
        public bool ShowAllDecks = false;
        public bool HideTimers = false;
        public double TimersHorizontalPosition = 80;
        public double TimersVerticalPosition = 50;
        public double TimersHorizontalSpacing = 0;
        public double TimersVerticalSpacing = 50;
        public double TimerWindowTop = -32000;
        public double TimerWindowLeft = -32000;
        public double TimerWindowHeight = 130;
        public double TimerWindowWidth = 150;

        [XmlArray(ElementName = "SelectedTags")]
        [XmlArrayItem(ElementName = "Tag")]
        public List<string> SelectedTags = new List<string>();

        public int ClickDelay = 50;
        public int SearchDelay = 100;
        public double CardPosX = 0.15;
        public double Card2PosX = 0.25;
        public double CardPosY = 0.3;
        public double SearchBoxX = 0.5;
        public double SearchBoxY = 0.89;
        public double SearchBoxYFullscreen = 0.92;
        public double NameDeckX = 0.8;
        public double NameDeckY = 0.05;
        public bool ExportSetDeckName = true;

        public bool AutoDeckDetection = true;
        public bool AutoSelectDetectedDeck = true;
        [XmlIgnore]
        public bool Debug = false;
        
        public bool HideOpponentCardAge = false;
        public bool HideOpponentCardMarks = false;
        public string AccentName;
        public string ThemeName;
        public string SelectedWindowBackground = "Theme";
        public bool TextOnTopPlayer = false;
        public bool TextOnTopOpponent = false;

        public string SelectedLanguage = "enUS";

        public bool PrioritizeGolden = true;

        public string KeyPressOnGameStart = "None";
        public string KeyPressOnGameEnd = "None";

        public bool ManaCurveMyDecks = true;
        public bool ManaCurveNewDeck = true;

        public bool TrackerCardToolTips = true;
        public bool WindowCardToolTips = true;
        public bool OverlayCardToolTips = true;



        private string GetLogFileName()
        {
            var date = DateTime.Now;
            _currentLogFile =  string.Format("Logs/log_{0}{1}{2}-{3}{4}{5}.txt", date.Day, date.Month, date.Year, date.Hour,
                                 date.Minute, date.Second);
            return _currentLogFile;
        }
    }
}