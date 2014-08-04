﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace Hearthstone_Deck_Tracker
{
	public class Config
	{
		private static Config _config = new Config();

		public readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
		                                     @"\HearthstoneDeckTracker";

		public string AccentName = Defaults.AccentName;
		public bool AdditionalOverlayTooltips = Defaults.AdditionalOverlayTooltips;
		public bool AlwaysOverwriteLogConfig = Defaults.AlwaysOverwriteLogConfig;
		public bool AutoDeckDetection = Defaults.AutoDeckDetection;
		public bool AutoSelectDetectedDeck = Defaults.AutoSelectDetectedDeck;
		public bool BringHsToForeground = Defaults.BringHsToForeground;
		public double Card2PosX = Defaults.Card2PosX;
		public double CardPosX = Defaults.CardPosX;
		public double CardPosY = Defaults.CardPosY;
		public bool CardSortingClassFirst = Defaults.CardSortingClassFirst;
		public bool CheckForUpdates = Defaults.CheckForUpdates;
		public bool ClearLogFileAfterGame = Defaults.ClearLogFileAfterGame;
		public int ClickDelay = Defaults.ClickDelay;
		public string CreatedByVersion = Defaults.CreatedByVersion;
		public int CustomHeight = Defaults.CustomHeight;
		public int CustomWidth = Defaults.CustomWidth;
		[XmlIgnore] public bool Debug = Defaults.Debug;
		public bool ExportSetDeckName = Defaults.ExportSetDeckName;
		public bool ExtraFeatures = Defaults.ExtraFeatures;
		public bool FlashHsOnTurnStart = Defaults.FlashHsOnTurnStart;
		public bool GenerateLog = Defaults.GenerateLog;
		public string HearthstoneDirectory = Defaults.HearthstoneDirectory;
		public bool HideDecksInOverlay = Defaults.HideDecksInOverlay;
		public bool HideDrawChances = Defaults.HideDrawChances;
		public bool HideInBackground = Defaults.HideInBackground;
		public bool HideInMenu = Defaults.HideInMenu;
		public bool HideOpponentCardAge = Defaults.HideOpponentCardAge;
		public bool HideOpponentCardCount = Defaults.HideOpponentCardCount;
		public bool HideOpponentCardMarks = Defaults.HideOpponentCardMarks;
		public bool HideOpponentCards = Defaults.HideOpponentCards;
		public bool HideOpponentDrawChances = Defaults.HideOpponentDrawChances;
		public bool HideOverlay = Defaults.HideOverlay;
		public bool HidePlayerCardCount = Defaults.HidePlayerCardCount;
		public bool HidePlayerCards = Defaults.HidePlayerCards;
		public bool HideSecrets = Defaults.HideSecrets;
		public bool HideTimers = Defaults.HideTimers;
		public bool HighlightCardsInHand = Defaults.HighlightCardsInHand;
		public bool HighlightDiscarded = Defaults.HighlightDiscarded;
		public bool HighlightLastDrawn = Defaults.HighlightLastDrawn;
		public bool KeepDecksVisible = Defaults.KeepDecksVisible;
		public string KeyPressOnGameEnd = Defaults.KeyPressOnGameEnd;
		public string KeyPressOnGameStart = Defaults.KeyPressOnGameStart;
		public string LastDeck = Defaults.LastDeck;
		public bool ManaCurveMyDecks = Defaults.ManaCurveMyDecks;
		public bool ManaCurveNewDeck = Defaults.ManaCurveNewDeck;
		public bool MinimizeToTray = Defaults.MinimizeToTray;
		public double NameDeckX = Defaults.NameDeckX;
		public double NameDeckY = Defaults.NameDeckY;
		public int OffsetX = Defaults.OffsetX;
		public int OffsetY = Defaults.OffsetY;
		public double OpponentDeckHeight = Defaults.OpponentDeckHeight;
		public double OpponentDeckLeft = Defaults.OpponentDeckLeft;
		public double OpponentDeckTop = Defaults.OpponentDeckTop;
		public double OpponentOpacity = Defaults.OpponentOpacity;
		public int OpponentWindowHeight = Defaults.OpponentWindowHeight;
		public int? OpponentWndLeft = Defaults.OpponentWndLeft;
		public int? OpponentWndTop = Defaults.OpponentWndTop;
		public bool OverlayCardToolTips = Defaults.OverlayCardToolTips;
		public double OverlayOpacity = Defaults.OverlayOpacity;
		public double OverlayOpponentScaling = Defaults.OverlayOpponentScaling;
		public double OverlayPlayerScaling = Defaults.OverlayPlayerScaling;
		public double PlayerDeckHeight = Defaults.PlayerDeckHeight;
		public double PlayerDeckLeft = Defaults.PlayerDeckLeft;
		public double PlayerDeckTop = Defaults.PlayerDeckTop;
		public double PlayerOpacity = Defaults.PlayerOpacity;
		public int PlayerWindowHeight = Defaults.PlayerWindowHeight;
		public int? PlayerWndLeft = Defaults.PlayerWndLeft;
		public int? PlayerWndTop = Defaults.PlayerWndTop;
		public bool PrioritizeGolden = Defaults.PrioritizeGolden;
		public bool RemoveCardsFromDeck = Defaults.RemoveCardsFromDeck;
		public bool SaveInAppData = Defaults.SaveInAppData;
		public bool SavePlayedGames = Defaults.SavePlayedGames;
		public string SavePlayedGamesName = Defaults.SavePlayedGamesName;
		public string SavePlayedGamesPath = Defaults.SavePlayedGamesPath;
		public double SearchBoxPosY = Defaults.SearchBoxPosY;
		public double SearchBoxX = Defaults.SearchBoxX;
		public int SearchDelay = Defaults.SearchDelay;
		public double SecretsLeft = Defaults.SecretsLeft;
		public double SecretsTop = Defaults.SecretsTop;
		public string SelectedDeckSorting = Defaults.SelectedDeckSorting;
		public string SelectedLanguage = Defaults.SelectedLanguage;

		[XmlArray(ElementName = "SelectedTags")] [XmlArrayItem(ElementName = "Tag")] public List<string> SelectedTags =
			Defaults.SelectedTags;

		public string SelectedWindowBackground = Defaults.SelectedWindowBackground;
		public bool ShowAllDecks = Defaults.ShowAllDecks;
		public bool ShowInTaskbar = Defaults.ShowInTaskbar;
		public bool ShowPlayerGet = Defaults.ShowPlayerGet;
		public bool StartMinimized = Defaults.StartMinimized;
		public Operation TagOperation = Defaults.TagOperation;
		public bool TextOnTopOpponent = Defaults.TextOnTopOpponent;
		public bool TextOnTopPlayer = Defaults.TextOnTopPlayer;
		public string ThemeName = Defaults.ThemeName;
		public double TimerLeft = Defaults.TimerLeft;
		public int TimerWindowHeight = Defaults.TimerWindowHeight;
		public int? TimerWndLeft = Defaults.TimerWndLeft;
		public bool TimerWindowOnStartup = Defaults.TimerWindowOnStartup;
		public int? TimerWndTop = Defaults.TimerWndTop;
		public bool TimerWindowTopmost = Defaults.TimerWindowTopmost;
		public bool TimerWindowTopmostIfHsForeground = Defaults.TimerWindowTopmostIfHsForeground;
		public int TimerWindowWidth = Defaults.TimerWindowWidth;
		public double TimersHorizontalPosition = Defaults.TimersHorizontalPosition;
		public double TimersHorizontalSpacing = Defaults.TimersHorizontalSpacing;
		public double TimersVerticalPosition = Defaults.TimersVerticalPosition;
		public double TimersVerticalSpacing = Defaults.TimersVerticalSpacing;
		public bool TrackerCardToolTips = Defaults.TrackerCardToolTips;
		public int? MainWndLeft = Defaults.MainWndLeft;
		public int? MainWndTop = Defaults.MainWndTop;
		public int UpdateDelay = Defaults.UpdateDelay;
		public bool UseSameScaling = Defaults.UseSameScaling;
		public bool VisibleOverlay = Defaults.VisibleOverlay;
		public bool WindowCardToolTips = Defaults.WindowCardToolTips;
		public int WindowHeight = Defaults.WindowHeight;
		public string WindowsBackgroundHex = Defaults.WindowsBackgroundHex;
		public bool WindowsOnStartup = Defaults.WindowsOnStartup;
		public bool WindowsTopmost = Defaults.WindowsTopmost;
		public bool WindowsTopmostIfHsForeground = Defaults.WindowsTopmostIfHsForeground;
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

		public static Config Instance
		{
			get { return _config; }
		}
		
		private string GetLogFileName()
		{
			var date = DateTime.Now;
			_currentLogFile = string.Format("Logs/log_{0}{1}{2}-{3}{4}{5}.txt", date.Day, date.Month, date.Year,
			                                date.Hour,
			                                date.Minute, date.Second);
			return _currentLogFile;
		}

		public static void Save()
		{
			XmlManager<Config>.Save(Instance.ConfigPath, Instance);
		}

		public static void SaveBackup(bool deleteOriginal = false)
		{
			var configPath = Instance.ConfigPath;

			if (File.Exists(configPath))
			{
				File.Copy(configPath, configPath + DateTime.Now.ToFileTime());

				if (deleteOriginal)
					File.Delete(configPath);
			}
		}

		public static string Load()
		{
			var foundConfig = false;
			try
			{
				if (File.Exists("config.xml"))
				{
					_config = XmlManager<Config>.Load("config.xml");
					foundConfig = true;
				}
				else if (File.Exists(Instance.AppDataPath + @"\config.xml"))
				{
					_config = XmlManager<Config>.Load(Instance.AppDataPath + @"\config.xml");
					foundConfig = true;
				}
				else if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
				{
					//save locally if appdata doesn't exist (when e.g. not on C)
					Instance.SaveInAppData = false;
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(
					e.Message + "\n\n" + e.InnerException +
					"\n\n If you don't know how to fix this, please delete " + Instance.ConfigPath,
					"Error loading config.xml");
				Application.Current.Shutdown();
			}

			var configPath = Instance.ConfigPath;

			if (!foundConfig)
			{
				if (Instance.HomeDir != string.Empty)
					Directory.CreateDirectory(Instance.HomeDir);
				using (var sr = new StreamWriter(Instance.ConfigPath, false))
				{
					sr.WriteLine("<Config></Config>");
				}
			}
			else if (Instance.SaveInAppData) //check if config needs to be moved
			{
				if (File.Exists("config.xml"))
				{
					Directory.CreateDirectory(Instance.HomeDir);
					SaveBackup(true); //backup in case the file already exists
					File.Move("config.xml", Instance.ConfigPath);
					Logger.WriteLine("Moved config to appdata");
				}
			}
			else
			{
				if (File.Exists(Instance.AppDataPath + @"\config.xml"))
				{
					SaveBackup(true); //backup in case the file already exists
					File.Move(Instance.AppDataPath + @"\config.xml", Instance.ConfigPath);
					Logger.WriteLine("Moved config to local");
				}
			}

			return configPath;
		}

		public class Defaults
		{
			public static readonly string CreatedByVersion;
			public static readonly string AccentName;
			public static readonly bool AdditionalOverlayTooltips = true;
			public static readonly bool AlwaysOverwriteLogConfig = true;
			public static readonly bool AutoDeckDetection = true;
			public static readonly bool AutoSelectDetectedDeck = true;
			public static readonly bool BringHsToForeground = false;
			public static readonly double Card2PosX = 0.25;
			public static readonly double CardPosX = 0.15;
			public static readonly double CardPosY = 0.3;
			public static readonly bool CardSortingClassFirst = false;
			public static readonly bool CheckForUpdates = true;
			public static readonly bool ClearLogFileAfterGame = true;
			public static readonly int ClickDelay = 50;
			public static readonly int CustomHeight = -1;
			public static readonly int CustomWidth = -1;
			public static readonly bool Debug = false;
			public static readonly bool ExtraFeatures = false;
			public static readonly bool ExportSetDeckName = true;
			public static readonly bool FlashHsOnTurnStart = true;
			public static readonly bool GenerateLog = false;
			public static readonly string HearthstoneDirectory = "";
			public static readonly bool HideDecksInOverlay = false;
			public static readonly bool HideDrawChances = false;
			public static readonly bool HideInBackground = false;
			public static readonly bool HideInMenu = false;
			public static readonly bool HideOpponentCardAge = false;
			public static readonly bool HideOpponentCardCount = false;
			public static readonly bool HideOpponentCardMarks = false;
			public static readonly bool HideOpponentCards = false;
			public static readonly bool HideOpponentDrawChances = false;
			public static readonly bool HideOverlay = false;
			public static readonly bool HidePlayerCardCount = false;
			public static readonly bool HidePlayerCards = false;
			public static readonly bool HideSecrets = false;
			public static readonly bool HideTimers = false;
			public static readonly bool HighlightCardsInHand = false;
			public static readonly bool HighlightDiscarded = false;
			public static readonly bool HighlightLastDrawn = true;
			public static readonly bool KeepDecksVisible = true;
			public static readonly string KeyPressOnGameEnd = "None";
			public static readonly string KeyPressOnGameStart = "None";
			public static readonly string LastDeck = "";
			public static readonly bool ManaCurveMyDecks = true;
			public static readonly bool ManaCurveNewDeck = true;
			public static readonly bool MinimizeToTray = false;
			public static readonly double NameDeckX = 0.8;
			public static readonly double NameDeckY = 0.05;
			public static readonly int OffsetX = 0;
			public static readonly int OffsetY = 0;
			public static readonly double OpponentDeckHeight = 65;
			public static readonly double OpponentDeckLeft = 0.5;
			public static readonly double OpponentDeckTop = 17;
			public static readonly double OpponentOpacity = 100;
			public static readonly int OpponentWindowHeight = 400;
			public static readonly int? OpponentWndLeft = null;
			public static readonly int? OpponentWndTop = null;
			public static readonly bool OverlayCardToolTips = true;
			public static readonly double OverlayOpacity = 100;
			public static readonly double OverlayOpponentScaling = 100;
			public static readonly double OverlayPlayerScaling = 100;
			public static readonly double PlayerDeckHeight = 65;
			public static readonly double PlayerDeckLeft = 99.5;
			public static readonly double PlayerDeckTop = 17;
			public static readonly double PlayerOpacity = 100;
			public static readonly int PlayerWindowHeight = 400;
			public static readonly int? PlayerWndLeft = null;
			public static readonly int? PlayerWndTop = null;
			public static readonly bool PrioritizeGolden = true;
			public static readonly bool RemoveCardsFromDeck = false;
			public static readonly bool SaveInAppData = true;
			public static readonly bool SavePlayedGames = false;
			public static readonly string SavePlayedGamesName = "Game";
			public static readonly string SavePlayedGamesPath = "";
			public static readonly double SearchBoxPosY = 0.92;
			public static readonly double SearchBoxX = 0.5;
			public static readonly int SearchDelay = 100;
			public static readonly double SecretsLeft = 15;
			public static readonly double SecretsTop = 5;
			public static readonly string SelectedDeckSorting = "Name";
			public static readonly string SelectedLanguage = "enUS";
			public static readonly List<string> SelectedTags = new List<string>();
			public static readonly string SelectedWindowBackground = "Theme";
			public static readonly bool ShowAllDecks = false;
			public static readonly bool ShowInTaskbar = false;
			public static readonly bool ShowPlayerGet = false;
			public static readonly bool StartMinimized = false;
			public static readonly Operation TagOperation = Operation.Or;
			public static readonly bool TextOnTopOpponent = false;
			public static readonly bool TextOnTopPlayer = false;
			public static readonly string ThemeName;
			public static readonly double TimerLeft = 75;
			public static readonly int TimerWindowHeight = 130;
			public static readonly int? TimerWndLeft = null;
			public static readonly bool TimerWindowOnStartup = false;
			public static readonly int? TimerWndTop = null;
			public static readonly bool TimerWindowTopmost = false;
			public static readonly bool TimerWindowTopmostIfHsForeground = false;
			public static readonly int TimerWindowWidth = 150;
			public static readonly double TimersHorizontalPosition = 80;
			public static readonly double TimersHorizontalSpacing = 0;
			public static readonly double TimersVerticalPosition = 43.5;
			public static readonly double TimersVerticalSpacing = 50;
			public static readonly bool TrackerCardToolTips = true;
			public static readonly int? MainWndLeft = null;
			public static readonly int? MainWndTop = null;
			public static readonly int UpdateDelay = 100;
			public static readonly bool UseSameScaling = true;
			public static readonly bool VisibleOverlay = false;
			public static readonly bool WindowCardToolTips = true;
			public static readonly int WindowHeight = 672;
			public static readonly string WindowsBackgroundHex = "#696969";
			public static readonly bool WindowsOnStartup = false;
			public static readonly bool WindowsTopmost = false;
			public static readonly bool WindowsTopmostIfHsForeground = false;

		}
	}
}