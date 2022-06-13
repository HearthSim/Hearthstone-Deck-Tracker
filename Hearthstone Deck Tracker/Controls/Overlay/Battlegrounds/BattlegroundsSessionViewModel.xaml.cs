using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds
{
	public partial class BattlegroundsSessionViewModel : ViewModel, INotifyPropertyChanged
	{
		private Lazy<BattlegroundsDb> _db = new();

		public ObservableCollection<BattlegroundsGameViewModel> SessionGames { get; set; } = new();
		
		public void Reset()
		{
			BgRatingStart = "0";
			BgRatingCurrent = "0";
			BannedTribe1 = Race.BEAST;
			BannedTribe2 = Race.BEAST;
			BannedTribe3 = Race.BEAST;
			BannedTribe4 = Race.BEAST;
			BannedTribesVisibility = Visibility.Collapsed;
			BannedTribesMsgVisibility = Visibility.Visible;
			GridHeaderVisibility = Visibility.Collapsed;
			GamesEmptyStateVisibility = Visibility.Visible;
		}

		public void OnGameEnd()
		{
			if(Core.Game.Spectator)
				return;

			var currentRating = Core.Game.CurrentGameStats?.BattlegroundsRatingAfter;
			BgRatingCurrent = $"{currentRating:N0}";

			OnPropertyChanged(nameof(BgRatingCurrent));
			UpdateLatestGames();
		}

		public async void Update()
		{
			if(Core.Game.Spectator)
				await Task.Delay(1500);

			UpdateBannedTribes();

			var firstGame = UpdateLatestGames();

			var rating = Core.Game.BattlegroundsRatingInfo?.Rating ?? 0;
			var ratingStart = firstGame?.Rating ?? rating;
			if(rating == 0)
				rating = ratingStart;
			BgRatingStart = $"{ratingStart:N0}";
			BgRatingCurrent = $"{rating:N0}";

			OnPropertyChanged(nameof(BgRatingStart));
			OnPropertyChanged(nameof(BgRatingCurrent));
		}

		public void UpdateSectionsVisibilities()
		{
			BgBannedTribesSectionVisibility = Config.Instance.ShowSessionRecapMinionsBanned
				? Visibility.Visible
				: Visibility.Collapsed;

			BgStartCurrentMMRSectionVisibility = Config.Instance.ShowSessionRecapStartCurrentMMR
				? Visibility.Visible
				: Visibility.Collapsed;

			BgLatestGamesSectionVisibility = Config.Instance.ShowSessionRecapLatestGames
				? Visibility.Visible
				: Visibility.Collapsed;

			OnPropertyChanged(nameof(BgBannedTribesSectionVisibility));
			OnPropertyChanged(nameof(BgStartCurrentMMRSectionVisibility));
			OnPropertyChanged(nameof(BgLatestGamesSectionVisibility));
			Core.Windows.BattlegroundsSessionWindow.UpdateBattlegroundsSessionLayoutHeight();
		}

		private void UpdateBannedTribes()
		{
			var allRaces = _db.Value.Races;
			var availableRaces = BattlegroundsUtils.GetAvailableRaces(Core.Game.CurrentGameStats?.GameId) ?? allRaces;
			var unavailableRaces = allRaces.Where(x => !availableRaces.Contains(x) && x != Race.INVALID && x != Race.ALL)
				.OrderBy(t => BattlegroundsTribe.GetTribeName(t))
				.ToList();

			var bannedTribesUpdated = unavailableRaces.Count() >= 4;
			if(bannedTribesUpdated)
			{
				BannedTribe1 = unavailableRaces[0];
				BannedTribe2 = unavailableRaces[1];
				BannedTribe3 = unavailableRaces[2];
				BannedTribe4 = unavailableRaces[3];

				OnPropertyChanged(nameof(BannedTribe1));
				OnPropertyChanged(nameof(BannedTribe2));
				OnPropertyChanged(nameof(BannedTribe3));
				OnPropertyChanged(nameof(BannedTribe4));
			}

			if(Core.Game.CurrentMode == Mode.GAMEPLAY && bannedTribesUpdated)
			{
				BannedTribesVisibility = Visibility.Visible;
				BannedTribesMsgVisibility = Visibility.Collapsed;
			}
			else
			{
				BannedTribesVisibility = Visibility.Collapsed;
				BannedTribesMsgVisibility = Visibility.Visible;
			}
			OnPropertyChanged(nameof(BannedTribesVisibility));
			OnPropertyChanged(nameof(BannedTribesMsgVisibility));
		}

		private GameItem? UpdateLatestGames()
		{
			SessionGames.Clear();
			var sortedGames = BattlegroundsLastGames.Instance.Games
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

			OnPropertyChanged(nameof(GridHeaderVisibility));
			OnPropertyChanged(nameof(GamesEmptyStateVisibility));
			Core.Windows.BattlegroundsSessionWindow.UpdateBattlegroundsSessionLayoutHeight();

			return firstGame;
		}

		private List<GameItem> GetSessionGames(List<GameItem> sortedGames)
		{
			DateTime? sessionStartTime = null;
			DateTime? previousGameEndTime = null;
			int previousGameRatingAfter = 0;

			foreach(var g in sortedGames)
			{
				if(previousGameEndTime != null)
				{
					var gStartTime = DateTime.Parse(g.StartTime);
					TimeSpan ts = gStartTime - (DateTime)previousGameEndTime;

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

				TimeSpan ts = DateTime.Now - DateTime.Parse(lastGame.EndTime);

				if(ts.TotalHours >= 2 || ratingResetedAfterLastGame)
					return new List<GameItem>();
			}

			return sessionGames;
		}

		private void DeleteOldGames(List<GameItem> sortedGames)
		{
			sortedGames.ForEach(g =>
			{
				TimeSpan ts = DateTime.Now - DateTime.Parse(g.StartTime);
				if(g.StartTime != null && ts.TotalDays >= 7)
					BattlegroundsLastGames.Instance.RemoveGame(g.StartTime);
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

		public string? BgRatingStart { get; set; }
		public string? BgRatingCurrent { get; set; }
		public Race BannedTribe1 { get; set; }
		public Race BannedTribe2 { get; set; }
		public Race BannedTribe3 { get; set; }
		public Race BannedTribe4 { get; set; }
		public Visibility BannedTribesVisibility { get; set; }
		public Visibility BannedTribesMsgVisibility { get; set; }
		public Visibility GridHeaderVisibility { get; set; }
		public Visibility GamesEmptyStateVisibility { get; set; }
		public Visibility BgBannedTribesSectionVisibility { get; set; }
		public Visibility BgStartCurrentMMRSectionVisibility { get; set; }
		public Visibility BgLatestGamesSectionVisibility { get; set; }
	}
}
