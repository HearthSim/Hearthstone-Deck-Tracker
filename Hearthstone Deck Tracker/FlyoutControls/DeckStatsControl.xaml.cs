using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for GameStats.xaml
	/// </summary>
	public partial class DeckStatsControl
	{
		private Deck _deck;
		private bool _initialized;

		public DeckStatsControl()
		{
			InitializeComponent();
			ComboboxGameMode.ItemsSource = Enum.GetValues(typeof(Game.GameMode));
		}

		public void LoadConfig()
		{
			ComboboxGameMode.SelectedItem = Config.Instance.SelectedStatsFilterGameMode;
			ComboboxTime.SelectedValue = Config.Instance.SelectedStatsFilterTime;
			_initialized = true;
		}

		private async void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			if(_deck == null)
				return;

			var selectedGame = DGrid.SelectedItem as GameStats;
			if(selectedGame == null)
				return;

			if(await Helper.MainWindow.ShowDeleteGameStatsMessage(selectedGame) != MessageDialogResult.Affirmative)
				return;

			if(_deck.DeckStats.Games.Contains(selectedGame))
			{
				selectedGame.DeleteGameFile();
				_deck.DeckStats.Games.Remove(selectedGame);
				DeckStatsList.Save();
				Logger.WriteLine("Deleted game: " + selectedGame);
				Helper.MainWindow.DeckPickerList.Items.Refresh();
				Refresh();
			}
		}

		public void SetDeck(Deck deck)
		{
			_deck = deck;
			var selectedGameMode = (Game.GameMode)ComboboxGameMode.SelectedItem;
			var comboboxString = ComboboxTime.SelectedValue.ToString();
			var timeFrame = DateTime.Now.Date;
			switch(comboboxString)
			{
				case "Today":
					//timeFrame -= new TimeSpan(0, 0, 0, 0);
					break;
				case "Last Week":
					timeFrame -= new TimeSpan(7, 0, 0, 0);
					break;
				case "Last Month":
					timeFrame -= new TimeSpan(30, 0, 0, 0);
					break;
				case "Last Year":
					timeFrame -= new TimeSpan(365, 0, 0, 0);
					break;
				case "All Time":
					timeFrame = new DateTime();
					break;
			}
			DGrid.Items.Clear();
			var filteredGames = deck.DeckStats.Games.Where(g => (g.GameMode == selectedGameMode
			                                                     || selectedGameMode == Game.GameMode.All)
			                                                    && g.StartTime > timeFrame).ToList();

			foreach(var game in filteredGames)
				DGrid.Items.Add(game);
			DataGridWinLoss.Items.Clear();
			DataGridWinLoss.Items.Add(new WinLoss(filteredGames, "Win"));
			DataGridWinLoss.Items.Add(new WinLoss(filteredGames, "Loss"));

			DGrid.Items.SortDescriptions.Clear();
			DGrid.Items.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Descending));
		}

		public void Refresh()
		{
			if(_deck != null)
				SetDeck(_deck);
		}

		private void BtnDetails_Click(object sender, RoutedEventArgs e)
		{
			OpenGameDetails();
		}

		private void DGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			OpenGameDetails();
		}

		private void OpenGameDetails()
		{
			var selected = DGrid.SelectedItem as GameStats;
			if(selected != null)
			{
				Helper.MainWindow.GameDetailsFlyout.SetGame(selected);
				Helper.MainWindow.FlyoutGameDetails.Header = selected.ToString();
				Helper.MainWindow.FlyoutGameDetails.Width = Helper.MainWindow.FlyoutDeckStats.ActualWidth;
				Helper.MainWindow.FlyoutGameDetails.IsOpen = true;
			}
		}

		private void ComboboxGameMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.SelectedStatsFilterGameMode = (Game.GameMode)ComboboxGameMode.SelectedValue;
			Config.Save();
			Refresh();
		}

		private void ComboboxTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.SelectedStatsFilterTime = (string)ComboboxTime.SelectedValue;
			Config.Save();
			Refresh();
		}

		private class WinLoss
		{
			private readonly List<GameStats> _stats;

			public WinLoss(List<GameStats> stats, string result)
			{
				_stats = stats;
				Result = result;
			}

			public string Result { get; private set; }

			public string Total
			{
				get
				{
					var numGames = _stats.Count(s => s.Result.ToString() == Result);
					return GetDisplayString(numGames, _stats.Count);
				}
			}

			public string Druid
			{
				get { return GetClassDisplayString("Druid"); }
			}

			public string Hunter
			{
				get { return GetClassDisplayString("Hunter"); }
			}

			public string Mage
			{
				get { return GetClassDisplayString("Mage"); }
			}

			public string Paladin
			{
				get { return GetClassDisplayString("Paladin"); }
			}

			public string Priest
			{
				get { return GetClassDisplayString("Priest"); }
			}

			public string Rogue
			{
				get { return GetClassDisplayString("Rogue"); }
			}

			public string Shaman
			{
				get { return GetClassDisplayString("Shaman"); }
			}

			public string Warrior
			{
				get { return GetClassDisplayString("Warrior"); }
			}

			public string Warlock
			{
				get { return GetClassDisplayString("Warlock"); }
			}

			private string GetClassDisplayString(string hsClass)
			{
				var numGames = _stats.Count(s => s.Result.ToString() == Result && s.OpponentHero == hsClass);
				var total = _stats.Count(s => s.OpponentHero == hsClass);
				return GetDisplayString(numGames, total);
			}

			private string GetDisplayString(int num, int total)
			{
				var percent = total > 0 ? Math.Round(100.0 * num / total, 2).ToString() : "-";
				return string.Format("{0} ({1}%)", num, percent);
			}
		}
	}
}