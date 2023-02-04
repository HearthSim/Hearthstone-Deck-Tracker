using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session
{
	public partial class BattlegroundsSessionViewModel : ViewModel
	{
		private readonly Lazy<BattlegroundsDb> _db = new();

		public ObservableCollection<BattlegroundsGameViewModel> SessionGames { get; set; } = new();
		public ObservableCollection<Rase> AvailableRaces { get; set; } = new();
		public ObservableCollection<Race> UnavailableRaces { get; set; } = new();

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

			UpdateBannedTribes();

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
			BgAvailableTribesSectionVisibility = Config.Instance.ShowSessionRecapMinionsAvailable
				? Visibility.Visible
				: Visibility.Collapsed;

			BgBannedTribesSectionVisibility = Config.Instance.ShowSessionRecapMinionsBanned
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

		private void UpdateBannedTribes()
		{
			var allRaces = _db.Value.Races;
			var availableRaces = BattlegroundsUtils.GetAvailableRaces(Core.Game.CurrentGameStats?.GameId) ?? allRaces;
			var unavailableRaces = allRaces.Where(x => !availableRaces.Contains(x) && x != Race.INVALID && x != Race.ALL);

			AvailableRaces.Clear();
			foreach (var item in availableRaces.OrderBy(t => HearthDbConverter.RaceConverter(t) ?? "").ToList()) {
				AvailableRaces.Add(item);
			}

			UnavailableRaces.Clear();
			foreach (var item in unavailableRaces.OrderBy(t => HearthDbConverter.RaceConverter(t) ?? "").ToList()) {
				UnavailableRaces.Add(item);
			}

			var bannedTribesUpdated = unavailableRaces.Count() >= 5;

			if(Core.Game.CurrentMode == Mode.GAMEPLAY && bannedTribesUpdated)
			{
				BannedTribesVisibility = Visibility.Visible;
				AvailableTribesVisibility = Visibility.Visible;
				TribesMsgVisibility = Visibility.Collapsed;
			}
			else
			{
				BannedTribesVisibility = Visibility.Collapsed;
				AvailableTribesVisibility = Visibility.Collapsed;
				TribesMsgVisibility = Visibility.Visible;
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

		private Visibility _bannedTribesVisibility;
		public Visibility BannedTribesVisibility
		{
			get => _bannedTribesVisibility;
			set
			{
				_bannedTribesVisibility = value;
				OnPropertyChanged();
			}
		}

		private Visibility _availableTribesVisibility;
		public Visibility AvailableTribesVisibility
		{
			get => _availableTribesVisibility;
			set
			{
				_availableTribesVisibility = value;
				OnPropertyChanged();
			}
		}

		private Visibility _TribesMsgVisibility;
		public Visibility TribesMsgVisibility
		{
			get => _TribesMsgVisibility;
			set
			{
				_TribesMsgVisibility = value;
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

		private Visibility _bgBannedTribesSectionVisibility;
		public Visibility BgBannedTribesSectionVisibility
		{
			get => _bgBannedTribesSectionVisibility;
			set
			{
				_bgBannedTribesSectionVisibility = value;
				OnPropertyChanged();
			}
		}

		private Visibility _bgAvailableTribesSectionVisibility;
		public Visibility BgAvailableTribesSectionVisibility
		{
			get => _bgAvailableTribesSectionVisibility;
			set
			{
				_bgAvailableTribesSectionVisibility = value;
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
}
