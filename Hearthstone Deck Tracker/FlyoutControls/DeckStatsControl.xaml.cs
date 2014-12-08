using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
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

		public Visibility OnlyOverallVisible
		{
			get { return TabControlCurrentOverall.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible; }
		}

		public DeckStatsControl()
		{
			InitializeComponent();
			ComboboxGameMode.ItemsSource = Enum.GetValues(typeof(Game.GameMode));
			_isGroupBoxExpanded = new Dictionary<GroupBox, bool> {{GroupboxClassOverview, true}};
		}

		public void LoadConfig()
		{
			ComboboxGameMode.SelectedItem = Config.Instance.SelectedStatsFilterGameMode;
			ComboboxTime.SelectedValue = Config.Instance.SelectedStatsFilterTime;
			ComboboxUnassigned.SelectedValue = Config.Instance.StatsOverallAssignedOnly;
			_initialized = true;
			ExpandCollapseGroupBox(GroupboxDeckOverview, Config.Instance.StatsDeckOverviewIsExpanded);
			ExpandCollapseGroupBox(GroupboxClassOverview, Config.Instance.StatsClassOverviewIsExpanded);
			ExpandCollapseGroupBox(GroupboxOverallTotalOverview, Config.Instance.StatsOverallTotalIsExpanded);
			ExpandCollapseGroupBox(GroupboxOverallDetailOverview, Config.Instance.StatsOverallDetailIsExpanded);

			LoadOverallStats();
		}

		private void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			if(_deck == null)
				return;
			DeleteGames(DataGridGames);

		}

		private async void DeleteGames(DataGrid dataGrid)
		{
			MetroWindow window;
			if(Config.Instance.StatsInWindow)
				window = Helper.MainWindow.StatsWindow;
			else
				window = Helper.MainWindow;

			var count = dataGrid.SelectedItems.Count;
			if(count == 1)
			{
				var selectedGame = dataGrid.SelectedItem as GameStats;
				if(selectedGame == null)
					return;

				if(await window.ShowDeleteGameStatsMessage(selectedGame) != MessageDialogResult.Affirmative)
					return;
				if(_deck.DeckStats.Games.Contains(selectedGame))
				{
					selectedGame.DeleteGameFile();
					_deck.DeckStats.Games.Remove(selectedGame);
					Logger.WriteLine("Deleted game: " + selectedGame);
					DeckStatsList.Save();
				}
				else
				{
					var deckstats = DefaultDeckStats.Instance.DeckStats.FirstOrDefault(ds => ds.Games.Contains(selectedGame));
					if(deckstats != null)
					{
						selectedGame.DeleteGameFile();
						deckstats.Games.Remove(selectedGame);
						Logger.WriteLine("Deleted game: " + selectedGame);
						DefaultDeckStats.Save();
					}
				}
				Helper.MainWindow.DeckPickerList.Items.Refresh();
				Refresh();
			}
			else if(count > 1)
			{
				if(await window.ShowDeleteMultipleGameStatsMessage(count) != MessageDialogResult.Affirmative)
					return;
				foreach(var selectedItem in dataGrid.SelectedItems)
				{
					var selectedGame = selectedItem as GameStats;
					if(selectedGame == null) continue;
					if(_deck.DeckStats.Games.Contains(selectedGame))
					{
						selectedGame.DeleteGameFile();
						_deck.DeckStats.Games.Remove(selectedGame);
					}
					else
					{
						var deckstats = DefaultDeckStats.Instance.DeckStats.FirstOrDefault(ds => ds.Games.Contains(selectedGame));
						if(deckstats != null)
						{
							selectedGame.DeleteGameFile();
							deckstats.Games.Remove(selectedGame);
						}
					}

				}
				DeckStatsList.Save();
				DefaultDeckStats.Save();
				Logger.WriteLine("Deleted " + count + " games");
				Helper.MainWindow.DeckPickerList.Items.Refresh();
				Refresh();
			}
		}

		public void SetDeck(Deck deck)
		{
			_deck = deck;
			if(deck == null)
			{
				TabControlCurrentOverall.SelectedIndex = 1;
				TabItemDeck.Visibility = Visibility.Collapsed;
				TabItemOverall.Visibility = Visibility.Collapsed;
				return;
			}
			TabItemDeck.Visibility = Visibility.Visible;
			TabItemOverall.Visibility = Visibility.Visible;
			DataGridGames.Items.Clear();
			var filteredGames = FilterGames(deck.DeckStats.Games).ToList();
			foreach(var game in filteredGames)
				DataGridGames.Items.Add(game);
			DataGridWinLoss.Items.Clear();
			DataGridWinLoss.Items.Add(new WinLoss(filteredGames, "%"));
			DataGridWinLoss.Items.Add(new WinLoss(filteredGames, "Win - Loss"));

			var defaultStats = DefaultDeckStats.Instance.GetDeckStats(deck.Class) ?? new DeckStats();

			DataGridWinLossClass.Items.Clear();
			var allGames = Helper.MainWindow.DeckList.DecksList
			                     .Where(d => d.GetClass == _deck.GetClass)
			                     .SelectMany(d => FilterGames(d.DeckStats.Games))
								 .Concat(FilterGames(defaultStats.Games)).ToList();

			DataGridWinLossClass.Items.Add(new WinLoss(allGames, "%"));
			DataGridWinLossClass.Items.Add(new WinLoss(allGames, "Win - Loss"));
			DataGridGames.Items.SortDescriptions.Clear();
			DataGridGames.Items.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Descending));
		}
		
		private IEnumerable<GameStats> FilterGames(IEnumerable<GameStats> games)
		{
			var selectedGameMode = (Game.GameMode)ComboboxGameMode.SelectedItem;
			var noteFilter = TextboxNoteFilter.Text;
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
			return
				games.Where(g =>
				            (g.GameMode == selectedGameMode || selectedGameMode == Game.GameMode.All) && g.StartTime > timeFrame &&
				            (g.Note == null && noteFilter == string.Empty || g.Note != null && g.Note.Contains(noteFilter)));
		} 

		public void Refresh()
		{
			var oldSelectionOverall = DataGridOverallGames.SelectedItem;
			LoadOverallStats();
			if(oldSelectionOverall != null)
				DataGridOverallGames.SelectedItem = oldSelectionOverall;

			if(_deck == null) return;
			var oldSelectionCurrent = DataGridGames.SelectedItem;
			SetDeck(_deck);
            if(oldSelectionCurrent != null)
				DataGridGames.SelectedItem = oldSelectionCurrent;
		}

		private void BtnDetails_Click(object sender, RoutedEventArgs e)
		{
			OpenGameDetails(DataGridGames.SelectedItem as GameStats);
        }

		private void DGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			OpenGameDetails(DataGridGames.SelectedItem as GameStats);
		}

		private void OpenGameDetails(GameStats selected)
		{
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

		private void BtnOverallMoveToOtherDeck_Click(object sender, RoutedEventArgs e)
		{
			MoveGameToOtherDeck(DataGridOverallGames.SelectedItem as GameStats);
		}

		private void BtnOverallDetails_Click(object sender, RoutedEventArgs e)
		{
			OpenGameDetails(DataGridOverallGames.SelectedItem as GameStats);
		}

		private void BtnOverallEditNote_Click(object sender, RoutedEventArgs e)
		{
			EditNote(DataGridOverallGames.SelectedItem as GameStats);
		}

		private void BtnOverallDelete_Click(object sender, RoutedEventArgs e)
		{
			DeleteGames(DataGridOverallGames);
		}

		private void DGridOverall_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			OpenGameDetails(DataGridOverallGames.SelectedItem as GameStats);
		}

		private void DGridOverall_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var enabled = DataGridOverallGames.SelectedItems.Count > 0;
			BtnOverallDelete.IsEnabled = enabled;
			BtnOverallDetails.IsEnabled = enabled;
			BtnOverallNote.IsEnabled = enabled;
			BtnOverallMoveToOtherDeck.IsEnabled = enabled;
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

		private void BtnEditNote_Click(object sender, RoutedEventArgs e)
		{
			EditNote(DataGridGames.SelectedItem as GameStats);
		}

		private async void EditNote(GameStats selected)
		{
			if(selected == null) return;
			var settings = new MetroDialogSettings { DefaultText = selected.Note };
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
		
		private void GroupboxOverallTotalOverview_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if(e.GetPosition(GroupboxOverallTotalOverview).Y < GroupBoxHeaderHeight)
			{
				Config.Instance.StatsOverallTotalIsExpanded = ExpandCollapseGroupBox(GroupboxOverallTotalOverview);
				Config.Save();
			}
		}

		private void GroupboxOverallDetailOverview_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if(e.GetPosition(GroupboxOverallDetailOverview).Y < GroupBoxHeaderHeight)
			{
				Config.Instance.StatsOverallDetailIsExpanded = ExpandCollapseGroupBox(GroupboxOverallDetailOverview);
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
			MoveGameToOtherDeck(DataGridGames.SelectedItem as GameStats);
		}

		private void MoveGameToOtherDeck(GameStats selectedGame)
		{
			if(selectedGame == null)
				return;

			var heroes = new Dictionary<string, int>();
			foreach(var turn in selectedGame.TurnStats)
			{
				foreach(var play in turn.Plays)
				{
					if(!play.Type.ToString().Contains("Player"))
						continue;
					var hero = Game.GetCardFromId(play.CardId).PlayerClass;
					if(hero == null)
						continue;
					if(!heroes.ContainsKey(hero))
						heroes.Add(hero, 0);
					heroes[hero]++;
				}
			}

			var heroPlayed = heroes.OrderByDescending(x => x.Value).First().Key;

			var possibleTargets = Helper.MainWindow.DeckList.DecksList.Where(d => d.Class == heroPlayed);

			var dialog = new MoveGameDialog(possibleTargets);
			if(Config.Instance.StatsInWindow)
				dialog.Owner = Helper.MainWindow.StatsWindow;
			else
				dialog.Owner = Helper.MainWindow;

			dialog.ShowDialog();
			var selectedDeck = dialog.SelectedDeck;

			if(selectedDeck == null)
				return;
			var defaultDeck = DefaultDeckStats.Instance.DeckStats.FirstOrDefault(ds => ds.Games.Contains(selectedGame));
            if(defaultDeck != null)
            {
	            defaultDeck.Games.Remove(selectedGame);
				DefaultDeckStats.Save();
            }
			else
			{
				_deck.DeckStats.Games.Remove(selectedGame);
			}
			selectedDeck.DeckStats.Games.Add(selectedGame);
			DeckStatsList.Save();
			Helper.MainWindow.WriteDecks();
			Refresh();
			Helper.MainWindow.DeckPickerList.UpdateList();
		}


		public void LoadOverallStats()
		{
			var needToSaveDeckStats = false;
			DataGridOverallWinLoss.Items.Clear();
			DataGridOverallTotal.Items.Clear();
			DataGridOverallGames.Items.Clear();
			var total = new List<GameStats>();
			foreach(var @class in Game.Classes)
			{
				var allGames = new List<GameStats>();
				if(Config.Instance.StatsOverallAssignedOnly == "With deck" || Config.Instance.StatsOverallAssignedOnly == "All")
					allGames.AddRange(Helper.MainWindow.DeckList.DecksList.Where(x => x.Class == @class).SelectMany(d => d.DeckStats.Games));
				if(Config.Instance.StatsOverallAssignedOnly == "Without deck" || Config.Instance.StatsOverallAssignedOnly == "All")
					allGames.AddRange(DefaultDeckStats.Instance.GetDeckStats(@class).Games);

				allGames = FilterGames(allGames).ToList();
				total.AddRange(allGames);
				DataGridOverallWinLoss.Items.Add(new WinLoss(allGames, CheckboxPercent.IsChecked ?? true, @class));

				foreach(var game in allGames)
				{
					if(string.IsNullOrEmpty(game.PlayerHero))
					{
						//for some reason this does not get loaded after saving it to the xml
						game.PlayerHero = @class;
						needToSaveDeckStats = true;
					}
					DataGridOverallGames.Items.Add(game);
				}
			}
			if(needToSaveDeckStats)
			{
				DeckStatsList.Save();
			}
			DataGridOverallTotal.Items.Add(new WinLoss(total, "%"));
			DataGridOverallTotal.Items.Add(new WinLoss(total, "Win - Loss"));

		}

		private void CheckboxPercent_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			LoadOverallStats();
		}

		private void CheckboxPercent_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			LoadOverallStats();
		}
		
		public class WinLoss
		{
			private readonly bool _percent;
			private readonly List<GameStats> _stats;
			private readonly string _playerHero;

			public WinLoss(List<GameStats> stats, string text)
			{
				_percent = text == "%";
				_stats = stats;
				Text = text;
			}

			public WinLoss(List<GameStats> stats, bool percent, string playerHero)
			{
				_percent = percent;
				_stats = stats;
				_playerHero = playerHero;
			}

			public BitmapImage PlayerHeroImage
			{
				get
				{
					if(!Game.Classes.Contains(_playerHero))
						return new BitmapImage();
					var uri = new Uri(string.Format("../Resources/{0}_small.png", _playerHero.ToLower()), UriKind.Relative);
					return new BitmapImage(uri);
				}
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
				return total > 0 ? Math.Round(100.0 * wins / total, 1) + "%" : "-";
			}

			private string GetClassDisplayString(string hsClass)
			{
				return _percent ? GetPercent(hsClass) : GetWinLoss(hsClass);
			}
		}
		
		private void ComboboxUnassigned_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.StatsOverallAssignedOnly = ComboboxUnassigned.SelectedValue.ToString();
			Config.Save();
			LoadOverallStats();
		}
		
		private void TabItemHeaderOverall_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			StackPanelUnassignedFilter.Visibility = Visibility.Visible;
		}

		private void TabItemHeaderCurrent_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			StackPanelUnassignedFilter.Visibility = Visibility.Collapsed;
		}
	}
}