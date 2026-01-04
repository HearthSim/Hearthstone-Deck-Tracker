using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Composition;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using Hearthstone_Deck_Tracker.Utility.Exceptions;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Commands;
using HSReplay.Requests;
using HSReplay.Responses;
using Newtonsoft.Json;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;

public class BattlegroundsSessionViewModel : ViewModel
{
	private readonly BattlegroundsDb _db = BattlegroundsDbSingleton.Instance;

	public BattlegroundsSessionViewModel()
	{
		RetryCompStatsCommand = new Command(async () => await RetryCompStats());
	}

	public ObservableCollection<Race> AvailableMinionTypes { get; } = new();
	public ObservableCollection<Race> BannedMinionTypes { get; } = new();
	public ObservableCollection<BattlegroundsGameViewModel> SessionGames { get; } = new();

	public Race BannedMinionType1 => AvailableMinionTypes.FirstOrDefault();

	public void OnGameEnd()
	{
		if(Core.Game.Spectator)
			return;

		var currentRating = Core.Game.CurrentGameStats?.BattlegroundsRatingAfter;
		BgRatingCurrent = $"{currentRating:N0}";

		UpdateLatestGames();
	}

	private readonly SemaphoreSlim _updateCompStatsSemaphore = new SemaphoreSlim(1, 1);

	public ICommand RetryCompStatsCommand { get; }


	public async void Update()
	{
		if(Core.Game.IsChinaModuleActive)
			return;

		if(Core.Game.Spectator)
			await Task.Delay(1500);

		UpdateMinionTypes();

		var firstGame = await UpdateLatestGames();

		var rating = (IsDuos  ? Core.Game.BattlegroundsRatingInfo?.DuosRating : Core.Game.BattlegroundsRatingInfo?.Rating) ?? 0;
		var ratingStart = firstGame?.Rating ?? rating;
		if(rating == 0)
			rating = ratingStart;
		BgRatingStart = $"{ratingStart:N0}";
		BgRatingCurrent = $"{rating:N0}";

		// Update method might be called multiple times.
		// We need to prevent multiple calls to UpdateCompositionStatsIfNeeded to happen at the same time.
		// This also ensures only one API call is made.
		try
		{
			await _updateCompStatsSemaphore.WaitAsync();
			await UpdateCompositionStatsIfNeeded();
		}
		finally
		{
			_updateCompStatsSemaphore.Release();
		}
	}

	private bool IsDuos => Core.Game.IsInMenu
		? BattlegroundsGameMode == SelectedBattlegroundsGameMode.DUOS
		: Core.Game.IsBattlegroundsDuosMatch;

	public void UpdateSectionsVisibilities()
	{
		AvailableMinionTypesSectionVisibility = Config.Instance.ShowSessionRecapMinionsAvailable
			? Visibility.Visible
			: Visibility.Collapsed;

		BannedMinionTypesSectionVisibility = Config.Instance.ShowSessionRecapMinionsBanned
			? Visibility.Visible
			: Visibility.Collapsed;

		BgStartCurrentMMRSectionVisibility = Config.Instance.ShowSessionRecapStartCurrentMMR
			? Visibility.Visible
			: Visibility.Collapsed;

		BgLatestGamesSectionVisibility = Config.Instance.ShowSessionRecapLatestGames
			? Visibility.Visible
			: Visibility.Collapsed;

		Core.Windows.BattlegroundsSessionWindow.UpdateBattlegroundsSessionLayoutHeight();
	}

	public async void UpdateCompositionStatsVisibility()
	{
		if(IsDuos || !Config.Instance.ShowBattlegroundsTier7SessionCompStats
		          || !Config.Instance.EnableBattlegroundsTier7Overlay)
		{
			AvailableCompStatsSectionVisibility = Visibility.Collapsed;
			return;
		}
		var acc = Reflection.Client.GetAccountId();
		if(acc != null)
			await Tier7Trial.Update(acc.Hi, acc.Lo);

		var userOwnsTier7 = HSReplayNetOAuth.AccountData?.IsTier7 ?? false;

		AvailableCompStatsSectionVisibility =
			Tier7Trial.RemainingTrials > 0 || Tier7Trial.IsTrialForCurrentGameActive(Core.Game.MetaData.ServerInfo?.GameHandle) || userOwnsTier7 || CompositionStats != null ? Visibility.Visible
				: Visibility.Collapsed;
	}

	private void UpdateMinionTypes()
	{
		var allRaces = _db.Races.Where(x => x != Race.INVALID && x != Race.ALL).ToList();
		var availableRaces = BattlegroundsUtils.GetAvailableRaces()?.ToList() ?? allRaces;
		var unavailableRaces = allRaces.Where(x => !availableRaces.Contains(x)).ToList();

		var validMinionTypes = unavailableRaces.Count() >= 5 && unavailableRaces.Count() != allRaces.Count();
		if(validMinionTypes)
		{
			AvailableMinionTypes.Clear();
			foreach(var race in availableRaces.OrderBy(t => HearthDbConverter.GetLocalizedRace(t) ?? ""))
			{
				AvailableMinionTypes.Add(race);
			}

			BannedMinionTypes.Clear();
			foreach(var race in unavailableRaces.OrderBy(t => HearthDbConverter.GetLocalizedRace(t) ?? ""))
			{
				BannedMinionTypes.Add(race);
			}

			OnPropertyChanged(nameof(BannedMinionType1));
		}

		if((Core.Game.CurrentMode == Mode.GAMEPLAY || SceneHandler.Scene == Mode.GAMEPLAY) && validMinionTypes)
		{
			MinionTypesBodyVisibility = Visibility.Visible;
			MinionTypesWaitingMsgVisibility = Visibility.Collapsed;
		}
		else
		{
			MinionTypesBodyVisibility = Visibility.Hidden;
			MinionTypesWaitingMsgVisibility = Visibility.Visible;
		}
	}

	private async Task<BattlegroundsCompStats?> GetBattlegroundsCompStats()
	{
		var gameId = Core.Game.MetaData.ServerInfo?.GameHandle;
		var userOwnsTier7 = HSReplayNetOAuth.AccountData?.IsTier7 ?? false;
		var userHasTrials = Tier7Trial.RemainingTrials > 0;

		if(!userOwnsTier7 && !(userHasTrials || Tier7Trial.IsTrialForCurrentGameActive(gameId)))
			return null;

		if(IsDuos)
			return null;

	    if(Core.Game.Spectator)
	        return null;

	    if(!Config.Instance.EnableBattlegroundsTier7Overlay || !Config.Instance.ShowBattlegroundsTier7SessionCompStats)
	        return null;

	    if(Remote.Config.Data?.Tier7?.Disabled ?? false)
	        throw new CompositionStatsException("Tier 7 remotely disabled");

	    var availableRaces = BattlegroundsUtils.GetAvailableRaces();

	    // Retry logic: game state may not be ready immediately
	    if(availableRaces == null)
	    {
		    Log.Info("[Tier7CompStats] Available races not ready, retrying after delay...");
		    await Task.Delay(2000);
		    availableRaces = BattlegroundsUtils.GetAvailableRaces();
		    
		    if(availableRaces == null)
		    {
			    await Task.Delay(3000);
			    availableRaces = BattlegroundsUtils.GetAvailableRaces();
		    }
	    }

	    if(availableRaces == null)
		    throw new CompositionStatsException("Unable to get available races");

	    var compParams = new BattlegroundsCompStatsParams
	    {
		    BattlegroundsRaces = availableRaces.Cast<int>().ToArray(),
		    LanguageCode = Helper.GetCardLanguage(),
	    };

	    // Avoid using a trial when we can't get the api params anyway.
	    if(compParams == null)
			throw new CompositionStatsException("Unable to get API parameters");

	    // Use a trial if we can
	    string? token = null;
	    if(!userOwnsTier7)
	    {
	        var acc = Reflection.Client.GetAccountId();
	        token = acc != null ? await Tier7Trial.ActivateOrContinue(acc.Hi, acc.Lo, gameId) : null;
	        if(!((Core.Game.GameEntity?.GetTag(GameTag.STEP) ?? 0) <= (int)Step.BEGIN_MULLIGAN) && token == null)
		        return null;

	        if(token == null)
	            throw new CompositionStatsException("Unable to get trial token");
	    }

	#if(DEBUG)
	    var json = JsonConvert.SerializeObject(compParams);
	    Log.Debug($"Fetching Battlegrounds Hero Pick stats with parameters={json}...");
	#endif

	    // At this point the user either owns tier7 or has an active trial!

	    BattlegroundsCompStats? compStats;
	    try
	    {
		    compStats = token != null && !userOwnsTier7
			    ?  await ApiWrapper.GetTier7CompStats(token, compParams)
			    : await HSReplayNetOAuth.MakeRequest(c => c.GetTier7CompStats(compParams)
			);
	    }
	    catch
	    {
		    throw new CompositionStatsException("Invalid server response");
	    }

	    if(compStats == null || compStats.Data.FirstPlaceCompsLobbyRaces.Count == 0)
		    throw new CompositionStatsException("Invalid server response");

	    return compStats;
	}

	private void ClearCompositionStats()
	{
		CompositionStats = null;
		CompStatsBodyVisibility = Visibility.Hidden;
		CompStatsWaitingMsgVisibility = Visibility.Visible;
		CompStatsErrorVisibility = Visibility.Hidden;

		Core.Windows.BattlegroundsSessionWindow.UpdateBattlegroundsSessionLayoutHeight();
	}

	private void ShowCompositionStats()
	{
		CompStatsBodyVisibility = Visibility.Visible;
		CompStatsWaitingMsgVisibility = Visibility.Collapsed;
		CompStatsErrorVisibility = Visibility.Hidden;

		Core.Windows.BattlegroundsSessionWindow.UpdateBattlegroundsSessionLayoutHeight();
	}

	private async Task UpdateCompositionStatsIfNeeded()
	{
		if(Core.Game.CurrentMode != Mode.GAMEPLAY || SceneHandler.Scene != Mode.GAMEPLAY)
		{
			ClearCompositionStats();
			return;
		}

		// Ensures data was already fetched and no more API calls are needed
		if(((CompositionStats != null && CompositionStats.Any()) || CompStatsErrorVisibility == Visibility.Visible) &&
		   (Core.Game.CurrentMode == Mode.GAMEPLAY || SceneHandler.Scene == Mode.GAMEPLAY))
		{
			return;
		}

		await TrySetCompStats();
	}

	private async Task TrySetCompStats()
	{
		var statsTask = GetBattlegroundsCompStats();

		BattlegroundsCompStats? battlegroundsCompStats = null;
		try
		{
			battlegroundsCompStats = await statsTask;
		}
		catch(Exception e)
		{
			HandleCompStatsError(e);
			return;
		}

		if(battlegroundsCompStats is BattlegroundsCompStats compStats)
		{
			var firstPlaceComps = compStats.Data?.FirstPlaceCompsLobbyRaces;
			if(firstPlaceComps != null && firstPlaceComps.Count > 0)
			{
				Log.Info($"[Tier7CompStats] Success: received {firstPlaceComps.Count} compositions");
				SetBattlegroundsCompositionStatsViewModel(
					firstPlaceComps
				);
				ShowCompositionStats();
			}
			else
			{
				Log.Warn($"[Tier7CompStats] API returned null or empty compositions. compStats.Data={compStats.Data}, firstPlaceComps.Count={firstPlaceComps?.Count ?? 0}");
			}
		}
		else
		{
			Log.Warn($"[Tier7CompStats] Received null from API");
		}
	}

	public void HideCompStatsOnError()
	{
		if(CompStatsErrorVisibility == Visibility.Visible)
		{
			AvailableCompStatsSectionVisibility = Visibility.Collapsed;
			Core.Windows.BattlegroundsSessionWindow.UpdateBattlegroundsSessionLayoutHeight();
		}
	}

	private void HandleCompStatsError(Exception error)
	{
		Influx.OnGetBattlegroundsCompositionStatsError(error.GetType().Name, error.Message);
		Log.Error($"[Tier7CompStats] error: {error.GetType().Name} - {error.Message}; debug={CompStatsDebugInfo ?? "<none>"}");

		var beforeHeroPicked = (Core.Game.GameEntity?.GetTag(GameTag.STEP) ?? 0) <= (int)Step.BEGIN_MULLIGAN;
		if(!beforeHeroPicked)
		{
			Task.Run(async () =>
			{
				// Ensure update after 20 seconds
				await Task.Delay(20_000);
				HideCompStatsOnError();
			}).Forget();
		}

		CompStatsErrorVisibility = Visibility.Visible;
		CompStatsBodyVisibility = Visibility.Hidden;
		CompStatsWaitingMsgVisibility = Visibility.Hidden;
	}

	private async Task RetryCompStats()
	{
		CompStatsErrorVisibility = Visibility.Hidden;
		CompStatsBodyVisibility = Visibility.Hidden;
		CompStatsWaitingMsgVisibility = Visibility.Visible;

		try
		{
			await _updateCompStatsSemaphore.WaitAsync();
			await TrySetCompStats();
		}
		finally
		{
			_updateCompStatsSemaphore.Release();
		}
	}

	private async Task<GameItem?> UpdateLatestGames()
	{
		var sortedGames = (await Instance.PlayerGames(IsDuos))
			.OrderBy(g => g.StartTime)
			.ToList();
		DeleteOldGames(sortedGames);

		var sessionGames = GetSessionGames(sortedGames);
		var firstGame = sessionGames.FirstOrDefault();

		SessionGames.Clear();
		sessionGames
			.GetRange(Math.Max(sessionGames.Count - 8, 0), Math.Min(8, sessionGames.Count))
			.OrderByDescending(g => g.StartTime)
			.ToList()
			.ForEach(AddOrUpdateGame);

		GridHeaderVisibility = sessionGames.Count > 0
			? Visibility.Visible
			: Visibility.Collapsed;

		GamesEmptyStateVisibility = sessionGames.Count == 0
			? Visibility.Visible
			: Visibility.Collapsed;

		Core.Windows.BattlegroundsSessionWindow.UpdateBattlegroundsSessionLayoutHeight();

		return firstGame;
	}

	private List<GameItem> GetSessionGames(List<GameItem> sortedGames)
	{
		DateTime? sessionStartTime = null;
		DateTime? previousGameEndTime = null;
		var previousGameRatingAfter = 0;

		foreach(var g in sortedGames)
		{
			if(previousGameEndTime != null)
			{
				var gStartTime = DateTime.Parse(g.StartTime);
				var ts = gStartTime - (DateTime)previousGameEndTime;

				var diffMMR = g.Rating - previousGameRatingAfter;
				// Check for MMR reset
				var ratingReset = g.Rating < 500 && diffMMR < -500;

				if(ts.TotalHours >= 2 || ratingReset)
					sessionStartTime = gStartTime;
			}
			previousGameEndTime = DateTime.Parse(g.EndTime);
			previousGameRatingAfter = g.RatingAfter;
		};

		var sessionGames = sessionStartTime == null
			? sortedGames
			: sortedGames.Where(g => DateTime.Parse(g.StartTime) >= sessionStartTime).ToList();

		if(sessionGames.Count > 0)
		{
			var lastGame = sessionGames.LastOrDefault();

			// Check for MMR reset on last game
			var ratingResetAfterLastGame = false;
			if(Core.Game.BattlegroundsRatingInfo?.Rating != null)
			{
				var currentMMR = Core.Game.BattlegroundsRatingInfo?.Rating;
				var sessionLastMMR = lastGame.RatingAfter;
				ratingResetAfterLastGame = currentMMR < 500 && currentMMR - sessionLastMMR < -500;
			}

			var ts = DateTime.Now - DateTime.Parse(lastGame.EndTime);

			if(ts.TotalHours >= 2 || ratingResetAfterLastGame)
				return new List<GameItem>();
		}

		return sessionGames;
	}

	private void DeleteOldGames(List<GameItem> sortedGames)
	{
		sortedGames.ForEach(g =>
		{
			var ts = DateTime.Now - DateTime.Parse(g.StartTime);
			if(g.StartTime != null && ts.TotalDays >= 7)
				Instance.RemoveGame(g.StartTime);
		});
	}

	private void AddOrUpdateGame(GameItem game)
	{
		var existingGame = SessionGames.FirstOrDefault(x => x?.StartTime == game.StartTime);
		if(existingGame == null)
		{
			SessionGames.Add(new BattlegroundsGameViewModel(game));
		}
	}

	public string AvailableMinionTypesHeaderLabel =>
		(_availableMinionTypesSectionVisibility == Visibility.Visible, _bannedMinionTypesSectionVisibility == Visibility.Visible) switch
		{
			(true, false) => LocUtil.Get("Battlegrounds_Session_Header_Label_Minions_Available"),
			(false, true) => LocUtil.Get("Battlegrounds_Session_Header_Label_Minions_Banned"),
			_ => LocUtil.Get("Battlegrounds_Session_Header_Label_Minions_MinionTypes"),
		};

	public Visibility MinionTypesSectionVisibility =>
		_availableMinionTypesSectionVisibility == Visibility.Visible || _bannedMinionTypesSectionVisibility == Visibility.Visible
			? Visibility.Visible
			: Visibility.Collapsed;

	public Visibility MinionTypesBorderVisibility=>
		_availableMinionTypesSectionVisibility == Visibility.Visible && _bannedMinionTypesSectionVisibility == Visibility.Visible
			? Visibility.Visible
			: Visibility.Collapsed;

	public Visibility CompStatsSectionVisibility =>
		_availableMinionTypesSectionVisibility == Visibility.Visible || _bannedMinionTypesSectionVisibility == Visibility.Visible
			? Visibility.Visible
			: Visibility.Collapsed;

	// Animation delay to update the layout height
	private async Task UpdateBattlegroundsSessionLayoutHeightWithDelay()
	{
		await Task.Delay(300);

		Application.Current.Dispatcher.Invoke(() =>
			Core.Windows.BattlegroundsSessionWindow.UpdateBattlegroundsSessionLayoutHeight());
	}

	private Visibility _availableCompStatsSectionVisibility;
	public Visibility AvailableCompStatsSectionVisibility
	{
		get => _availableCompStatsSectionVisibility;
		set
		{
			_availableCompStatsSectionVisibility = value;

			UpdateBattlegroundsSessionLayoutHeightWithDelay().Forget();

			OnPropertyChanged();
			OnPropertyChanged(nameof(CompStatsSectionVisibility));
		}
	}

	private Visibility _availableMinionTypesSectionVisibility;
	public Visibility AvailableMinionTypesSectionVisibility
	{
		get => _availableMinionTypesSectionVisibility;
		set
		{
			_availableMinionTypesSectionVisibility = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(MinionTypesSectionVisibility));
			OnPropertyChanged(nameof(AvailableMinionTypesHeaderLabel));
			OnPropertyChanged(nameof(MinionTypesBorderVisibility));
		}
	}

	private Visibility _bannedMinionTypesSectionVisibility;
	public Visibility BannedMinionTypesSectionVisibility
	{
		get => _bannedMinionTypesSectionVisibility;
		set
		{
			_bannedMinionTypesSectionVisibility = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(MinionTypesSectionVisibility));
			OnPropertyChanged(nameof(AvailableMinionTypesHeaderLabel));
			OnPropertyChanged(nameof(MinionTypesBorderVisibility));
		}
	}

	private Visibility _minionTypesBodyVisibility;
	public Visibility MinionTypesBodyVisibility
	{
		get => _minionTypesBodyVisibility;
		set
		{
			_minionTypesBodyVisibility = value;
			OnPropertyChanged();
		}
	}

	private Visibility _compStatsErrorVisibility = Visibility.Hidden;

	public Visibility CompStatsErrorVisibility
	{
		get => _compStatsErrorVisibility;
		set
		{
			_compStatsErrorVisibility = value;
			OnPropertyChanged();
		}
	}

	private string? _compStatsDebugInfo;
	public string? CompStatsDebugInfo
	{
		get => _compStatsDebugInfo;
		set
		{
			_compStatsDebugInfo = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(CompStatsDebugVisibility));
		}
	}

	public Visibility CompStatsDebugVisibility => string.IsNullOrWhiteSpace(_compStatsDebugInfo) ? Visibility.Collapsed : Visibility.Visible;

	private Visibility _compStatsWaitingMsgVisibility;
	public Visibility CompStatsWaitingMsgVisibility
	{
		get => _compStatsWaitingMsgVisibility;
		set
		{
			_compStatsWaitingMsgVisibility = value;
			OnPropertyChanged();
		}
	}

	private Visibility _compStatsBodyVisibility = Visibility.Hidden;
	public Visibility CompStatsBodyVisibility
	{
		get => _compStatsBodyVisibility;
		set
		{
			_compStatsBodyVisibility = value;
			OnPropertyChanged();
		}
	}

	private Visibility _minionTypesWaitingMsgVisibility;
	public Visibility MinionTypesWaitingMsgVisibility
	{
		get => _minionTypesWaitingMsgVisibility;
		set
		{
			_minionTypesWaitingMsgVisibility = value;
			OnPropertyChanged();
		}
	}

	private string? _bgRatingStart;
	public string? BgRatingStart
	{
		get => _bgRatingStart;
		set
		{
			_bgRatingStart = value;
			OnPropertyChanged();
		}
	}

	private string? _bgRatingCurrent;
	public string? BgRatingCurrent
	{
		get => _bgRatingCurrent;
		set
		{
			_bgRatingCurrent = value;
			OnPropertyChanged();
		}
	}

	private Visibility _gridHeaderVisibility;
	public Visibility GridHeaderVisibility
	{
		get => _gridHeaderVisibility;
		set
		{
			_gridHeaderVisibility = value;
			OnPropertyChanged();
		}
	}

	private Visibility _gamesEmptyStateVisibility;
	public Visibility GamesEmptyStateVisibility
	{
		get => _gamesEmptyStateVisibility;
		set
		{
			_gamesEmptyStateVisibility = value;
			OnPropertyChanged();
		}
	}

	private Visibility _bgStartCurrentMMRSectionVisibility;
	public Visibility BgStartCurrentMMRSectionVisibility
	{
		get => _bgStartCurrentMMRSectionVisibility;
		set
		{
			_bgStartCurrentMMRSectionVisibility = value;
			OnPropertyChanged();
		}
	}

	private Visibility _bgLatestGamesSectionVisibility;
	public Visibility BgLatestGamesSectionVisibility
	{
		get => _bgLatestGamesSectionVisibility;
		set
		{
			_bgLatestGamesSectionVisibility = value;
			OnPropertyChanged();
		}
	}

	public void SetBattlegroundsCompositionStatsViewModel(List<BattlegroundsCompStats.LobbyComp> compsData)
	{
		var compStatsOrdered = compsData.OrderByDescending(c => c.Popularity).ToList();
		if(compStatsOrdered.Any())
		{
			var max = Math.Max(Math.Ceiling(compStatsOrdered[0].Popularity), 40);
			CompositionStats = compStatsOrdered
				.Where(comp => comp.Id != -1 && comp.Name != null)
				.Select(comp => {
					var minionDbfId = comp.KeyMinionsTop3 == null || !comp.KeyMinionsTop3.Any() ? 59201 : comp.KeyMinionsTop3.First();
					return new BattlegroundsCompositionStatsRowViewModel(
						comp.Name,
						minionDbfId,
						comp.Popularity,
						comp.AvgFinalPlacement,
						max
					);
				});
		}
	}

	public IEnumerable<BattlegroundsCompositionStatsRowViewModel>? CompositionStats
	{
		get => GetProp<IEnumerable<BattlegroundsCompositionStatsRowViewModel>?>(null);
		set => SetProp(value);
	}

	#region Mode
	public SelectedBattlegroundsGameMode BattlegroundsGameMode
	{
		get
		{
			return GetProp(SelectedBattlegroundsGameMode.UNKNOWN);
		}
		set
		{
			var modified = GetProp(SelectedBattlegroundsGameMode.UNKNOWN) != value;
			SetProp(value);
			if(modified)
			{
				UpdateSectionsVisibilities();
				UpdateCompositionStatsVisibility();
				Update();
			}
		}
	}
	#endregion
}
