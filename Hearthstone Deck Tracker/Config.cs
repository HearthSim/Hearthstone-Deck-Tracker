#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public class Config
	{
		#region Settings

		private static Config _config;

		public static readonly string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
		                                            + @"\HearthstoneDeckTracker";

		[DefaultValue("Blue")]
		public string AccentName = "Blue";

		[DefaultValue(MetroTheme.BaseLight)]
		public MetroTheme AppTheme = MetroTheme.BaseLight;

		[DefaultValue("00000000-0000-0000-0000-000000000000")]
		public string ActiveDeckIdString = Guid.Empty.ToString();

		[DefaultValue(true)]
		public bool AdditionalOverlayTooltips = true;

		[DefaultValue(false)]
		public bool AdvancedOptions = false;

		[DefaultValue(true)]
		public bool AlwaysOverwriteLogConfig = true;

		[DefaultValue(false)]
		public bool AlternativeScreenCapture = false;

		[DefaultValue(true)]
		public bool ArenaRewardDialog = true;

		[DefaultValue(false)]
		public bool AlwaysShowGoldProgress = false;

		[DefaultValue(true)]
		public bool ArenaSummaryChartsExpanded = true;

		[DefaultValue(DisplayedTimeFrame.AllTime)]
		public DisplayedTimeFrame ArenaStatsTimeFrameFilter = DisplayedTimeFrame.AllTime;

		[DefaultValue(null)]
		public DateTime? ArenaStatsTimeFrameCustomStart = null;

		[DefaultValue(null)]
		public DateTime? ArenaStatsTimeFrameCustomEnd = null;

		[DefaultValue(RegionAll.ALL)]
		public RegionAll ArenaStatsRegionFilter = RegionAll.ALL;

		[DefaultValue(true)]
		public bool ArenaStatsShowLegends = true;

		[DefaultValue(true)]
		public bool ArenaStatsTextColoring = true;

		[DefaultValue(true)]
		public bool AskBeforeDiscardingGame = true;

		[DefaultValue(true)]
		public bool ArenaStatsIncludeArchived = true;

		[DefaultValue(1)]
		public int ArenaStatsCustomSeasonMin = 1;

		[DefaultValue(null)]
		public int? ArenaStatsCustomSeasonMax = null;

		[DefaultValue(71.67)]
		public double AttackIconPlayerVerticalPosition = 71.67;

		[DefaultValue(67.5)]
		public double AttackIconPlayerHorizontalPosition = 67.5;

		[DefaultValue(19.91)]
		public double AttackIconOpponentVerticalPosition = 19.91;

		[DefaultValue(67.5)]
		public double AttackIconOpponentHorizontalPosition = 67.5;

		[DefaultValue(false)]
		public bool AutoArchiveArenaDecks = false;

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

		[DefaultValue("Brawl {Date dd-MM hh:mm}")]
		public string BrawlDeckNameTemplate = "Brawl {Date dd-MM hh:mm}";

		[DefaultValue(false)]
		public bool BringHsToForeground = false;

		[DefaultValue(false)]
		public bool CardSortingClassFirst = false;

		[DefaultValue(false)]
		public bool CheckForBetaUpdates = false;

		[DefaultValue(true)]
		public bool CheckForUpdates = true;

		[DefaultValue(ClassColorScheme.Classic)]
		public ClassColorScheme ClassColorScheme = ClassColorScheme.Classic;

		[DefaultValue(IconStyle.Round)]
		public IconStyle ClassIconStyle = IconStyle.Round;

		[DefaultValue("dark")]
		public string CardBarTheme = "dark";

		[DefaultValue(true)]
		public bool CardDbIncludeWildOnlyCards = true;

		[DefaultValue(true)]
		public bool ConstructedAutoImportNew = true;

		[DefaultValue(true)]
		public bool ConstructedAutoUpdate = true;

		[DefaultValue(false)]
		public bool ConstructedStatsAsPercent = false;

		[DefaultValue(false)]
		public bool ConstructedStatsApplyTagFilters = false;

		[DefaultValue(true)]
		public bool ConstructedSummaryChartsExpanded = true;

		[DefaultValue(GameMode.All)]
		public GameMode ConstructedStatsModeFilter = GameMode.All;

		[DefaultValue(DisplayedTimeFrame.CurrentSeason)]
		public DisplayedTimeFrame ConstructedStatsTimeFrameFilter = DisplayedTimeFrame.CurrentSeason;

		[DefaultValue(HeroClassStatsFilter.All)]
		public HeroClassStatsFilter ConstructedStatsClassFilter = HeroClassStatsFilter.All;

		[DefaultValue(HeroClassStatsFilter.All)]
		public HeroClassStatsFilter ConstructedStatsOpponentClassFilter = HeroClassStatsFilter.All;

		[DefaultValue(RegionAll.ALL)]
		public RegionAll ConstructedStatsRegionFilter = RegionAll.ALL;

		[DefaultValue(Format.All)]
		public Format ConstructedStatsFormatFilter = Format.All;

		[DefaultValue(AllYesNo.All)]
		public AllYesNo ConstructedStatsCoinFilter = AllYesNo.All;

		[DefaultValue(GameResultAll.All)]
		public GameResultAll ConstructedStatsResultFilter = GameResultAll.All;

		[DefaultValue(null)]
		public DateTime? ConstructedStatsTimeFrameCustomStart = null;

		[DefaultValue(null)]
		public DateTime? ConstructedStatsTimeFrameCustomEnd = null;

		[DefaultValue(true)]
		public bool ConstructedStatsIncludeArchived = true;

		[DefaultValue("L1")]
		public string ConstructedStatsRankFilterMin = "L1";

		[DefaultValue("25")]
		public string ConstructedStatsRankFilterMax = "25";

		[DefaultValue(1)]
		public int ConstructedStatsCustomSeasonMin = 1;

		[DefaultValue(null)]
		public int? ConstructedStatsCustomSeasonMax = null;

		[DefaultValue(0)]
		public int ConstructedStatsTurnsFilterMin = 0;

		[DefaultValue(99)]
		public int ConstructedStatsTurnsFilterMax = 99;

		[DefaultValue("")]
		public string ConstructedStatsOpponentNameFilter = "";

		[DefaultValue("")]
		public string ConstructedStatsNoteFilter = "";

		[DefaultValue(false)]
		public bool ConstructedStatsActiveDeckOnly = false;

		[DefaultValue(true)]
		public bool ClearLogFileAfterGame = true;

		[DefaultValue(false)]
		public bool CloseWithHearthstone = false;

		[DefaultValue(false)]
		public bool StartHearthstoneWithHDT = false;

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
		public bool Debug = false;

		[DefaultValue(50)]
		public int DeckExportDelay = 60;

		[DefaultValue(DeckLayout.Layout1)]
		public DeckLayout DeckPickerItemLayout = DeckLayout.Layout1;

		[DefaultValue(true)]
		public bool DeckPickerWildIncludesStandard = true;

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

		[DefaultValue(DisplayedStats.All)]
		public DisplayedStats DisplayedStats = DisplayedStats.All;

		[DefaultValue(DisplayedTimeFrame.AllTime)]
		public DisplayedTimeFrame DisplayedTimeFrame = DisplayedTimeFrame.AllTime;

		[DefaultValue(true)]
		public bool EnterToSaveNote = true;

		[DefaultValue(0.06)]
		public double ExportAllButtonX = 0.06;

		[DefaultValue(0.915)]
		public double ExportAllButtonY = 0.915;

		[DefaultValue(false)]
		public bool ExportAddDeckVersionToName = false;

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

		[DefaultValue(0.45)]
		public double ExportStandardSetButtonY = 0.45;

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

		[DefaultValue(false)]
		public bool ExportForceClear = false;

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

		[DefaultValue(true)]
		public bool EnableExportAutoFilter = true;

		[DefaultValue(false)]
		public bool ExtraFeatures = false;

		[DefaultValue(true)]
		public bool ExtraFeaturesFriendslist = true;

		[DefaultValue(false)]
		public bool ExtraFeaturesSecrets = false;

		[DefaultValue(true)]
		public bool FlashHsOnTurnStart = true;

		[DefaultValue(false)]
		public bool ForceMouseHook = false;

		[DefaultValue(false)]
		public bool GameResultNotificationsUnexpectedOnly = false;

		[DefaultValue(0.075)]
		public double GoldProgessX = 0.76;

		[DefaultValue(0.075)]
		public double GoldProgessY = 0.93;

		[DefaultValue(new[] {0, 0, 0, 0, 0})]
		//move this to some data file
		public int[] GoldProgress = {0, 0, 0, 0, 0};

		//move this to some data file
		public DateTime[] GoldProgressLastReset =
		{
			DateTime.MinValue,
			DateTime.MinValue,
			DateTime.MinValue,
			DateTime.MinValue,
			DateTime.MinValue
		};

		[DefaultValue(new[] {0, 0, 0, 0, 0})]
		//move this to some data file
		public int[] GoldProgressTotal = {0, 0, 0, 0, 0};

		[DefaultValue(true)]
		public bool GoogleAnalytics = true;

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

		[DefaultValue(@"C:\Program Files (x86)\Hearthstone")]
		public string HearthstoneDirectory = @"C:\Program Files (x86)\Hearthstone";

		[DefaultValue("Logs")]
		public string HearthstoneLogsDirectoryName = "Logs";

		[DefaultValue("Hearthstone")]
		public string HearthstoneWindowName = "Hearthstone";

		[DefaultValue(HeroClassStatsFilter.All)]
		public HeroClassStatsFilter ArenaStatsClassFilter = HeroClassStatsFilter.All;

		[DefaultValue(false)]
		public bool HideDecksInOverlay = false;

		[DefaultValue(false)]
		public bool HideDrawChances = false;

		[DefaultValue(false)]
		public bool HideInBackground = false;

		[DefaultValue(false)]
		public bool HideInMenu = false;

		[DefaultValue(true)]
		public bool HideOpponentAttackIcon = true;

		[DefaultValue(DisplayMode.Auto)]
		public DisplayMode OpponentCthunCounter = DisplayMode.Auto;

		[DefaultValue(DisplayMode.Never)]
		public DisplayMode OpponentSpellsCounter = DisplayMode.Never;

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

		[DefaultValue(true)]
		public bool HidePlayerAttackIcon = true;

		[DefaultValue(DisplayMode.Auto)]
		public DisplayMode PlayerCthunCounter = DisplayMode.Auto;

		[DefaultValue(DisplayMode.Auto)]
		public DisplayMode PlayerSpellsCounter = DisplayMode.Auto;

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

		[Obsolete]
		[DefaultValue("")]
		public string LastDeck = "";

		[DefaultValue(0L)]
		public long LastHearthStatsDecksSync = 0L;

		[DefaultValue(0L)]
		public long LastHearthStatsGamesSync = 0L;

		[DefaultValue(LastPlayedDateFormat.DayMonthYear)]
		public LastPlayedDateFormat LastPlayedDateFormat = LastPlayedDateFormat.DayMonthYear;

		[DefaultValue(false)]
		public bool LogConfigConsolePrinting = false;

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

		[DefaultValue(null)]
		public bool? NonLatinUseDefaultFont = null;

		[DefaultValue(false)]
		public bool NoteDialogDelayed = false;

		[DefaultValue(4)]
		public int NotificationFadeOutDelay = 4;

		[DefaultValue(0)]
		public int OffsetX = 0;

		[DefaultValue(0)]
		public int OffsetY = 0;

		[DefaultValue(72)]
		public double OpponentDeckHeight = 72;

		[DefaultValue(0.5)]
		public double OpponentDeckLeft = 0.5;

		[DefaultValue(12.5)]
		public double OpponentDeckTop = 12.5;

		[DefaultValue(true)]
		public bool OpponentIncludeCreated = true;

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
		public bool OverlayCardAnimations = true;

		[DefaultValue(true)]
		public bool OverlayCardAnimationsOpacity = true;

		[DefaultValue(true)]
		public bool OverlayCardMarkToolTips = true;

		[DefaultValue(true)]
		public bool OverlayCardToolTips = true;

		[DefaultValue(250)]
		public int OverlayMouseOverTriggerDelay = 250;

		[DefaultValue(100)]
		public double OverlayOpacity = 100;

		[DefaultValue(100)]
		public double OverlayOpponentScaling = 100;

		[DefaultValue(100)]
		public double OverlayPlayerScaling = 100;

		[DefaultValue(false)]
		public bool OverlayCenterPlayerStackPanel = false;

		[DefaultValue(false)]
		public bool OverlayCenterOpponentStackPanel = false;

		[DefaultValue(false)]
		public bool OverlaySecretToolTipsOnly = false;

		[DefaultValue(new[] {"Win Rate", "Cards", "Card Counter", "Draw Chances", "Fatigue Counter"})]
		public string[] PanelOrderOpponent = {"Win Rate", "Cards", "Card Counter", "Draw Chances", "Fatigue Counter"};

		[DefaultValue(new[] {"Deck Title", "Wins", "Cards", "Card Counter", "Draw Chances", "Fatigue Counter"})]
		public string[] PanelOrderPlayer = {"Deck Title", "Wins", "Cards", "Card Counter", "Draw Chances", "Fatigue Counter"};

		[DefaultValue(88)]
		public double PlayerDeckHeight = 88;

		[DefaultValue(99.5)]
		public double PlayerDeckLeft = 99.5;

		[DefaultValue(2)]
		public double PlayerDeckTop = 2;

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

		[DefaultValue(false)]
		public bool RarityCardFrames = false;

		[DefaultValue(false)]
		public bool RarityCardGems = false;

		[DefaultValue(true)]
		public bool RecordArena = true;

		[DefaultValue(true)]
		public bool RecordCasual = true;

		[DefaultValue(true)]
		public bool RecordFriendly = true;

		[DefaultValue(false)]
		public bool RecordOther = false;

		[DefaultValue(true)]
		public bool RecordBrawl = true;

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
		public bool ReselectLastDeckUsed = false;

		//updating from <= 0.5.1: 
		//SaveConfigInAppData and SaveDataInAppData are set to SaveInAppData AFTER the config isloaded
		//=> Need to be null to avoid creating new config in appdata if config is stored locally.
		[DefaultValue(true)]
		public bool? SaveConfigInAppData;

		[DefaultValue(true)]
		public bool? SaveDataInAppData = null;

		[DefaultValue(true)]
		public bool SaveInAppData = true;

		[DefaultValue(15)]
		public double SecretsLeft = 15;

		[DefaultValue(100)]
		public double SecretsOpacity = 100;

		[DefaultValue(1)]
		public double SecretsPanelScaling = 1;

		[DefaultValue(5)]
		public double SecretsTop = 5;

		[DefaultValue(ArenaImportingBehaviour.AutoImportSave)]
		public ArenaImportingBehaviour? SelectedArenaImportingBehaviour = ArenaImportingBehaviour.AutoImportSave;

		[DefaultValue(new[] {HeroClassAll.All})]
		public HeroClassAll[] SelectedDeckPickerClasses = {HeroClassAll.All};

		[DefaultValue("Name")]
		public string SelectedDeckSorting = "Name";

		[DefaultValue("Name")]
		public string SelectedDeckSortingArena = "Name";

		[DefaultValue(DeckType.All)]
		public DeckType SelectedDeckPickerDeckType = DeckType.All;

		[DefaultValue("enUS")]
		public string SelectedLanguage = "enUS";

		[XmlArray(ElementName = "AlternativeLanguages")]
		[XmlArrayItem(ElementName = "Language")]
		public List<string> AlternativeLanguages = new List<string>();

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

		[DefaultValue(false)]
		public bool ShowBatteryLife = false;

		[DefaultValue(false)]
		public bool ShowBatteryLifePercent = false;

		[DefaultValue(false)]
		public bool ShowCapturableOverlay = false;

		[DefaultValue(false)]
		public bool ShowDeckTitle = false;

		[DefaultValue(false)]
		public bool ShowDeckWins = false;

		[DefaultValue(true)]
		public bool ShowExportingDialog = true;

		[DefaultValue(true)]
		public bool ShowFlavorText = true;

		[DefaultValue(true)]
		public bool ShowGameResultNotifications = true;

		[DefaultValue("c7b1c7904951f7a")]
		public string ImgurClientId = "c7b1c7904951f7a";

		[DefaultValue(false)]
		public bool ShowInTaskbar = false;

		[DefaultValue(false)]
		public bool ShowLastPlayedDateOnDeck = false;
		
		[DefaultValue(true)]
		public bool ShowLoginDialog = true;

		[DefaultValue(true)]
		public bool ShowSplashScreen = true;

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
		public bool SpectatorUseNoDeck = false;

		[DefaultValue(false)]
		public bool StartMinimized = false;

		[DefaultValue(false)]
		public bool StartWithWindows = false;

		[DefaultValue(true)]
		public bool StatsAutoRefresh = true;

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

		[DefaultValue("#FF00FF")]
		public string StreamingOverlayBackground = "#FF00FF";

		[DefaultValue(true)]
		public bool TagDecksOnImport = true;

		[DefaultValue(TagFilerOperation.Or)]
		public TagFilerOperation TagOperation = TagFilerOperation.Or;

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

		[DefaultValue(72)]
		public double TimersHorizontalPosition = 72;

		[DefaultValue(48)]
		public double TimersHorizontalSpacing = 48;

		[DefaultValue(44.5)]
		public double TimersVerticalPosition = 44.5;

		[DefaultValue(42)]
		public double TimersVerticalSpacing = 42;

		[DefaultValue(true)]
		public bool TrackerCardToolTips = true;

		[DefaultValue(null)]
		public int? TrackerWindowLeft = null;

		[DefaultValue(null)]
		public int? TrackerWindowTop = null;

		[DefaultValue(false)]
		public bool UseAnyUnityWindow = false;

		[DefaultValue(true)]
		public bool UseAnimations = true;

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

		[DefaultValue(68.5)]
		public double WotogIconsPlayerVertical = 68.5;

		[DefaultValue(86.8)]
		public double WotogIconsPlayerHorizontal = 86.8;

		[DefaultValue(18.5)]
		public double WotogIconsOpponentVertical = 18.5;

		[DefaultValue(86.5)]
		public double WotogIconsOpponentHorizontal = 86.5;

		#region deprecated

		[DefaultValue("BaseLight")]
		public string ThemeName = "BaseLight";

		#endregion

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
				return Guid.TryParse(ActiveDeckIdString, out id) ? id : Guid.Empty;
			}
			set { ActiveDeckIdString = value.ToString(); }
		}

		#endregion

		#region Properties

		[Obsolete]
		public string HomeDir => Instance.SaveInAppData ? AppDataPath + "/" : string.Empty;

		public string BackupDir => Path.Combine(DataDir, "Backups");

		public string ConfigPath => Instance.ConfigDir + "config.xml";

		public string ConfigDir => Instance.SaveConfigInAppData == false ? string.Empty : AppDataPath + "\\";

		public string DataDir => Instance.SaveDataInAppData == false ? DataDirPath + "\\" : AppDataPath + "\\";

		public string ReplayDir => Path.Combine(DataDir, "Replays");

		public string HearthStatsFilePath => Path.Combine(DataDir, "hearthstatsauth");

		public static Config Instance
		{
			get
			{
				if(_config != null)
					return _config;
				_config = new Config();
				_config.ResetAll();
				_config.SelectedTags = new List<string>();
				_config.GameDetails = new GameDetailsConfig();
				return _config;
			}
		}

		#endregion

		#region Misc

		private Config()
		{
		}

		public static void Save() => XmlManager<Config>.Save(Instance.ConfigPath, Instance);

		public static void SaveBackup(bool deleteOriginal = false)
		{
			var configPath = Instance.ConfigPath;

			if(!File.Exists(configPath))
				return;

			File.Copy(configPath, configPath + DateTime.Now.ToFileTime());

			if(deleteOriginal)
				File.Delete(configPath);
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
				else if(File.Exists(AppDataPath + @"\config.xml"))
				{
					_config = XmlManager<Config>.Load(AppDataPath + @"\config.xml");
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
				Save();
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
						Log.Info("Moved config to appdata");
					}
				}
				else if(File.Exists(AppDataPath + @"\config.xml"))
				{
					SaveBackup(true); //backup in case the file already exists
					File.Move(AppDataPath + @"\config.xml", Instance.ConfigPath);
					Log.Info("Moved config to local");
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

			public object Value { get; }
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
