using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone;

//using System.ComponentModel;

namespace Hearthstone_Deck_Tracker
{
	public class Config
	{
		#region Settings

		private static Config _config; //= new Config();

		public readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\HearthstoneDeckTracker";

		[DefaultValue("")]
		public string AccentName = "";

		[DefaultValue(true)]
		public bool AdditionalOverlayTooltips = true;

		[DefaultValue(true)]
		public bool AdvancedWindowSearch = true;

		[DefaultValue(true)]
		public bool AlwaysOverwriteLogConfig = true;

		[DefaultValue(true)]
		public bool AutoClearDeck = true;

		[DefaultValue(true)]
		public bool AutoDeckDetection = true;

		[DefaultValue(true)]
		public bool AutoSelectDetectedDeck = true;

		[DefaultValue(false)]
		public bool BringHsToForeground = false;

		[DefaultValue(false)]
		public bool CardSortingClassFirst = false;

		[DefaultValue(true)]
		public bool CheckForUpdates = true;

		[DefaultValue(true)]
		public bool ClearLogFileAfterGame = true;

		[DefaultValue(false)]
		public bool CloseWithHearthstone = false;

		[DefaultValue("")]
		public string CreatedByVersion = "";

		[DefaultValue(-1)]
		public int CustomHeight = -1;

		[DefaultValue(-1)]
		public int CustomWidth = -1;

		[DefaultValue(false)]
		[XmlIgnore]
		public bool Debug = false;

		[DefaultValue(50)]
		public int DeckExportDelay = 50;

		[DefaultValue(false)]
		public bool DiscardGameIfIncorrectDeck = false;

		[DefaultValue(true)]
		public bool EnterToSaveNote = true;

		[DefaultValue(0.06)]
		public double ExportAllButtonX = 0.06;

		[DefaultValue(0.915)]
		public double ExportAllButtonY = 0.915;

		[DefaultValue(0.12)]
		public double ExportCard1X = 0.12;

		[DefaultValue(0.285)]
		public double ExportCard2X = 0.285;

		[DefaultValue(0.32)]
		public double ExportCardsY = 0.32;

		[DefaultValue(0.86)]
		public double ExportClearX = 0.83;

		[DefaultValue(0.16)]
		public double ExportClearY = 0.13;

		[DefaultValue(0.2)]
		public double ExportClearCheckYFixed = 0.185;

		[DefaultValue(0.85)]
		public double ExportNameDeckX = 0.85;

		[DefaultValue(0.075)]
		public double ExportNameDeckY = 0.075;

		[DefaultValue(false)]
		public bool ExportPasteClipboard = false;

		[DefaultValue(0.5)]
		public double ExportSearchBoxX = 0.5;

		[DefaultValue(0.915)]
		public double ExportSearchBoxY = 0.915;

		[DefaultValue(true)]
		public bool ExportSetDeckName = true;

		[DefaultValue(false)]
		public bool ExtraFeatures = false;

		[DefaultValue(true)]
		public bool FlashHsOnTurnStart = true;

		private GameDetailsConfig _gameDetails;
		public GameDetailsConfig GameDetails
		{
			get { return _gameDetails ?? (_gameDetails = new GameDetailsConfig()); }
			set { _gameDetails = value; }
		}
		
		[DefaultValue("")]
		public string HearthstoneDirectory = "";

		[DefaultValue(false)]
		public bool HideDecksInOverlay = false;

		[DefaultValue(false)]
		public bool HideDrawChances = false;

		[DefaultValue(false)]
		public bool HideInBackground = false;

		[DefaultValue(false)]
		public bool HideInMenu = false;

		[DefaultValue(false)]
		public bool HideOpponentCardAge = false;

		[DefaultValue(false)]
		public bool HideOpponentCardCount = false;

		[DefaultValue(false)]
		public bool HideOpponentCardMarks = false;

		[DefaultValue(false)]
		public bool HideOpponentCards = false;

		[DefaultValue(false)]
		public bool HideOpponentDrawChances = false;

		[DefaultValue(false)]
		public bool HideOverlay = false;

		[DefaultValue(false)]
		public bool HidePlayerCardCount = false;

		[DefaultValue(false)]
		public bool HidePlayerCards = false;

		[DefaultValue(false)]
		public bool HideSecrets = false;

		[DefaultValue(false)]
		public bool HideTimers = false;

		[DefaultValue(false)]
		public bool HighlightCardsInHand = false;

		[DefaultValue(false)]
		public bool HighlightDiscarded = false;

		[DefaultValue(true)]
		public bool HighlightLastDrawn = true;

		[DefaultValue(true)]
		public bool KeepDecksVisible = true;

		[DefaultValue(true)]
		public bool KeepStatsWhenDeletingDeck = true;

		[DefaultValue("None")]
		public string KeyPressOnGameEnd = "None";

		[DefaultValue("None")]
		public string KeyPressOnGameStart = "None";

		[DefaultValue("")]
		public string LastDeck = "";

		[DefaultValue(0)]
		public int LogLevel = 0;

		[DefaultValue(true)]
		public bool ManaCurveMyDecks = true;

		[DefaultValue(false)]
		public bool MinimizeToTray = false;

		[DefaultValue(0)]
		public int OffsetX = 0;

		[DefaultValue(0)]
		public int OffsetY = 0;

		[DefaultValue(65)]
		public double OpponentDeckHeight = 65;

		[DefaultValue(0.5)]
		public double OpponentDeckLeft = 0.5;

		[DefaultValue(17)]
		public double OpponentDeckTop = 17;

		[DefaultValue(100)]
		public double OpponentOpacity = 100;

		[DefaultValue(400)]
		public int OpponentWindowHeight = 400;

		[DefaultValue(null)]
		public int? OpponentWindowLeft = null;

		[DefaultValue(false)]
		public bool OpponentWindowOnStart = false;

		[DefaultValue(null)]
		public int? OpponentWindowTop = null;

		[DefaultValue(true)]
		public bool OverlayCardToolTips = true;

		[DefaultValue(100)]
		public double OverlayOpacity = 100;

		[DefaultValue(100)]
		public double OverlayOpponentScaling = 100;

		[DefaultValue(100)]
		public double OverlayPlayerScaling = 100;

		[DefaultValue(false)]
		public bool OverlaySecretToolTipsOnly = false;

		[DefaultValue(false)]
		public bool OwnsGoldenFeugen = false;

		[DefaultValue(false)]
		public bool OwnsGoldenStalagg = false;

		[DefaultValue(new[] {"Win Rate", "Cards", "Draw Chances", "Card Counter"})]
		public string[] PanelOrderOpponent = { "Win Rate", "Cards", "Draw Chances", "Card Counter" };

		[DefaultValue(new[] {"Deck Title", "Wins", "Cards", "Draw Chances", "Card Counter"})]
		public string[] PanelOrderPlayer = { "Deck Title", "Wins", "Cards", "Draw Chances", "Card Counter" };

		[DefaultValue(65)]
		public double PlayerDeckHeight = 65;

		[DefaultValue(99.5)]
		public double PlayerDeckLeft = 99.5;

		[DefaultValue(17)]
		public double PlayerDeckTop = 17;

		[DefaultValue(100)]
		public double PlayerOpacity = 100;

		[DefaultValue(400)]
		public int PlayerWindowHeight = 400;

		[DefaultValue(null)]
		public int? PlayerWindowLeft = null;

		[DefaultValue(false)]
		public bool PlayerWindowOnStart = false;

		[DefaultValue(null)]
		public int? PlayerWindowTop = null;

		[DefaultValue(true)]
		public bool PrioritizeGolden = true;

		[DefaultValue(true)]
		public bool RecordArena = true;

		[DefaultValue(true)]
		public bool RecordCasual = true;

		[DefaultValue(true)]
		public bool RecordFriendly = true;

		[DefaultValue(false)]
		public bool RecordOther = false;

		[DefaultValue(false)]
		public bool RecordPractice = false;

		[DefaultValue(true)]
		public bool RecordRanked = true;

		[DefaultValue(false)]
		public bool RemoveCardsFromDeck = false;

		//updating from <= 0.5.1: 
		//SaveConfigInAppData and SaveDataInAppData are set to SaveInAppData AFTER the config isloaded
		//=> Need to be null to avoid creating new config in appdata if config is stored locally.
		[DefaultValue(true)]
		public bool? SaveConfigInAppData = null;

		[DefaultValue(true)]		
		public bool? SaveDataInAppData = null;

		[DefaultValue(true)]
		public bool SaveInAppData = true;

		[DefaultValue(1)]
		public double SecretsPanelScaling = 1;

		[DefaultValue(15)]
		public double SecretsLeft = 15;

		[DefaultValue(5)]
		public double SecretsTop = 5;

		[DefaultValue("Name")]
		public string SelectedDeckSorting = "Name";

		[DefaultValue("enUS")]
		public string SelectedLanguage = "enUS";

		[DefaultValue(Game.GameMode.All)]
		public Game.GameMode SelectedStatsFilterGameMode = Game.GameMode.All;

		[DefaultValue("All Time")]
		public string SelectedStatsFilterTime = "All Time";

		[XmlArray(ElementName = "SelectedTags")]
		[XmlArrayItem(ElementName = "Tag")]
		public List<string> SelectedTags = new List<string>();

		[DefaultValue("Theme")]
		public string SelectedWindowBackground = "Theme";

		[DefaultValue(false)]
		public bool ShowAllDecks = false;

		[DefaultValue(false)]
		public bool ShowDeckTitle = false;

		[DefaultValue(false)]
		public bool ShowDeckWins = false;

		[DefaultValue(false)]
		public bool ShowInTaskbar = false;

		[DefaultValue(false)]
		public bool ShowLogTab = false;

		[DefaultValue(false)]
		public bool ShowNoteDialogAfterGame = false;

		[DefaultValue(false)]
		public bool ShowPlayerGet = false;

		[DefaultValue(false)]
		public bool ShowWinRateAgainst = false;

		[DefaultValue(false)]
		public bool StartMinimized = false;

		[DefaultValue(false)]
		public bool StatsClassOverviewIsExpanded = false;

		//[DefaultValue(true)]
		//public bool StatsDeckOverviewIsExpanded = true;

		[DefaultValue(false)]
		public bool StatsInWindow = false;

		[DefaultValue(672)]
		public int StatsWindowHeight = 672;

		[DefaultValue(null)]
		public int? StatsWindowLeft = null;

		[DefaultValue(null)]
		public int? StatsWindowTop = null;

		[DefaultValue(510)]
		public int StatsWindowWidth = 510;

		[DefaultValue(true)]
		public bool TagDecksOnImport = true;

		[DefaultValue(Operation.Or)]
		public Operation TagOperation = Operation.Or;

		[DefaultValue("")]
		public string ThemeName = "";

		[DefaultValue(75)]
		public double TimerLeft = 75;

		[DefaultValue(130)]
		public int TimerWindowHeight = 130;

		[DefaultValue(null)]
		public int? TimerWindowLeft = null;

		[DefaultValue(false)]
		public bool TimerWindowOnStartup = false;

		[DefaultValue(null)]
        public int? TimerWindowTop = null;

        [DefaultValue(false)]
        public bool TimerWindowTopmost = false;

        [DefaultValue(false)]
        public bool TimerAlert = false;

        [DefaultValue(30)]
        public int TimerAlertSeconds = 30;

		[DefaultValue(false)]
		public bool TimerWindowTopmostIfHsForeground = false;

		[DefaultValue(150)]
		public int TimerWindowWidth = 150;

		[DefaultValue(80)]
		public double TimersHorizontalPosition = 80;

		[DefaultValue(0)]
		public double TimersHorizontalSpacing = 0;

		[DefaultValue(43.5)]
		public double TimersVerticalPosition = 43.5;

		[DefaultValue(50)]
		public double TimersVerticalSpacing = 50;

		[DefaultValue(true)]
		public bool TrackerCardToolTips = true;

		[DefaultValue(null)]
		public int? TrackerWindowLeft = null;

		[DefaultValue(null)]
		public int? TrackerWindowTop = null;

		[DefaultValue(100)]
		public int UpdateDelay = 100;

		[DefaultValue(false)]
		public bool UseFullTextSearch = false;

		[DefaultValue(true)]
		public bool UseSameScaling = true;

		[DefaultValue(false)]
		public bool VisibleOverlay = false;

		[DefaultValue(true)]
		public bool WindowCardToolTips = true;

		[DefaultValue(620)]
		public int WindowHeight = 620;

		[DefaultValue(550)]
		public int WindowWidth = 550;

		[DefaultValue("#696969")]
		public string WindowsBackgroundHex = "#696969";

		[DefaultValue(false)]
		public bool WindowsTopmost = false;

		[DefaultValue(false)]
		public bool WindowsTopmostIfHsForeground = false;

		private string _currentLogFile;

		#endregion

		#region Properties

		[Obsolete]
		public string HomeDir
		{
			get { return Instance.SaveInAppData ? AppDataPath + "/" : string.Empty; }
		}

		public string ConfigPath
		{
			get { return Instance.ConfigDir + "config.xml"; }
		}

		public string ConfigDir
		{
			get { return Instance.SaveConfigInAppData == false ? string.Empty : AppDataPath + "\\"; }
		}

		public string DataDir
		{
			get { return Instance.SaveDataInAppData == false ? string.Empty : AppDataPath + "\\"; }
		}

		public string LogFilePath
		{
			get { return Instance._currentLogFile ?? GetLogFileName(); }
		}

		public static Config Instance
		{
			get
			{
				if(_config == null)
				{
					_config = new Config();
					_config.ResetAll();
					_config.SelectedTags = new List<string>();
					_config.GameDetails = new GameDetailsConfig();
				}

				return _config;
			}
		}


		#endregion

		#region Misc

		private Config()
		{
		}

		private string GetLogFileName()
		{
			var date = DateTime.Now;
			Instance._currentLogFile = string.Format("Logs/log_{0}{1}{2}-{3}{4}{5}.txt", date.Day, date.Month, date.Year,
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

			if(File.Exists(configPath))
			{
				File.Copy(configPath, configPath + DateTime.Now.ToFileTime());

				if(deleteOriginal)
					File.Delete(configPath);
			}
		}

		public static void Load()
		{
			var foundConfig = false;
			try
			{
				if(File.Exists("config.xml"))
				{
					_config = XmlManager<Config>.Load("config.xml");
					foundConfig = true;
				}
				else if(File.Exists(Instance.AppDataPath + @"\config.xml"))
				{
					_config = XmlManager<Config>.Load(Instance.AppDataPath + @"\config.xml");
					foundConfig = true;
				}
				else if(!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)))
					//save locally if appdata doesn't exist (when e.g. not on C)
					Instance.SaveConfigInAppData = false;
			}
			catch(Exception e)
			{
				MessageBox.Show(
					e.Message + "\n\n" + e.InnerException +
					"\n\n If you don't know how to fix this, please delete " + Instance.ConfigPath,
					"Error loading config.xml");
				Application.Current.Shutdown();
			}

			if(!foundConfig)
			{
				if(Instance.ConfigDir != string.Empty)
					Directory.CreateDirectory(Instance.ConfigDir);
				using(var sr = new StreamWriter(Instance.ConfigPath, false))
					sr.WriteLine("<Config></Config>");
			}
			else if(Instance.SaveConfigInAppData != null)
			{
				if(Instance.SaveConfigInAppData.Value) //check if config needs to be moved
				{
					if(File.Exists("config.xml"))
					{
						Directory.CreateDirectory(Instance.ConfigDir);
						SaveBackup(true); //backup in case the file already exists
						File.Move("config.xml", Instance.ConfigPath);
						Logger.WriteLine("Moved config to appdata");
					}
				}
				else if(File.Exists(Instance.AppDataPath + @"\config.xml"))
				{
					SaveBackup(true); //backup in case the file already exists
					File.Move(Instance.AppDataPath + @"\config.xml", Instance.ConfigPath);
					Logger.WriteLine("Moved config to local");
				}
			}
		}

		public void ResetAll()
		{
			foreach(var field in GetType().GetFields())
			{
				var attr = (DefaultValueAttribute)field.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault();
				if(attr != null)
					field.SetValue(this, attr.Value);
			}
			
			/*
			foreach(System.ComponentModel.PropertyDescriptor prop in System.ComponentModel.TypeDescriptor.GetProperties(this))
			{
				var attr = (DefaultValueAttribute)prop.Attributes[typeof(DefaultValueAttribute)];
				if(attr != null)
				{
					prop.SetValue(this, attr.Value);
				}
			}
			*/
		}

		public void Reset(string name)
		{
			//TODO: Upgrade to use LINQ and not the property's name!!
			//var proper = System.ComponentModel.TypeDescriptor.GetProperties(this).OfType<System.ComponentModel.PropertyDescriptor>().First(x => x.Name == PropertyName);
			//var attr = (DefaultValueAttribute)proper.Attributes[typeof(DefaultValueAttribute)];

			var proper = GetType().GetFields().First(x => x.Name == name);
			var attr = (DefaultValueAttribute)proper.GetCustomAttributes(typeof(DefaultValueAttribute), false).First();
			proper.SetValue(this, attr.Value);
		}

		[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
		private sealed class DefaultValueAttribute : Attribute
		{
			// This is a positional argument
			public DefaultValueAttribute(object value)
			{
				Value = value;
			}

			public object Value { get; private set; }
		}

		public class GameDetailsConfig
		{
			public bool ShowOpponentDraw = false;
			public bool ShowOpponentMulligan = false;
			public bool ShowOpponentPlay = true;
			public bool ShowPlayerDraw = false;
			public bool ShowPlayerMulligan = false;
			public bool ShowPlayerPlay = true;
		}

		#endregion
	}
}