using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for GameStats.xaml
	/// </summary>
	public partial class DeckStatsControl
	{
		private const int GroupBoxHeaderHeight = 28;
		private readonly Dictionary<GroupBox, bool> _isGroupBoxExpanded;
		private Deck _deck;
		private bool _initialized;

		public DeckStatsControl()
		{
			InitializeComponent();
			ComboboxGameMode.ItemsSource = Enum.GetValues(typeof(Game.GameMode));
			_isGroupBoxExpanded = new Dictionary<GroupBox, bool> {{GroupboxDeckOverview, true}, {GroupboxClassOverview, true}};
		}

		public void LoadConfig()
		{
			ComboboxGameMode.SelectedItem = Config.Instance.SelectedStatsFilterGameMode;
			ComboboxTime.SelectedValue = Config.Instance.SelectedStatsFilterTime;
			_initialized = true;
			ExpandCollapseGroupBox(GroupboxDeckOverview, Config.Instance.StatsDeckOverviewIsExpanded);
			ExpandCollapseGroupBox(GroupboxClassOverview, Config.Instance.StatsClassOverviewIsExpanded);
		}

		private async void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			if(_deck == null)
				return;

			MetroWindow window;
			if(Config.Instance.StatsInWindow)
				window = Helper.MainWindow.StatsWindow;
			else
				window = Helper.MainWindow;

			var count = DataGridGames.SelectedItems.Count;
			if(count == 1)
			{
				var selectedGame = DataGridGames.SelectedItem as GameStats;
				if(selectedGame == null)
					return;

				if(await MessageDialogs.ShowDeleteGameStatsMessage(window, selectedGame) != MessageDialogResult.Affirmative)
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
			else if(count > 1)
			{
				if(await MessageDialogs.ShowDeleteMultipleGameStatsMessage(window, count) != MessageDialogResult.Affirmative)
					return;
				foreach(var selectedItem in DataGridGames.SelectedItems)
				{
					var selectedGame = selectedItem as GameStats;
					if(selectedGame == null) continue;
					if(!_deck.DeckStats.Games.Contains(selectedGame)) continue;
					selectedGame.DeleteGameFile();
					_deck.DeckStats.Games.Remove(selectedGame);
				}
				DeckStatsList.Save();
				Logger.WriteLine("Deleted " + count + " games");
				Helper.MainWindow.DeckPickerList.Items.Refresh();
				Refresh();
			}
		}

		public void SetDeck(Deck deck)
		{
			_deck = deck;
			if(deck == null) return;
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
			var noteFilter = TextboxNoteFilter.Text;
			DataGridGames.Items.Clear();
			var filteredGames = deck.DeckStats.Games.Where(g => (g.GameMode == selectedGameMode
			                                                     || selectedGameMode == Game.GameMode.All)
			                                                    && g.StartTime > timeFrame
			                                                    && (g.Note == null && noteFilter == string.Empty
			                                                        || g.Note != null && g.Note.Contains(noteFilter))).ToList();
			foreach(var game in filteredGames)
				DataGridGames.Items.Add(game);
			DataGridWinLoss.Items.Clear();
			DataGridWinLoss.Items.Add(new WinLoss(filteredGames, "%"));
			DataGridWinLoss.Items.Add(new WinLoss(filteredGames, "Win - Loss"));

			DataGridWinLossClass.Items.Clear();
			var allGames = Helper.MainWindow.DeckList.DecksList
			                     .Where(d => d.GetClass == _deck.GetClass)
			                     .SelectMany(d => d.DeckStats.Games
			                                       .Where(g => (g.GameMode == selectedGameMode
			                                                    || selectedGameMode == Game.GameMode.All)
			                                                   && g.StartTime > timeFrame
			                                                   && (g.Note == null && noteFilter == string.Empty
			                                                       || g.Note != null && g.Note.Contains(noteFilter)))).ToList();
			DataGridWinLossClass.Items.Add(new WinLoss(allGames, "%"));
			DataGridWinLossClass.Items.Add(new WinLoss(allGames, "Win - Loss"));
			DataGridGames.Items.SortDescriptions.Clear();
			DataGridGames.Items.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Descending));
		}

		public void Refresh()
		{
			if(_deck == null) return;
			var oldSelection = DataGridGames.SelectedItem;
			SetDeck(_deck);
			if(oldSelection != null)
				DataGridGames.SelectedItem = oldSelection;
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
			var selected = DataGridGames.SelectedItem as GameStats;
			if(selected != null)
			{
				if(Config.Instance.StatsInWindow)
				{
					Helper.MainWindow.StatsWindow.GameDetailsFlyout.SetGame(selected);
					Helper.MainWindow.StatsWindow.FlyoutGameDetails.Header = selected.ToString();
					Helper.MainWindow.StatsWindow.FlyoutGameDetails.IsOpen = true;
				}
				else
				{
					Helper.MainWindow.GameDetailsFlyout.SetGame(selected);
					Helper.MainWindow.FlyoutGameDetails.Header = selected.ToString();
					Helper.MainWindow.FlyoutGameDetails.IsOpen = true;
				}
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

		private void DGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var enabled = DataGridGames.SelectedItems.Count > 0;
			BtnDelete.IsEnabled = enabled;
			BtnDetails.IsEnabled = enabled;
			BtnNote.IsEnabled = enabled;
			BtnMoveToOtherDeck.IsEnabled = enabled;
		}

		private async void BtnEditNote_Click(object sender, RoutedEventArgs e)
		{
			var selected = DataGridGames.SelectedItem as GameStats;
			if(selected == null) return;
			var settings = new MetroDialogSettings {DefaultText = selected.Note};
			string newNote;
			if(Config.Instance.StatsInWindow)
				newNote = await Helper.MainWindow.StatsWindow.ShowInputAsync("Note", "", settings);
			else
				newNote = await Helper.MainWindow.ShowInputAsync("Note", "", settings);
			if(newNote == null)
				return;
			selected.Note = newNote;
			DeckStatsList.Save();
			Refresh();
		}


		private void GroupBoxDeckOverview_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if(e.GetPosition(GroupboxDeckOverview).Y < GroupBoxHeaderHeight)
			{
				Config.Instance.StatsDeckOverviewIsExpanded = ExpandCollapseGroupBox(GroupboxDeckOverview);
				Config.Save();
			}
		}

		private void GroupboxClassOverview_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if(e.GetPosition(GroupboxClassOverview).Y < GroupBoxHeaderHeight)
			{
				Config.Instance.StatsClassOverviewIsExpanded = ExpandCollapseGroupBox(GroupboxClassOverview);
				Config.Save();
			}
		}

		private bool ExpandCollapseGroupBox(GroupBox groupBox, bool? expand = null)
		{
			_isGroupBoxExpanded[groupBox] = expand ?? !_isGroupBoxExpanded[groupBox];
			groupBox.Height = _isGroupBoxExpanded[groupBox] ? double.NaN : GroupBoxHeaderHeight;
			if(_isGroupBoxExpanded[groupBox])
				groupBox.Header = groupBox.Header.ToString().Replace("> ", string.Empty);
			else
				groupBox.Header = "> " + groupBox.Header;
			return _isGroupBoxExpanded[groupBox];
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			//todo: probably not the best performance
			Refresh();
		}

		private void BtnMoveToOtherDeck_Click(object sender, RoutedEventArgs e)
		{
			var selectedGame = DataGridGames.SelectedItem as GameStats;
			if(selectedGame == null) return;

			var possibleTargets = Helper.MainWindow.DeckList.DecksList.Where(d => d.Class == _deck.Class);

			var dialog = new MoveGameDialog(possibleTargets);
			if(Config.Instance.StatsInWindow)
				dialog.Owner = Helper.MainWindow.StatsWindow;
			else dialog.Owner = Helper.MainWindow;

			dialog.ShowDialog();
			var selectedDeck = dialog.SelectedDeck;

			if(selectedDeck == null) return;

			_deck.DeckStats.Games.Remove(selectedGame);
			selectedDeck.DeckStats.Games.Add(selectedGame);
			DeckStatsList.Save();
			Helper.MainWindow.WriteDecks();
			Refresh();
			Helper.MainWindow.DeckPickerList.UpdateList();
		}

		private class WinLoss
		{
			private readonly bool _percent;
			private readonly List<GameStats> _stats;

			public WinLoss(List<GameStats> stats, string text)
			{
				_percent = text == "%";
				_stats = stats;
				Text = text;
			}

			public string Text { get; private set; }

			public string Total
			{
				get { return _percent ? GetPercent() : GetWinLoss(); }
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

			private string GetWinLoss(string hsClass = null)
			{
				var wins = _stats.Count(s => s.Result.ToString() == "Win" && (hsClass == null || s.OpponentHero == hsClass));
				var losses = _stats.Count(s => s.Result.ToString() == "Loss" && (hsClass == null || s.OpponentHero == hsClass));
				return wins + " - " + losses;
			}

			private string GetPercent(string hsClass = null)
			{
				var wins = _stats.Count(s => s.Result.ToString() == "Win" && (hsClass == null || s.OpponentHero == hsClass));
				var total = _stats.Count(s => (hsClass == null || s.OpponentHero == hsClass));
				var percent = total > 0 ? Math.Round(100.0 * wins / total, 1).ToString() : "-";
				return string.Format("{0}%", percent);
			}

			private string GetClassDisplayString(string hsClass)
			{
				return _percent ? GetPercent(hsClass) : GetWinLoss(hsClass);
			}
		}
	}
}