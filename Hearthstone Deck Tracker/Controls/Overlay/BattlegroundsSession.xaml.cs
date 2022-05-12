using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;
using Hearthstone_Deck_Tracker.Annotations;
using System.Runtime.CompilerServices;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsSession : UserControl, INotifyPropertyChanged
	{
		private Lazy<BattlegroundsDb> _db = new Lazy<BattlegroundsDb>();
		private BrushConverter _bc = new BrushConverter();

		public ObservableCollection<BattlegroundsGame> Games { get; set; } = new ObservableCollection<BattlegroundsGame>();

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		internal virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(int), typeof(BattlegroundsGame));

		public BattlegroundsSession()
		{
			InitializeComponent();
		}

		public int CornerRadius
		{
			get { return (int)GetValue(CornerRadiusProperty); }
			set
			{
				SetValue(CornerRadiusProperty, value);
			}
		}

		public void Update()
		{
			var allRaces = _db.Value.Races;
			var availableRaces = BattlegroundsUtils.GetAvailableRaces(Core.Game.CurrentGameStats?.GameId) ?? allRaces;
			var unavailableRaces = allRaces.Where(x => !availableRaces.Contains(x) && x != Race.INVALID && x != Race.ALL).ToList();

			if(unavailableRaces.Count() >= 3)
			{
				BgTribe1.Tribe = unavailableRaces[0];
				BgTribe2.Tribe = unavailableRaces[1];
				BgTribe3.Tribe = unavailableRaces[2];
				if(unavailableRaces.Count() == 4)
				{
					BgTribe4.Tribe = unavailableRaces[3];
				}
				else
				{
					BgBannedTribes.Children.Remove(BgTribe4);
					BgTribe2.Margin = new Thickness(15, 0, 0, 0);
					BgTribe3.Margin = new Thickness(15, 0, 0, 0);
				}
			}

			var firstGame = UpdateLatestGames();

			var rating = Core.Game.BattlegroundsRatingInfo?.Rating ?? 0;
			var ratingStart = firstGame?.RatingAfter ?? rating;
			BgRatingStart.Text = $"{ratingStart:N0}";
			BgRatingCurrent.Text = $"{rating:N0}";
		}

		private void BtnOptions_MouseUp(object sender, System.Windows.Input.MouseEventArgs e)
		{
			Core.MainWindow.ActivateWindow();
			Core.MainWindow.Options.TreeViewItemOverlayBattlegrounds.IsSelected = true;
			Core.MainWindow.FlyoutOptions.IsOpen = true;
		}

		private void BtnOptions_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			BtnOptions.Background = (Brush)_bc.ConvertFromString("#22FFFFFF");
		}

		private void BtnOptions_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			BtnOptions.Background = (Brush)_bc.ConvertFromString("#00FFFFFF");
		}

		public void Show()
		{
			if (Visibility == Visibility.Visible)
			{
				return;
			}
			Update();
			UpdateSectionsVisibilities();
			Visibility = Visibility.Visible;
		}

		public void Hide()
		{
			Visibility = Visibility.Hidden;
		}

		public GameItem? UpdateLatestGames()
		{
			Games.Clear();
			var sortedGames = BattlegroundsLastGames.Instance.Games
				.OrderBy(g => g.StartTime)
				.ToList();
			deleteOldGames(sortedGames);

			var sessionGames = getSessionGames(sortedGames);

			// Limit list to latest 10 items
			if(sessionGames.Count > 10)
				sessionGames.RemoveRange(0, sessionGames.Count - 10);

			sessionGames.ForEach(AddOrUpdateGame);

			GridHeader.Visibility = sessionGames.Count > 0
				? Visibility.Visible
				: Visibility.Collapsed;

			GamesEmptyState.Visibility = sessionGames.Count == 0
				? Visibility.Visible
				: Visibility.Collapsed;

			return sessionGames.FirstOrDefault();
		}

		private List<GameItem> getSessionGames(List<GameItem> sortedGames)
		{
			DateTime? sessionStartTime = null;
			DateTime? previousGameEndTime = null;
			int previousGameRatingAfter = 0;

			foreach (var g in sortedGames)
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

			if (sessionStartTime == null)
			{
				return sortedGames;
			}

			var sessionGames = sortedGames.Where(g => DateTime.Parse(g.StartTime) >= sessionStartTime).ToList();
			if (sessionGames.Count > 0)
			{
				var lastGame = (GameItem)sessionGames.LastOrDefault();

				// Check for MMR reset on last game
				var currentMMR = Core.Game.BattlegroundsRatingInfo?.Rating ?? 0;
				var sessionLastMMR = lastGame.RatingAfter;
				var ratingResetedAfterLastGame = currentMMR < 500 && currentMMR - sessionLastMMR < -500;

				TimeSpan ts = DateTime.Now - DateTime.Parse(lastGame.EndTime);

				if(ts.TotalHours >= 2 || ratingResetedAfterLastGame)
					return new List<GameItem>();
			}

			return sessionGames;
		}

		private void deleteOldGames(List<GameItem> sortedGames)
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
			var existingGame = Games.FirstOrDefault(x => x?.Game?.StartTime == game.StartTime);
			if (existingGame == null)
			{
				Games.Add(new BattlegroundsGame() { Game = game });
			}
			else
			{
				existingGame.Game = game;
			}
		}

		public void UpdateSectionsVisibilities()
		{
			BgBannedTribesSection.Visibility = Config.Instance.ShowSessionRecapMinionsBanned
				? Visibility.Visible
				: Visibility.Collapsed;

			BgStartCurrentMMRSection.Visibility = Config.Instance.ShowSessionRecapStartCurrentMMR
				? Visibility.Visible
				: Visibility.Collapsed;

			BgLastestGamesSection.Visibility = Config.Instance.ShowSessionRecapLatestGames
				? Visibility.Visible
				: Visibility.Collapsed;
		}
	}
}
