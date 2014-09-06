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
		public string AccentName;

		[DefaultValue(true)]
		public bool AdditionalOverlayTooltips;

		[DefaultValue(true)]
		public bool AlwaysOverwriteLogConfig;

		[DefaultValue(true)]
		public bool AutoDeckDetection;

		[DefaultValue(true)]
		public bool AutoSelectDetectedDeck;

		[DefaultValue(false)]
		public bool BringHsToForeground;

		[DefaultValue(false)]
		public bool CardSortingClassFirst;

		[DefaultValue(true)]
		public bool CheckForUpdates;

		[DefaultValue(true)]
		public bool ClearLogFileAfterGame;

		[DefaultValue(false)]
		public bool CloseWithHearthstone;

		[DefaultValue("")]
		public string CreatedByVersion;

		[DefaultValue(-1)]
		public int CustomHeight;

		[DefaultValue(-1)]
		public int CustomWidth;

		[DefaultValue(false)]
		[XmlIgnore]
		public bool Debug;

		[DefaultValue(50)]
		public int DeckExportDelay;

		[DefaultValue(false)]
		public bool DiscardGameIfIncorrectDeck;

		[DefaultValue(0.06)]
		public double ExportAllButtonX;

		[DefaultValue(0.915)]
		public double ExportAllButtonY;

		[DefaultValue(0.12)]
		public double ExportCard1X;

		[DefaultValue(0.285)]
		public double ExportCard2X;

		[DefaultValue(0.32)]
		public double ExportCardsY;

		[DefaultValue(0.85)]
		public double ExportNameDeckX;

		[DefaultValue(0.075)]
		public double ExportNameDeckY;

		[DefaultValue(false)]
		public bool ExportPasteClipboard;

		[DefaultValue(0.5)]
		public double ExportSearchBoxX;

		[DefaultValue(0.915)]
		public double ExportSearchBoxY;

		[DefaultValue(true)]
		public bool ExportSetDeckName;

		[DefaultValue(false)]
		public bool ExtraFeatures;

		[DefaultValue(true)]
		public bool FlashHsOnTurnStart;

		public GameDetailsConfig GameDetails;

		[DefaultValue(false)]
		public bool GenerateLog;

		[DefaultValue("")]
		public string HearthstoneDirectory;

		[DefaultValue(false)]
		public bool HideDecksInOverlay;

		[DefaultValue(false)]
		public bool HideDrawChances;

		[DefaultValue(false)]
		public bool HideInBackground;

		[DefaultValue(false)]
		public bool HideInMenu;

		[DefaultValue(false)]
		public bool HideOpponentCardAge;

		[DefaultValue(false)]
		public bool HideOpponentCardCount;

		[DefaultValue(false)]
		public bool HideOpponentCardMarks;

		[DefaultValue(false)]
		public bool HideOpponentCards;

		[DefaultValue(false)]
		public bool HideOverlay;

		[DefaultValue(false)]
		public bool HidePlayerCardCount;

		[DefaultValue(false)]
		public bool HidePlayerCards;

		[DefaultValue(false)]
		public bool HideSecrets;

		[DefaultValue(false)]
		public bool HideTimers;

		[DefaultValue(false)]
		public bool HighlightCardsInHand;

		[DefaultValue(false)]
		public bool HighlightDiscarded;

		[DefaultValue(true)]
		public bool HighlightLastDrawn;

		[DefaultValue(true)]
		public bool KeepDecksVisible;

		[DefaultValue("None")]
		public string KeyPressOnGameEnd;

		[DefaultValue("None")]
		public string KeyPressOnGameStart;

		[DefaultValue("")]
		public string LastDeck;

		[DefaultValue(0)]
		public int LogLevel;

		[DefaultValue(true)]
		public bool ManaCurveMyDecks;

		[DefaultValue(false)]
		public bool MinimizeToTray;

		[DefaultValue(0)]
		public int OffsetX;

		[DefaultValue(0)]
		public int OffsetY;

		[DefaultValue(65)]
		public double OpponentDeckHeight;

		[DefaultValue(0.5)]
		public double OpponentDeckLeft;

		[DefaultValue(17)]
		public double OpponentDeckTop;

		[DefaultValue(100)]
		public double OpponentOpacity;

		[DefaultValue(400)]
		public int OpponentWindowHeight;

		[DefaultValue(null)]
		public int? OpponentWindowLeft;

		[DefaultValue(false)]
		public bool OpponentWindowOnStart;

		[DefaultValue(null)]
		public int? OpponentWindowTop;

		[DefaultValue(true)]
		public bool OverlayCardToolTips;

		[DefaultValue(100)]
		public double OverlayOpacity;

		[DefaultValue(100)]
		public double OverlayOpponentScaling;

		[DefaultValue(100)]
		public double OverlayPlayerScaling;

		[DefaultValue(false)]
		public bool OverlaySecretToolTipsOnly;

		[DefaultValue(false)]
		public bool OwnsGoldenFeugen;

		[DefaultValue(false)]
		public bool OwnsGoldenStalagg;

		[DefaultValue(new[] {"Win Rate", "Cards", "Draw Chances", "Card Counter"})]
		public string[] PanelOrderOpponent;

		[DefaultValue(new[] {"Deck Title", "Wins", "Cards", "Draw Chances", "Card Counter"})]
		public string[] PanelOrderPlayer;

		[DefaultValue(65)]
		public double PlayerDeckHeight;

		[DefaultValue(99.5)]
		public double PlayerDeckLeft;

		[DefaultValue(17)]
		public double PlayerDeckTop;

		[DefaultValue(100)]
		public double PlayerOpacity;

		[DefaultValue(400)]
		public int PlayerWindowHeight;

		[DefaultValue(null)]
		public int? PlayerWindowLeft;

		[DefaultValue(false)]
		public bool PlayerWindowOnStart;

		[DefaultValue(null)]
		public int? PlayerWindowTop;

		[DefaultValue(true)]
		public bool PrioritizeGolden;

		[DefaultValue(true)]
		public bool RecordArena;

		[DefaultValue(true)]
		public bool RecordCasual;

		[DefaultValue(true)]
		public bool RecordFriendly;

		[DefaultValue(false)]
		public bool RecordOther;

		[DefaultValue(false)]
		public bool RecordPractice;

		[DefaultValue(true)]
		public bool RecordRanked;

		[DefaultValue(false)]
		public bool RemoveCardsFromDeck;
		
		[DefaultValue(true)]
		public bool SaveConfigInAppData;
		
		[DefaultValue(true)]		
		public bool SaveDataInAppData;

		[DefaultValue(true)][Obsolete]
		public bool SaveInAppData;

		[DefaultValue(35)]
		public double SecretsHeight;

		[DefaultValue(15)]
		public double SecretsLeft;

		[DefaultValue(5)]
		public double SecretsTop;

		[DefaultValue("Name")]
		public string SelectedDeckSorting;

		[DefaultValue("enUS")]
		public string SelectedLanguage;

		[DefaultValue(Game.GameMode.All)]
		public Game.GameMode SelectedStatsFilterGameMode;

		[DefaultValue("All Time")]
		public string SelectedStatsFilterTime;

		[XmlArray(ElementName = "SelectedTags")]
		[XmlArrayItem(ElementName = "Tag")]
		public List<string> SelectedTags;

		[DefaultValue("Theme")]
		public string SelectedWindowBackground;

		[DefaultValue(false)]
		public bool ShowAllDecks;

		[DefaultValue(false)]
		public bool ShowDeckTitle;

		[DefaultValue(false)]
		public bool ShowDeckWins;

		[DefaultValue(false)]
		public bool ShowInTaskbar;

		[DefaultValue(false)]
		public bool ShowPlayerGet;

		[DefaultValue(false)]
		public bool ShowWinRateAgainst;

		[DefaultValue(false)]
		public bool StartMinimized;

		[DefaultValue(false)]
		public bool StatsClassOverviewIsExpanded;

		[DefaultValue(true)]
		public bool StatsDeckOverviewIsExpanded;

		[DefaultValue(false)]
		public bool StatsInWindow;

		[DefaultValue(672)]
		public int StatsWindowHeight;

		[DefaultValue(null)]
		public int? StatsWindowLeft;

		[DefaultValue(null)]
		public int? StatsWindowTop;

		[DefaultValue(510)]
		public int StatsWindowWidth;

		[DefaultValue(true)]
		public bool TagDecksOnImport;

		[DefaultValue(Operation.Or)]
		public Operation TagOperation;

		[DefaultValue("")]
		public string ThemeName;

		[DefaultValue(75)]
		public double TimerLeft;

		[DefaultValue(130)]
		public int TimerWindowHeight;

		[DefaultValue(null)]
		public int? TimerWindowLeft;

		[DefaultValue(false)]
		public bool TimerWindowOnStartup;

		[DefaultValue(null)]
		public int? TimerWindowTop;

		[DefaultValue(false)]
		public bool TimerWindowTopmost;

		[DefaultValue(false)]
		public bool TimerWindowTopmostIfHsForeground;

		[DefaultValue(150)]
		public int TimerWindowWidth;

		[DefaultValue(80)]
		public double TimersHorizontalPosition;

		[DefaultValue(0)]
		public double TimersHorizontalSpacing;

		[DefaultValue(43.5)]
		public double TimersVerticalPosition;

		[DefaultValue(50)]
		public double TimersVerticalSpacing;

		[DefaultValue(true)]
		public bool TrackerCardToolTips;

		[DefaultValue(null)]
		public int? TrackerWindowLeft;

		[DefaultValue(null)]
		public int? TrackerWindowTop;

		[DefaultValue(100)]
		public int UpdateDelay;

		[DefaultValue(false)]
		public bool UseFullTextSearch;

		[DefaultValue(true)]
		public bool UseSameScaling;

		[DefaultValue(false)]
		public bool VisibleOverlay;

		[DefaultValue(true)]
		public bool WindowCardToolTips;

		[DefaultValue(620)]
		public int WindowHeight;

		[DefaultValue(550)]
		public int WindowWidth;

		[DefaultValue("#696969")]
		public string WindowsBackgroundHex;

		[DefaultValue(false)]
		public bool WindowsTopmost;

		[DefaultValue(false)]
		public bool WindowsTopmostIfHsForeground;

		[DefaultValue(true)]
		private string _currentLogFile;

		#endregion

		#region Properties

		public string HomeDir
		{
			get { return SaveInAppData ? AppDataPath + "/" : string.Empty; }
		}

		public string ConfigPath
		{
			get { return ConfigDir + "config.xml"; }
		}

		public string ConfigDir
		{
			get { return SaveConfigInAppData ? AppDataPath + "\\" : string.Empty; }
		}

		public string DataDir
		{
			get { return SaveDataInAppData ? AppDataPath + "\\" : string.Empty; }
		}

		public string LogFilePath
		{
			get { return _currentLogFile ?? GetLogFileName(); }
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
			else if(Instance.SaveConfigInAppData) //check if config needs to be moved
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
			public static readonly bool SaveConfigInAppData = true;
			public static readonly bool SaveDataInAppData = true;

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