using Hearthstone_Deck_Tracker.Hearthstone;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Hearthstone_Deck_Tracker.Annotations;
using System.Runtime.CompilerServices;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public partial class BattlegroundsSession : UserControl
	{
		private Lazy<BattlegroundsDb> _db = new Lazy<BattlegroundsDb>();
		public ObservableCollection<BattlegroundsGame> Games { get; set; } = new ObservableCollection<BattlegroundsGame>();

		public BattlegroundsSession()
		{
			InitializeComponent();
		}

		private void Update()
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
				} else
				{
					BgBannedTribes.Children.Remove(BgTribe4);
					BgTribe2.Margin = new Thickness(15, 0, 0, 0);
					BgTribe3.Margin = new Thickness(15, 0, 0, 0);
				}
			}

			var sortedGames = BattlegroundsLastGames.Instance.Games
				.OrderBy(g => g.Time)
				.ToList();
			var rating = Core.Game.BattlegroundsRatingInfo?.Rating;
			var ratingStart = sortedGames.FirstOrDefault()?.RatingAfter ?? rating;
			BgRatingStart.Text = $"{ratingStart:N0}";
			BgRatingCurrent.Text = $"{rating:N0}";

			sortedGames.ForEach(AddOrUpdateGame);

			BattlegroundsTierlistPanel.Children.Remove(
				sortedGames.Count == 0 ? GridHeader : GamesEmptyState
			);
		}

		private void AddOrUpdateGame(GameItem game)
		{
			var existingGame = Games.FirstOrDefault(x => x?.Game?.Time == game.Time);
			if (existingGame == null)
			{
				Games.Add(new BattlegroundsGame() { Game = game });
			}
			else
			{
				existingGame.Game = game;
			}
		}

		public void Show()
		{
			if (Visibility == Visibility.Visible)
			{
				return;
			}
			Update();
			Visibility = Visibility.Visible;
		}

		public void Hide()
		{
			Visibility = Visibility.Hidden;
		}
	}
}
