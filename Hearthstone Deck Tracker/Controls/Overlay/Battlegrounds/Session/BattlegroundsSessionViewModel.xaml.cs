using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;

public class BattlegroundsSessionViewModel : ViewModel
{
	private readonly Lazy<BattlegroundsDb> _db = new();

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

	public async void Update()
	{
		if(Core.Game.Spectator)
			await Task.Delay(1500);

		UpdateMinionTypes();

		var firstGame = await UpdateLatestGames();

		var rating = Core.Game.BattlegroundsRatingInfo?.Rating ?? 0;
		var ratingStart = firstGame?.Rating ?? rating;
		if(rating == 0)
			rating = ratingStart;
		BgRatingStart = $"{ratingStart:N0}";
		BgRatingCurrent = $"{rating:N0}";
	}

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

	private void UpdateMinionTypes()
	{
		var allRaces = _db.Value.Races.Where(x => x != Race.INVALID && x != Race.ALL).ToList();
		var availableRaces = BattlegroundsUtils.GetAvailableRaces(Core.Game.CurrentGameStats?.GameId)?.ToList() ?? allRaces;
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

	private async Task<GameItem?> UpdateLatestGames()
	{
		SessionGames.Clear();
		var sortedGames = (await Instance.PlayerGames())
			.OrderBy(g => g.StartTime)
			.ToList();
		DeleteOldGames(sortedGames);

		var sessionGames = GetSessionGames(sortedGames);
		var firstGame = sessionGames.FirstOrDefault();

		// Limit list to latest 10 items
		if(sessionGames.Count > 10)
			sessionGames.RemoveRange(0, sessionGames.Count - 10);

		sessionGames.OrderByDescending(g => g.StartTime)
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
				var ratingReseted = g.Rating < 500 && diffMMR < -500;

				if(ts.TotalHours >= 2 || ratingReseted)
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
			var ratingResetedAfterLastGame = false;
			if(Core.Game.BattlegroundsRatingInfo?.Rating != null)
			{
				var currentMMR = Core.Game.BattlegroundsRatingInfo?.Rating;
				var sessionLastMMR = lastGame.RatingAfter;
				ratingResetedAfterLastGame = currentMMR < 500 && currentMMR - sessionLastMMR < -500;
			}

			var ts = DateTime.Now - DateTime.Parse(lastGame.EndTime);

			if(ts.TotalHours >= 2 || ratingResetedAfterLastGame)
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
}
