using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Hearthstone;
using System.Linq;

namespace Hearthstone_Deck_Tracker
{
	public class Config
	{
		#region Settings

		private static Config _config; //= new Config();

		public readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\HearthstoneDeckTracker";

		[DefaultValue(false)]
		[XmlIgnore]
		public bool Debug { get; set; }

		[XmlArray(ElementName = "SelectedTags")]
		[XmlArrayItem(ElementName = "Tag")]
		public List<string> SelectedTags { get; set; }

		public GameDetailsConfig GameDetails { get; set; }

		[DefaultValue("")]
		public string AccentName { get; set; }
		[DefaultValue(true)]
		public bool AdditionalOverlayTooltips { get; set; }
		[DefaultValue(true)]
		public bool AlwaysOverwriteLogConfig { get; set; }
		[DefaultValue(true)]
		public bool AutoDeckDetection { get; set; }
		[DefaultValue(true)]
		public bool AutoSelectDetectedDeck { get; set; }
		[DefaultValue(false)]
		public bool BringHsToForeground { get; set; }
		[DefaultValue(false)]
		public bool CardSortingClassFirst { get; set; }
		[DefaultValue(true)]
		public bool CheckForUpdates { get; set; }
		[DefaultValue(true)]
		public bool ClearLogFileAfterGame { get; set; }
		[DefaultValue(false)]
		public bool CloseWithHearthstone { get; set; }
		[DefaultValue("")]
		public string CreatedByVersion;
		[DefaultValue(-1)]
		public int CustomHeight { get; set; }
		[DefaultValue(-1)]
		public int CustomWidth { get; set; }
		[DefaultValue(50)]
		public int DeckExportDelay { get; set; }
		[DefaultValue(false)]
		public bool DiscardGameIfIncorrectDeck { get; set; }
		[DefaultValue(0.06)]
		public double ExportAllButtonX { get; set; }
		[DefaultValue(0.915)]
		public double ExportAllButtonY { get; set; }
		[DefaultValue(0.12)]
		public double ExportCard1X { get; set; }
		[DefaultValue(0.285)]
		public double ExportCard2X { get; set; }
		[DefaultValue(0.32)]
		public double ExportCardsY { get; set; }
		[DefaultValue(0.85)]
		public double ExportNameDeckX { get; set; }
		[DefaultValue(0.075)]
		public double ExportNameDeckY { get; set; }
		[DefaultValue(false)]
		public bool ExportPasteClipboard { get; set; }
		[DefaultValue(0.5)]
		public double ExportSearchBoxX { get; set; }
		[DefaultValue(0.915)]
		public double ExportSearchBoxY { get; set; }
		[DefaultValue(true)]
		public bool ExportSetDeckName { get; set; }
		[DefaultValue(false)]
		public bool ExtraFeatures { get; set; }
		[DefaultValue(true)]
		public bool FlashHsOnTurnStart { get; set; }
		[DefaultValue(false)]
		public bool GenerateLog { get; set; }
		[DefaultValue("")]
		public string HearthstoneDirectory { get; set; }
		[DefaultValue(false)]
		public bool HideDecksInOverlay { get; set; }
		[DefaultValue(false)]
		public bool HideDrawChances { get; set; }
		[DefaultValue(false)]
		public bool HideInBackground { get; set; }
		[DefaultValue(false)]
		public bool HideInMenu { get; set; }
		[DefaultValue(false)]
		public bool HideOpponentCardAge { get; set; }
		[DefaultValue(false)]
		public bool HideOpponentCardCount { get; set; }
		[DefaultValue(false)]
		public bool HideOpponentCardMarks { get; set; }
		[DefaultValue(false)]
		public bool HideOpponentCards { get; set; }
		[DefaultValue(false)]
		public bool HideOpponentDrawChances { get; set; }
		[DefaultValue(false)]
		public bool HideOverlay { get; set; }
		[DefaultValue(false)]
		public bool HidePlayerCardCount { get; set; }
		[DefaultValue(false)]
		public bool HidePlayerCards { get; set; }
		[DefaultValue(false)]
		public bool HideSecrets { get; set; }
		[DefaultValue(false)]
		public bool HideTimers { get; set; }
		[DefaultValue(false)]
		public bool HighlightCardsInHand { get; set; }
		[DefaultValue(false)]
		public bool HighlightDiscarded { get; set; }
		[DefaultValue(true)]
		public bool HighlightLastDrawn { get; set; }
		[DefaultValue(true)]
		public bool KeepDecksVisible { get; set; }
		[DefaultValue("None")]
		public string KeyPressOnGameEnd { get; set; }
		[DefaultValue("None")]
		public string KeyPressOnGameStart { get; set; }
		[DefaultValue("")]
		public string LastDeck { get; set; }
		[DefaultValue(0)]
		public int LogLevel { get; set; }
		[DefaultValue(true)]
		public bool ManaCurveMyDecks { get; set; }
		[DefaultValue(false)]
		public bool MinimizeToTray { get; set; }
		[DefaultValue(0)]
		public int OffsetX { get; set; }
		[DefaultValue(0)]
		public int OffsetY { get; set; }
		[DefaultValue(65)]
		public double OpponentDeckHeight { get; set; }
		[DefaultValue(0.5)]
		public double OpponentDeckLeft { get; set; }
		[DefaultValue(17)]
		public double OpponentDeckTop { get; set; }
		[DefaultValue(100)]
		public double OpponentOpacity { get; set; }
		[DefaultValue(400)]
		public int OpponentWindowHeight { get; set; }
		[DefaultValue(null)]
		public int? OpponentWindowLeft { get; set; }
		[DefaultValue(false)]
		public bool OpponentWindowOnStart { get; set; }
		[DefaultValue(null)]
		public int? OpponentWindowTop { get; set; }
		[DefaultValue(true)]
		public bool OverlayCardToolTips { get; set; }
		[DefaultValue(100)]
		public double OverlayOpacity { get; set; }
		[DefaultValue(100)]
		public double OverlayOpponentScaling { get; set; }
		[DefaultValue(100)]
		public double OverlayPlayerScaling { get; set; }
		[DefaultValue(false)]
		public bool OverlaySecretToolTipsOnly { get; set; }
		[DefaultValue(false)]
		public bool OwnsGoldenFeugen { get; set; }
		[DefaultValue(false)]
		public bool OwnsGoldenStalagg { get; set; }
		[DefaultValue(new[] { "Win Rate", "Cards", "Draw Chances", "Card Counter" })]
		public string[] PanelOrderOpponent { get; set; }
		[DefaultValue(new[] { "Deck Title", "Wins", "Cards", "Draw Chances", "Card Counter" })]
		public string[] PanelOrderPlayer { get; set; }
		[DefaultValue(65)]
		public double PlayerDeckHeight { get; set; }
		[DefaultValue(99.5)]
		public double PlayerDeckLeft { get; set; }
		[DefaultValue(17)]
		public double PlayerDeckTop { get; set; }
		[DefaultValue(100)]
		public double PlayerOpacity { get; set; }
		[DefaultValue(400)]
		public int PlayerWindowHeight { get; set; }
		[DefaultValue(null)]
		public int? PlayerWindowLeft { get; set; }
		[DefaultValue(false)]
		public bool PlayerWindowOnStart { get; set; }
		[DefaultValue(null)]
		public int? PlayerWindowTop { get; set; }
		[DefaultValue(true)]
		public bool PrioritizeGolden { get; set; }
		[DefaultValue(true)]
		public bool RecordArena { get; set; }
		[DefaultValue(true)]
		public bool RecordCasual { get; set; }
		[DefaultValue(true)]
		public bool RecordFriendly { get; set; }
		[DefaultValue(false)]
		public bool RecordOther { get; set; }
		[DefaultValue(false)]
		public bool RecordPractice { get; set; }
		[DefaultValue(true)]
		public bool RecordRanked { get; set; }
		[DefaultValue(false)]
		public bool RemoveCardsFromDeck { get; set; }
		[DefaultValue(true)]
		public bool SaveInAppData { get; set; }
		[DefaultValue(35)]
		public double SecretsHeight { get; set; }
		[DefaultValue(15)]
		public double SecretsLeft { get; set; }
		[DefaultValue(5)]
		public double SecretsTop { get; set; }
		[DefaultValue("Name")]
		public string SelectedDeckSorting { get; set; }
		[DefaultValue("enUS")]
		public string SelectedLanguage { get; set; }
		[DefaultValue(Game.GameMode.All)]
		public Game.GameMode SelectedStatsFilterGameMode { get; set; }
		[DefaultValue("All Time")]
		public string SelectedStatsFilterTime { get; set; }
		[DefaultValue("Theme")]
		public string SelectedWindowBackground { get; set; }
		[DefaultValue(false)]
		public bool ShowAllDecks { get; set; }
		[DefaultValue(false)]
		public bool ShowDeckTitle { get; set; }
		[DefaultValue(false)]
		public bool ShowDeckWins { get; set; }
		[DefaultValue(false)]
		public bool ShowInTaskbar { get; set; }
		[DefaultValue(false)]
		public bool ShowPlayerGet { get; set; }
		[DefaultValue(false)]
		public bool ShowWinRateAgainst { get; set; }
		[DefaultValue(false)]
		public bool StartMinimized { get; set; }
		[DefaultValue(false)]
		public bool StatsClassOverviewIsExpanded { get; set; }
		[DefaultValue(true)]
		public bool StatsDeckOverviewIsExpanded { get; set; }
		[DefaultValue(false)]
		public bool StatsInWindow { get; set; }
		[DefaultValue(672)]
		public int StatsWindowHeight { get; set; }
		[DefaultValue(null)]
		public int? StatsWindowLeft { get; set; }
		[DefaultValue(null)]
		public int? StatsWindowTop { get; set; }
		[DefaultValue(510)]
		public int StatsWindowWidth { get; set; }
		[DefaultValue(true)]
		public bool TagDecksOnImport { get; set; }
		[DefaultValue(Operation.Or)]
		public Operation TagOperation { get; set; }
		[DefaultValue("")]
		public string ThemeName { get; set; }
		[DefaultValue(75)]
		public double TimerLeft { get; set; }
		[DefaultValue(130)]
		public int TimerWindowHeight { get; set; }
		[DefaultValue(null)]
		public int? TimerWindowLeft { get; set; }
		[DefaultValue(false)]
		public bool TimerWindowOnStartup { get; set; }
		[DefaultValue(null)]
		public int? TimerWindowTop { get; set; }
		[DefaultValue(false)]
		public bool TimerWindowTopmost { get; set; }
		[DefaultValue(false)]
		public bool TimerWindowTopmostIfHsForeground { get; set; }
		[DefaultValue(150)]
		public int TimerWindowWidth { get; set; }
		[DefaultValue(80)]
		public double TimersHorizontalPosition { get; set; }
		[DefaultValue(0)]
		public double TimersHorizontalSpacing { get; set; }
		[DefaultValue(43.5)]
		public double TimersVerticalPosition { get; set; }
		[DefaultValue(50)]
		public double TimersVerticalSpacing { get; set; }
		[DefaultValue(true)]
		public bool TrackerCardToolTips { get; set; }
		[DefaultValue(null)]
		public int? TrackerWindowLeft { get; set; }
		[DefaultValue(null)]
		public int? TrackerWindowTop { get; set; }
		[DefaultValue(100)]
		public int UpdateDelay { get; set; }
		[DefaultValue(false)]
		public bool UseFullTextSearch { get; set; }
		[DefaultValue(true)]
		public bool UseSameScaling { get; set; }
		[DefaultValue(false)]
		public bool VisibleOverlay { get; set; }
		[DefaultValue(true)]
		public bool WindowCardToolTips { get; set; }
		[DefaultValue(620)]
		public int WindowHeight { get; set; }
		[DefaultValue(550)]
		public int WindowWidth { get; set; }
		[DefaultValue("#696969")]
		public string WindowsBackgroundHex { get; set; }
		[DefaultValue(false)]
		public bool WindowsTopmost { get; set; }
		[DefaultValue(false)]
		public bool WindowsTopmostIfHsForeground { get; set; }
		[DefaultValue(true)]
		private string _currentLogFile { get; set; }

		#endregion

		#region Properties

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

		private Config() { }

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

		public static string Load()
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
					Instance.SaveInAppData = false;
			}
			catch(Exception e)
			{
				MessageBox.Show(
					e.Message + "\n\n" + e.InnerException +
					"\n\n If you don't know how to fix this, please delete " + Instance.ConfigPath,
					"Error loading config.xml");
				Application.Current.Shutdown();
			}

			var configPath = Instance.ConfigPath;

			if(!foundConfig)
			{
				if(Instance.HomeDir != string.Empty)
					Directory.CreateDirectory(Instance.HomeDir);
				using(var sr = new StreamWriter(Instance.ConfigPath, false))
					sr.WriteLine("<Config></Config>");
			}
			else if(Instance.SaveInAppData) //check if config needs to be moved
			{
				if(File.Exists("config.xml"))
				{
					Directory.CreateDirectory(Instance.HomeDir);
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

			return configPath;
		}

		public void ResetAll()
		{
			// Use the DefaultValue property of each property to actually set it, via reflection.
			foreach(PropertyDescriptor prop in TypeDescriptor.GetProperties(this))
			{
				var attr = (DefaultValueAttribute)prop.Attributes[typeof(DefaultValueAttribute)];
				if(attr != null)
				{
					prop.SetValue(this, attr.Value);
				}
			}
		}

		public void Reset(string PropertyName)
		{
			//TODO: Upgrade to use LINQ and not the property's name!!
			var property = this.GetType().GetProperty(PropertyName);
			var attribute = property.CustomAttributes.OfType<DefaultValueAttribute>().First();
			property.SetValue(this, attribute.Value);
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