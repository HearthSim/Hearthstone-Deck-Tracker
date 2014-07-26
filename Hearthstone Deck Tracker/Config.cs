#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class Config
	{
		public readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\HearthstoneDeckTracker";

		public string AccentName;
		[Obsolete("Always True")]
		public bool AlwaysOverwriteLogConfig = true;
		public bool AutoDeckDetection = true;
		public bool AutoSelectDetectedDeck = true;
		public bool BringHsToForeground = false;
		public double Card2PosX = 0.25;
		public double CardPosX = 0.15;
		public double CardPosY = 0.3;
		public bool CardSortingClassFirst = false;
		public int ClickDelay = 50;
		public int CustomHeight = -1;
		public int CustomWidth = -1;
		[XmlIgnore]
		public bool Debug = false;
		public bool ExportSetDeckName = true;
		public bool FlashHs = true;
		public bool GenerateLog = false;
		public string HearthstoneDirectory = "";
		public bool HideDecksInOverlay = false;
		public bool HideDrawChances = false;
		public bool HideInBackground = false;
		public bool HideInMenu = false;
		public bool HideOpponentCardAge = false;
		public bool HideOpponentCardCount = false;
		public bool HideOpponentCardMarks = false;
		public bool HideOpponentCards = false;
		public bool HideOpponentDrawChances = false;
		public bool HideOverlay = false;
		public bool HidePlayerCardCount = false;
		public bool HidePlayerCards = false;
		public bool HideSecrets = false;
		public bool HideTimers = false;
		public bool HighlightCardsInHand = false;
		public bool HighlightDiscarded = false;
		public bool KeepDecksVisible = true;
		public string KeyPressOnGameEnd = "None";
		public string KeyPressOnGameStart = "None";
		public string LastDeck = "";
		public bool ManaCurveMyDecks = true;
		public bool ManaCurveNewDeck = true;
		public bool MinimizeToTray = false;
		public double NameDeckX = 0.8;
		public double NameDeckY = 0.05;
		public int OffsetX = 0;
		public int OffsetY = 0;
		public double OpponentDeckHeight = 65;
		public double OpponentDeckLeft = 0.5;
		public double OpponentDeckTop = 17;
		public double OpponentOpacity = 100;
		public int OpponentWindowHeight = 0;
		public double OpponentWindowLeft = -32000;
		public double OpponentWindowTop = -32000;
		public bool OverlayCardToolTips = true;
		public double OverlayOpacity = 100;
		public double OverlayOpponentScaling = 100;
		public double OverlayPlayerScaling = 100;
		public double PlayerDeckHeight = 65;
		public double PlayerDeckLeft = 99.5;
		public double PlayerDeckTop = 17;
		public double PlayerOpacity = 100;
		public int PlayerWindowHeight = 0;
		public double PlayerWindowLeft = -32000;
		public double PlayerWindowTop = -32000;
		public bool PrioritizeGolden = true;
		public bool SaveInAppData = true;
		public bool SavePlayedGames = false;
		public string SavePlayedGamesName = "Game";
		public string SavePlayedGamesPath = "";
		public double SearchBoxX = 0.5;
		public double SearchBoxPosY = 0.92;
		public int SearchDelay = 100;
		public double SecretsLeft = 15;
		public double SecretsTop = 5;
		public string SelectedLanguage = "enUS";
		[XmlArray(ElementName = "SelectedTags")]
		[XmlArrayItem(ElementName = "Tag")]
		public List<string> SelectedTags = new List<string>();
		public string SelectedWindowBackground = "Theme";
		public bool ShowAllDecks = false;
		public bool ShowInTaskbar = false;
		public Operation TagOperation = Operation.Or;
		public bool TextOnTopOpponent = false;
		public bool TextOnTopPlayer = false;
		public string ThemeName;
		public double TimerLeft = 75;
		public double TimerWindowHeight = 130;
		public double TimerWindowLeft = -32000;
		public bool TimerWindowOnStartup = false;
		public double TimerWindowTop = -32000;
		public bool TimerWindowTopmost = false;
		public bool TimerWindowTopmostIfHsForeground = false;
		public double TimerWindowWidth = 150;
		public double TimersHorizontalPosition = 80;
		public double TimersHorizontalSpacing = 0;
		public double TimersVerticalPosition = 50;
		public double TimersVerticalSpacing = 50;
		public bool TrackerCardToolTips = true;
		public int TrackerWindowLeft = -1;
		public int TrackerWindowTop = -1;
		public int UpdateDelay = 100;
		public bool UseSameScaling = true;
		public bool VisibleOverlay = false;
		public bool WindowCardToolTips = true;
		public int WindowHeight = 672;
		public string WindowsBackgroundHex = "#696969";
		public bool WindowsOnStartup = false;
		public bool WindowsTopmost = false;
		public bool WindowsTopmostIfHsForeground = false;
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


		private string GetLogFileName()
		{
			var date = DateTime.Now;
			_currentLogFile = string.Format("Logs/log_{0}{1}{2}-{3}{4}{5}.txt", date.Day, date.Month, date.Year,
											date.Hour,
											date.Minute, date.Second);
			return _currentLogFile;
		}


		private static Config _Config = new Config();
		public static Config Instance { get { return _Config; } }


		public static string Load(XmlManager<Config> _xmlManagerConfig)
		{
			string _configPath;
			//var _Config = new Config();
			//var _xmlManagerConfig = new XmlManager<Config> { Type = typeof(Config) };

			bool foundConfig = false;
			try
			{
				if (File.Exists("config.xml"))
				{
					_Config = _xmlManagerConfig.Load("config.xml");
					foundConfig = true;
				}
				else if (File.Exists(_Config.AppDataPath + @"\config.xml"))
				{
					_Config = _xmlManagerConfig.Load(_Config.AppDataPath + @"\config.xml");
					foundConfig = true;
				}
				else if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
				{
					//save locally if appdata doesn't exist (when e.g. not on C)
					_Config.SaveInAppData = false;
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(
					e.Message + "\n\n" + e.InnerException +
					"\n\n If you don't know how to fix this, please delete " + _Config.ConfigPath,
					"Error loading config.xml");
				Application.Current.Shutdown();
			}

			_configPath = _Config.ConfigPath;
			if (!foundConfig)
			{
				if (_Config.HomeDir != string.Empty)
					Directory.CreateDirectory(_Config.HomeDir);
				using (var sr = new StreamWriter(_Config.ConfigPath, false))
				{
					sr.WriteLine("<Config></Config>");
				}
			}
			else if (_Config.SaveInAppData) //check if config needs to be moved
			{
				if (File.Exists("config.xml"))
				{
					Directory.CreateDirectory(_Config.HomeDir);
					if (File.Exists(_Config.ConfigPath))
					{
						//backup in case the file already exists
						File.Move(_configPath, _configPath + DateTime.Now.ToFileTime());
					}
					File.Move("config.xml", _Config.ConfigPath);
					Logger.WriteLine("Moved config to appdata");
				}
			}
			else
			{
				if (File.Exists(_Config.AppDataPath + @"\config.xml"))
				{
					if (File.Exists(_Config.ConfigPath))
					{
						//backup in case the file already exists
						File.Move(_configPath, _configPath + DateTime.Now.ToFileTime());
					}
					File.Move(_Config.AppDataPath + @"\config.xml", _Config.ConfigPath);
					Logger.WriteLine("Moved config to local");
				}
			}

			return _configPath;
		}


	}
}