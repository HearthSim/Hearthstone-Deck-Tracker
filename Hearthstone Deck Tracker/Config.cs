#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class Config
	{
		#region Settings

		private static Config _config;

		public readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
		                                     + @"\HearthstoneDeckTracker";

		[DefaultValue("Blue")]
		public string AccentName = "Blue";

		[DefaultValue("00000000-0000-0000-0000-000000000000")]
		public string ActiveDeckIdString = Guid.Empty.ToString();

		[DefaultValue(true)]
		public bool AdditionalOverlayTooltips = true;

		[DefaultValue(true)]
		public bool AdvancedWindowSearch = true;

		[DefaultValue(true)]
		public bool AlwaysOverwriteLogConfig = true;

		[DefaultValue(false)]
		public bool AlwaysShowGoldProgress = false;

		[DefaultValue(true)]
		public bool AskBeforeDiscardingGame = true;

		[DefaultValue(true)]
		public bool AutoClearDeck = true;

		[DefaultValue(true)]
		public bool AutoDeckDetection = true;

		[DefaultValue(true)]
		public bool AutoGrayoutSecrets = true;

		[DefaultValue(false)]
		public bool AutoSaveOnImport = false;

		[DefaultValue(false)]
		public bool AutoUseDeck = false;

		[DefaultValue(true)]
        public bool DeckPickerCaps = true;

		[DefaultValue(true)]
		public bool AutoSelectDetectedDeck = true;

		[DefaultValue("Arena {Date dd-MM hh:mm}")]
		public string ArenaDeckNameTemplate = "Arena {Date dd-MM hh:mm}";

		[DefaultValue(false)]
		public bool BringHsToForeground = false;

		[DefaultValue(false)]
		public bool CardSortingClassFirst = false;

		[DefaultValue(false)]
		public bool CheckForBetaUpdates = false;

		[DefaultValue(true)]
		public bool CheckForUpdates = true;

		[DefaultValue(IconStyle.Round)]
		public IconStyle ClassIconStyle = IconStyle.Round;

		[DefaultValue(true)]
		public bool ClearLogFileAfterGame = true;

		[DefaultValue(false)]
		public bool CloseWithHearthstone = false;

		[DefaultValue(new string[0])]
		public string[] ConstructedImportingIgnoreCachedIds = new string[0];

		[DefaultValue("")]
		public string CreatedByVersion = "";

		[DefaultValue(null)]
		public DateTime? CustomDisplayedTimeFrame = null;

		[DefaultValue(-1)]
		public int CustomHeight = -1;

		[DefaultValue(-1)]
		public int CustomWidth = -1;

		[DefaultValue(".")]
		public string DataDirPath = ".";

		[DefaultValue(false)]
		[XmlIgnore]
		public bool Debug = false;

		[DefaultValue(50)]
		public int DeckExportDelay = 60;

		[DefaultValue(DeckLayout.Layout1)]
		public DeckLayout DeckPickerItemLayout = DeckLayout.Layout1;

		[DefaultValue(false)]
		public bool DeckImportAutoDetectCardCount = false;

		[DefaultValue(false)]
		public bool DiscardGameIfIncorrectDeck = false;

		[DefaultValue(false)]
		public bool DiscardZeroTurnGame = false;

		[DefaultValue(true)]
		public bool DisplayNetDeckAd = true;

		[DefaultValue(GameMode.All)]
		public GameMode DisplayedMode = GameMode.All;

		[DefaultValue(DisplayedStats.Selected)]
		public DisplayedStats DisplayedStats = DisplayedStats.Selected;

		[DefaultValue(DisplayedTimeFrame.AllTime)]
		public DisplayedTimeFrame DisplayedTimeFrame = DisplayedTimeFrame.AllTime;

		[DefaultValue(true)]
		public bool EnterToSaveNote = true;

		[DefaultValue(0.06)]
		public double ExportAllButtonX = 0.06;

		[DefaultValue(0.915)]
		public double ExportAllButtonY = 0.915;

		[DefaultValue(0.118)]
		public double ExportZeroButtonX = 0.118;

		[DefaultValue(0.917)]
		public double ExportZeroButtonY = 0.917;

		[DefaultValue(0.108)]
		public double ExportZeroSquareX = 0.108;

		[DefaultValue(0.907)]
		public double ExportZeroSquareY = 0.907;

		[DefaultValue(0.049)]
		public double ExportSetsButtonX = 0.049;

		[DefaultValue(0.917)]
		public double ExportSetsButtonY = 0.917;

		[DefaultValue(0.067)]
		public double ExportAllSetsButtonX = 0.067;

		[DefaultValue(0.607)]
		public double ExportAllSetsButtonY = 0.607;

		[DefaultValue(0.04)]
		public double ExportCard1X = 0.04;

		[DefaultValue(0.2)]
		public double ExportCard2X = 0.2;

		[DefaultValue(0.168)]
		public double ExportCardsY = 0.168;

		[DefaultValue(0.185)]
		public double ExportClearCheckYFixed = 0.185;

		[DefaultValue(0.83)]
		public double ExportClearX = 0.83;

		[DefaultValue(0.13)]
		public double ExportClearY = 0.13;

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

		[DefaultValue(1)]
		public int ExportStartDelay = 1;

		[DefaultValue(false)]
		public bool ExtraFeatures = false;

		[DefaultValue(false)]
		public bool FixedDuplicateMatches = false;

		[DefaultValue(true)]
		public bool FlashHsOnTurnStart = true;

		[DefaultValue(false)]
		public bool ForceMouseHook = false;

		[DefaultValue(0.075)]
		public double GoldProgessX = 0.76;

		[DefaultValue(0.075)]
		public double GoldProgessY = 0.93;

		[DefaultValue(new[] {0, 0, 0})]
		//move this to some data file
		public int[] GoldProgress = {0, 0, 0};

		//move this to some data file
		public DateTime[] GoldProgressLastReset = {DateTime.MinValue, DateTime.MinValue, DateTime.MinValue};

		[DefaultValue(new[] {0, 0, 0})]
		//move this to some data file
		public int[] GoldProgressTotal = {0, 0, 0};

		[DefaultValue(null)]
		public bool? HearthStatsAutoDeleteDecks = null;

		[DefaultValue(null)]
		public bool? HearthStatsAutoDeleteMatches = null;

		[DefaultValue(false)]
		public bool HearthStatsAutoSyncInBackground = false;

		[DefaultValue(true)]
		public bool HearthStatsAutoUploadNewDecks = true;

		[DefaultValue(true)]
		public bool HearthStatsAutoUploadNewGames = true;

		[DefaultValue(false)]
		public bool HearthStatsSyncOnStart = false;

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
		public bool HideOpponentFatigueCount = false;

		[DefaultValue(false)]
		public bool HideOverlay = false;

		[DefaultValue(false)]
		public bool HideOverlayInSpectator = false;

		[DefaultValue(false)]
		public bool HidePlayerCardCount = false;

		[DefaultValue(false)]
		public bool HidePlayerCards = false;

		[DefaultValue(false)]
		public bool HidePlayerFatigueCount = false;

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

		[DefaultValue(-1)]
		public int IgnoreNewsId = -1;

		[DefaultValue(true)]
		public bool KeepDecksVisible = true;

		[DefaultValue(true)]
		public bool KeepStatsWhenDeletingDeck = true;

		[DefaultValue("None")]
		public string KeyPressOnGameEnd = "None";

		[DefaultValue("None")]
		public string KeyPressOnGameStart = "None";

		[Obsolete]
		[DefaultValue("")]
		public string LastDeck = "";

		[DefaultValue(0L)]
		public long LastHearthStatsDecksSync = 0L;

		[DefaultValue(0L)]
		public long LastHearthStatsGamesSync = 0L;

		[DefaultValue(0)]
		public int LogLevel = 0;

		[DefaultValue(StatType.Mana)]
		public StatType ManaCurveFilter = StatType.Mana;

		[DefaultValue(true)]
		public bool ManaCurveMyDecks = true;

		[DefaultValue(false)]
		public bool MinimizeToTray = false;

		[DefaultValue(null)]
		public bool? NetDeckClipboardCheck = null;

		[DefaultValue(false)]
		public bool NoteDialogDelayed = false;

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
		public bool OverlayCardMarkToolTips = true;

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

		[DefaultValue(new[] {"Win Rate", "Cards", "Card Counter", "Draw Chances", "Fatigue Counter" })]
		public string[] PanelOrderOpponent = {"Win Rate", "Cards", "Card Counter", "Draw Chances", "Fatigue Counter" };

		[DefaultValue(new[] {"Deck Title", "Wins", "Cards", "Card Counter", "Draw Chances", "Fatigue Counter" })]
		public string[] PanelOrderPlayer = {"Deck Title", "Wins", "Cards", "Card Counter", "Draw Chances", "Fatigue Counter" };

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
		public bool RarityCardFrames = true;

		[DefaultValue(true)]
		public bool RecordArena = true;

		[DefaultValue(true)]
		public bool RecordCasual = true;

		[DefaultValue(true)]
		public bool RecordFriendly = true;

		[DefaultValue(false)]
		public bool RecordOther = false;

		[DefaultValue(false)]
		public bool RecordBrawl = false;

		[DefaultValue(false)]
		public bool RecordPractice = false;

		[DefaultValue(true)]
		public bool RecordRanked = true;

		[DefaultValue(true)]
		public bool RecordReplays = true;

		[DefaultValue(false)]
		public bool RecordSpectator = false;

		[DefaultValue(true)]
		public bool RememberHearthStatsLogin = true;

		[DefaultValue(false)]
		public bool RemoveCardsFromDeck = false;

		[DefaultValue(false)]
		public bool RemovedNoteUrls = false;

		[DefaultValue(true)]
		public bool ReplayViewerShowAttack = true;

		[DefaultValue(true)]
		public bool ReplayViewerShowDeath = true;

		[DefaultValue(true)]
		public bool ReplayViewerShowDiscard = true;

		[DefaultValue(true)]
		public bool ReplayViewerShowDraw = true;

		[DefaultValue(true)]
		public bool ReplayViewerShowHeroPower = true;

		[DefaultValue(true)]
		public bool ReplayViewerShowPlay = true;

		[DefaultValue(true)]
		public bool ReplayViewerShowSecret = true;

		[DefaultValue(true)]
		public bool ReplayViewerShowSummon = true;

		[DefaultValue(660)]
		public int ReplayWindowHeight = 660;

		[DefaultValue(null)]
		public int? ReplayWindowLeft = null;

		[DefaultValue(null)]
		public int? ReplayWindowTop = null;

		[DefaultValue(1250)]
		public int ReplayWindowWidth = 1250;

		[DefaultValue(false)]
		public bool ResolvedDeckStatsIds = false;

		[DefaultValue(false)]
		public bool ResolvedDeckStatsIssue = false;

		[DefaultValue(false)]
		public bool ResolvedOpponentNames = false;

		//updating from <= 0.5.1: 
		//SaveConfigInAppData and SaveDataInAppData are set to SaveInAppData AFTER the config isloaded
		//=> Need to be null to avoid creating new config in appdata if config is stored locally.
		[DefaultValue(true)]
		public bool? SaveConfigInAppData = null;

		[DefaultValue(true)]
		public bool? SaveDataInAppData = null;

		[DefaultValue(true)]
		public bool SaveHSLogIntoReplay = true;

		[DefaultValue(true)]
		public bool SaveInAppData = true;

		[DefaultValue(15)]
		public double SecretsLeft = 15;

		[DefaultValue(1)]
		public double SecretsPanelScaling = 1;

		[DefaultValue(5)]
		public double SecretsTop = 5;

		[DefaultValue(ArenaImportingBehaviour.AutoAsk)]
		public ArenaImportingBehaviour? SelectedArenaImportingBehaviour = ArenaImportingBehaviour.AutoAsk;

		[DefaultValue(new[] {HeroClassAll.All})]
		public HeroClassAll[] SelectedDeckPickerClasses = {HeroClassAll.All};

		[DefaultValue("Name")]
		public string SelectedDeckSorting = "Name";

		[DefaultValue("Name")]
		public string SelectedDeckSortingArena = "Name";

		[DefaultValue(DeckType.All)]
		public DeckType SelectedDeckType = DeckType.All;

		[DefaultValue("enUS")]
		public string SelectedLanguage = "enUS";

		[DefaultValue(GameMode.All)]
		public GameMode SelectedStatsFilterGameMode = GameMode.All;

		[DefaultValue(TimeFrame.AllTime)]
		public TimeFrame SelectedStatsFilterTimeFrame = TimeFrame.AllTime;

		[XmlArray(ElementName = "SelectedTags")]
		[XmlArrayItem(ElementName = "Tag")]
		public List<string> SelectedTags = new List<string>();

		[DefaultValue("Theme")]
		public string SelectedWindowBackground = "Theme";

		[Obsolete]
		[DefaultValue(false)]
		public bool ShowAllDecks = false;

		[DefaultValue(true)]
		public bool ShowArenaImportMessage = true;

		[DefaultValue(true)]
		public bool ShowConstructedImportMessage = true;

		[DefaultValue(false)]
		public bool ShowDeckTitle = false;

		[DefaultValue(false)]
		public bool ShowDeckWins = false;

		[DefaultValue(true)]
		public bool ShowExportingDialog = true;

		[DefaultValue("c7b1c7904951f7a")]
		public string ImgurClientId = "c7b1c7904951f7a";

		[DefaultValue(false)]
		public bool ShowInTaskbar = false;

		[DefaultValue(true)]
		public bool ShowLoginDialog = true;

		[DefaultValue(false)]
		public bool ShowLogTab = false;

		[DefaultValue(false)]
		public bool ShowNoteDialogAfterGame = false;

		[DefaultValue(false)]
		public bool ShowPlayerGet = false;

		[DefaultValue(false)]
		public bool ShowWinRateAgainst = false;

		[DefaultValue(true)]
		public bool SortDecksByClass = true;

		[DefaultValue(false)]
		public bool SortDecksByClassArena = false;

		[DefaultValue(false)]
		public bool StartMinimized = false;

		[DefaultValue(false)]
		public bool StartWithWindows = false;

		[DefaultValue(false)]
		public bool StatsClassOverviewIsExpanded = false;

		[DefaultValue(true)]
		public bool StatsDeckOverviewIsExpanded = true;

		[DefaultValue(HeroClassAll.All)]
		public HeroClassAll StatsFilterOpponentHeroClass = HeroClassAll.All;

		[DefaultValue(false)]
		public bool StatsInWindow = false;

		[DefaultValue(false)]
		public bool StatsOverallApplyTagFilters = false;

		[DefaultValue(FilterDeckMode.WithDeck)]
		public FilterDeckMode StatsOverallFilterDeckMode = FilterDeckMode.WithDeck;

		[DefaultValue(HeroClassAll.All)]
		public HeroClassAll StatsOverallFilterPlayerHeroClass = HeroClassAll.All;

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

		[DefaultValue(TagFilerOperation.Or)]
		public TagFilerOperation TagOperation = TagFilerOperation.Or;

		[DefaultValue("BaseLight")]
		public string ThemeName = "BaseLight";

		[DefaultValue(false)]
		public bool TimerAlert = false;

		[DefaultValue(30)]
		public int TimerAlertSeconds = 30;

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

		[DefaultValue(false)]
		public bool UseOldArenaImporting = false;

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

		private GameDetailsConfig _gameDetails;

		public GameDetailsConfig GameDetails
		{
			get { return _gameDetails ?? (_gameDetails = new GameDetailsConfig()); }
			set { _gameDetails = value; }
		}

		[XmlIgnore]
		public Guid ActiveDeckId
		{
			get
			{
				Guid id;
				if(Guid.TryParse(ActiveDeckIdString, out id))
					return id;
				return Guid.Empty;
			}
			set { ActiveDeckIdString = value.ToString(); }
		}

		#endregion

		#region Properties

		[Obsolete]
		public string HomeDir
		{
			get { return Instance.SaveInAppData ? AppDataPath + "/" : string.Empty; }
		}

		public string BackupDir
		{
			get { return Path.Combine(DataDir, "Backups"); }
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
			get { return Instance.SaveDataInAppData == false ? DataDirPath + "\\" : AppDataPath + "\\"; }
		}

		public string ReplayDir
		{
			get { return Path.Combine(DataDir, "Replays"); }
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

		public string HearthStatsFilePath
		{
			get { return Path.Combine(DataDir, "hearthstatsauth"); }
		}

		#endregion

		#region Misc

		private Config()
		{
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
			Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
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
				                e.Message + "\n\n" + e.InnerException + "\n\n If you don't know how to fix this, please delete "
				                + Instance.ConfigPath, "Error loading config.xml");
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
						Logger.WriteLine("Moved config to appdata", "Config");
					}
				}
				else if(File.Exists(Instance.AppDataPath + @"\config.xml"))
				{
					SaveBackup(true); //backup in case the file already exists
					File.Move(Instance.AppDataPath + @"\config.xml", Instance.ConfigPath);
					Logger.WriteLine("Moved config to local", "Config");
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
		}

		public void Reset(string name)
		{
			var proper = GetType().GetFields().First(x => x.Name == name);
			var attr = (DefaultValueAttribute)proper.GetCustomAttributes(typeof(DefaultValueAttribute), false).First();
			proper.SetValue(this, attr.Value);
		}

		[AttributeUsage(AttributeTargets.All, Inherited = false)]
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