#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.FlyoutControls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for GameStats.xaml
	/// </summary>
	public partial class DeckStatsControl
	{
	    private GameV2 _game;
	    private const string BtnOpponentDeckTextShow = "Show Opp. Deck";
		private const string BtnOpponentDeckTextHide = "Hide Opp. Deck";
		private const int GroupBoxHeaderHeight = 28;
		private readonly Dictionary<GroupBox, bool> _isGroupBoxExpanded;
		private Deck _deck;
		private bool _initialized;
		private HeroClassAll? _opponentCb;

		public DeckStatsControl()
		{
		    
		    InitializeComponent();
			ComboboxGameMode.ItemsSource = Enum.GetValues(typeof(GameMode));
			ComboboxTime.ItemsSource = Enum.GetValues(typeof(TimeFrame));
			ComboBoxOpponentClassD.ItemsSource = Enum.GetValues(typeof(HeroClassAll));
			ComboBoxOpponentClassOG.ItemsSource = Enum.GetValues(typeof(HeroClassAll));
			ComboBoxPlayerClass.ItemsSource = Enum.GetValues(typeof(HeroClassAll));
			ComboboxUnassigned.ItemsSource = Enum.GetValues(typeof(FilterDeckMode));
			_isGroupBoxExpanded = new Dictionary<GroupBox, bool> {{GroupboxClassOverview, true}};
		}

		public Visibility OnlyOverallVisible
		{
			get { return TabControlCurrentOverall.SelectedIndex == 0 ? Visibility.Collapsed : Visibility.Visible; }
		}

		public void LoadConfig(GameV2 game)
		{
            _game = game;
            ComboboxGameMode.SelectedItem = Config.Instance.SelectedStatsFilterGameMode;
			ComboboxTime.SelectedValue = Config.Instance.SelectedStatsFilterTimeFrame;
			ComboboxUnassigned.SelectedValue = Config.Instance.StatsOverallFilterDeckMode;
			ComboBoxPlayerClass.SelectedValue = Config.Instance.StatsOverallFilterPlayerHeroClass;
			ComboBoxOpponentClassD.SelectedValue = Config.Instance.StatsFilterOpponentHeroClass;
			ComboBoxOpponentClassOG.SelectedValue = Config.Instance.StatsFilterOpponentHeroClass;
			CheckBoxApplyTagFiltersOS.IsChecked = Config.Instance.StatsOverallApplyTagFilters;
			CheckBoxApplyTagFiltersOG.IsChecked = Config.Instance.StatsOverallApplyTagFilters;
			_initialized = true;
			ExpandCollapseGroupBox(GroupboxDeckOverview, Config.Instance.StatsDeckOverviewIsExpanded);
			ExpandCollapseGroupBox(GroupboxClassOverview, Config.Instance.StatsClassOverviewIsExpanded);

			LoadOverallStats();

			DataGridGames.Items.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Descending));
			DataGridOverallGames.Items.SortDescriptions.Add(new SortDescription("StartTime", ListSortDirection.Descending));

			Core.MainWindow.FlyoutOpponentDeck.ClosingFinished += (sender, args) =>
			{
				BtnShowOpponentDeck.Content = BtnOpponentDeckTextShow;
				BtnOverallShowOpponentDeck.Content = BtnOpponentDeckTextShow;
			};
		}

		private void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			if(_deck == null)
				return;
			DeleteGames(DataGridGames, false);
		}

		private async void DeleteGames(DataGrid dataGrid, bool overall)
		{
			MetroWindow window;
			if(Config.Instance.StatsInWindow)
				window = Core.Windows.StatsWindow;
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
				if(!overall)
				{
					if(_deck.DeckStats.Games.Contains(selectedGame))
					{
						selectedGame.DeleteGameFile();
						_deck.DeckStats.Games.Remove(selectedGame);
						Logger.WriteLine("Deleted game " + selectedGame + "(overall=" + overall + ")", "DeckStatsControl");
						DeckStatsList.Save();
					}
				}
				else
				{
					var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.DeckStats.Games.Contains(selectedGame));
					if(deck != null)
					{
						if(deck.DeckStats.Games.Contains(selectedGame))
						{
							selectedGame.DeleteGameFile();
							deck.DeckStats.Games.Remove(selectedGame);
							Logger.WriteLine("Deleted game " + selectedGame + "(overall=" + overall + ")", "DeckStatsControl");
							DefaultDeckStats.Save();
						}
					}
					else
					{
						var deckstats = DefaultDeckStats.Instance.DeckStats.FirstOrDefault(ds => ds.Games.Contains(selectedGame));
						if(deckstats != null)
						{
							selectedGame.DeleteGameFile();
							deckstats.Games.Remove(selectedGame);
							Logger.WriteLine("Deleted game " + selectedGame + "(overall=" + overall + ")", "DeckStatsControl");
							DefaultDeckStats.Save();
						}
					}
				}
				if(HearthStatsAPI.IsLoggedIn && selectedGame.HasHearthStatsId && await Core.MainWindow.ShowCheckHearthStatsMatchDeletionDialog())
					HearthStatsManager.DeleteMatchesAsync(new List<GameStats> {selectedGame});
				//Core.MainWindow.DeckPickerList.Items.Refresh();
				Core.MainWindow.DeckPickerList.UpdateDecks();
				Refresh();
			}
			else if(count > 1)
			{
				if(await window.ShowDeleteMultipleGameStatsMessage(count) != MessageDialogResult.Affirmative)
					return;
				var selectedGames = dataGrid.SelectedItems.Cast<GameStats>().Where(g => g != null).ToList();
				foreach(var selectedGame in selectedGames)
				{
					if(!overall)
					{
						if(_deck.DeckStats.Games.Contains(selectedGame))
						{
							selectedGame.DeleteGameFile();
							_deck.DeckStats.Games.Remove(selectedGame);
							Logger.WriteLine("Deleted game " + selectedGame + "(overall=" + overall + ")", "DeckStatsControl");
						}
					}
					else
					{
						var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.DeckStats.Games.Contains(selectedGame));
						if(deck != null)
						{
							if(deck.DeckStats.Games.Contains(selectedGame))
							{
								selectedGame.DeleteGameFile();
								deck.DeckStats.Games.Remove(selectedGame);
								Logger.WriteLine("Deleted game " + selectedGame + "(overall=" + overall + ")", "DeckStatsControl");
							}
						}
						else
						{
							var deckstats = DefaultDeckStats.Instance.DeckStats.FirstOrDefault(ds => ds.Games.Contains(selectedGame));
							if(deckstats != null)
							{
								selectedGame.DeleteGameFile();
								deckstats.Games.Remove(selectedGame);
								Logger.WriteLine("Deleted game " + selectedGame + "(overall=" + overall + ")", "DeckStatsControl");
							}
						}
					}
				}

				if(HearthStatsAPI.IsLoggedIn && selectedGames.Any(g => g.HasHearthStatsId)
				   && await Core.MainWindow.ShowCheckHearthStatsMatchDeletionDialog())
					HearthStatsManager.DeleteMatchesAsync(selectedGames);
				DeckStatsList.Save();
				DefaultDeckStats.Save();
				Logger.WriteLine("Deleted " + count + " games", "DeckStatsControl");
				Core.MainWindow.DeckPickerList.UpdateDecks();
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
				StackPanelUnassignedFilter.Visibility = Visibility.Visible;
				return;
			}
			TabItemDeck.Visibility = Visibility.Visible;
			TabItemOverall.Visibility = Visibility.Visible;
			StackPanelUnassignedFilter.Visibility = TabControlCurrentOverall.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
			DataGridGames.Items.Clear();
			var filteredGames = FilterGames(deck.DeckStats.Games).ToList();
			var modified = false;
			foreach(var game in filteredGames)
			{
				if(!game.VerifiedHeroes && VerifyHeroes(game))
					modified = true;
				if(Config.Instance.StatsFilterOpponentHeroClass == HeroClassAll.All
				   || game.OpponentHero == Config.Instance.StatsFilterOpponentHeroClass.ToString())
					DataGridGames.Items.Add(game);
			}
			if(modified)
				DeckStatsList.Save();
			DataGridWinLoss.Items.Clear();
			DataGridWinLoss.Items.Add(new WinLoss(filteredGames, "%"));
			DataGridWinLoss.Items.Add(new WinLoss(filteredGames, "Win - Loss"));
			//current version
			var games = filteredGames.Where(g => g.BelongsToDeckVerion(deck)).ToList();
			DataGridWinLoss.Items.Add(new WinLoss(games, "%", deck.Version));
			DataGridWinLoss.Items.Add(new WinLoss(games, "Win - Loss", deck.Version));
			//prev versions
			foreach(var v in deck.Versions.OrderByDescending(d => d.Version))
			{
				games = filteredGames.Where(g => g.BelongsToDeckVerion(v)).ToList();
				DataGridWinLoss.Items.Add(new WinLoss(games, "%", v.Version));
				DataGridWinLoss.Items.Add(new WinLoss(games, "Win - Loss", v.Version));
			}

			var defaultStats = DefaultDeckStats.Instance.GetDeckStats(deck.Class) ?? new DeckStats();

			DataGridWinLossClass.Items.Clear();
			var allGames =
				DeckList.Instance.Decks.Where(d => d.GetClass == _deck.GetClass)
				        .SelectMany(d => FilterGames(d.DeckStats.Games).Where(g => !g.IsClone))
				        .Concat(FilterGames(defaultStats.Games))
				        .ToList();

			DataGridWinLossClass.Items.Add(new WinLoss(allGames, "%"));
			DataGridWinLossClass.Items.Add(new WinLoss(allGames, "Win - Loss"));
			DataGridGames.Items.Refresh();
		}

		private IEnumerable<GameStats> FilterGames(IEnumerable<GameStats> games)
		{
			var selectedGameMode = (GameMode)ComboboxGameMode.SelectedItem;
			var noteFilter = TextboxNoteFilter.Text;
			var comboboxString = (TimeFrame)ComboboxTime.SelectedItem;

			var endTime = DateTime.Today + new TimeSpan(0, 23, 59, 59, 999);
			var startTime = DateTime.Today;

			switch(comboboxString)
			{
				case TimeFrame.Today:
					endTime = DateTime.Now;
					break;
				case TimeFrame.Yesterday:
					startTime -= new TimeSpan(1, 0, 0, 0);
					endTime -= new TimeSpan(1, 0, 0, 0);
					break;
				case TimeFrame.Last24Hours:
					startTime = DateTime.Now - new TimeSpan(1, 0, 0, 0);
					endTime = DateTime.Now;
					break;
				case TimeFrame.ThisWeek:
					startTime -= new TimeSpan(((int)(startTime.DayOfWeek) - 1), 0, 0, 0);
					break;
				case TimeFrame.PreviousWeek:
					startTime -= new TimeSpan(7 + ((int)(startTime.DayOfWeek) - 1), 0, 0, 0);
					endTime -= new TimeSpan(((int)(endTime.DayOfWeek)), 0, 0, 0);
					break;
				case TimeFrame.Last7Days:
					startTime -= new TimeSpan(7, 0, 0, 0);
					break;
				case TimeFrame.ThisMonth:
					startTime -= new TimeSpan(startTime.Day - 1, 0, 0, 0);
					break;
				case TimeFrame.PreviousMonth:
					startTime -= new TimeSpan(startTime.Day - 1 + DateTime.DaysInMonth(startTime.AddMonths(-1).Year, startTime.AddMonths(-1).Month), 0,
					                          0, 0);
					endTime -= new TimeSpan(endTime.Day, 0, 0, 0);
					break;
				case TimeFrame.ThisYear:
					startTime -= new TimeSpan(startTime.DayOfYear - 1, 0, 0, 0);
					break;
				case TimeFrame.PreviousYear:
					startTime -= new TimeSpan(startTime.DayOfYear - 1 + (DateTime.IsLeapYear(startTime.Year) ? 366 : 365), 0, 0, 0);
					endTime -= new TimeSpan(startTime.DayOfYear, 0, 0, 0);
					break;
				case TimeFrame.AllTime:
					startTime = new DateTime();
					break;
			}

			return
				games.Where(
				            g =>
				            (g.GameMode == selectedGameMode || selectedGameMode == GameMode.All) && g.StartTime >= startTime
				            && g.StartTime <= endTime
				            && (g.Note == null && noteFilter == string.Empty
				                || g.Note != null && g.Note.ToLowerInvariant().Contains(noteFilter.ToLowerInvariant())));
		}

		public void Refresh()
		{
			var oldSelectionOverall = DataGridOverallGames.SelectedItem;
			LoadOverallStats();
			if(oldSelectionOverall != null)
				DataGridOverallGames.SelectedItem = oldSelectionOverall;

			if(_deck == null)
				return;
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
			var target = e.OriginalSource as DependencyObject;
			if(target == null)
				return;
			while(!(target is DataGridRow))
			{
				target = VisualTreeHelper.GetParent(target);
				if(target == null)
					return;
			}
			OpenGameDetails(DataGridGames.SelectedItem as GameStats);
		}

		private void OpenGameDetails(GameStats selected)
		{
			if(selected != null)
			{
				if(selected.HasReplayFile && !Keyboard.IsKeyDown(Key.LeftCtrl)) //hold ctrl to open old game viewer
					ReplayReader.LaunchReplayViewer(selected.ReplayFile);
				else if(Config.Instance.StatsInWindow)
				{
					Core.Windows.StatsWindow.GameDetailsFlyout.SetGame(selected);
					Core.Windows.StatsWindow.FlyoutGameDetails.Header = selected.ToString();
					Core.Windows.StatsWindow.FlyoutGameDetails.IsOpen = true;
				}
				else
				{
					Core.MainWindow.GameDetailsFlyout.SetGame(selected);
					Core.MainWindow.FlyoutGameDetails.Header = selected.ToString();
					Core.MainWindow.FlyoutGameDetails.IsOpen = true;
				}
			}
		}

		private void BtnOverallMoveToOtherDeck_Click(object sender, RoutedEventArgs e)
		{
			MoveGameToOtherDeck(DataGridOverallGames.SelectedItems.Cast<GameStats>().ToList());
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
			DeleteGames(DataGridOverallGames, true);
		}

		private void DGridOverall_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var target = e.OriginalSource as DependencyObject;
			if(target == null)
				return;
			while(!(target is DataGridRow))
			{
				target = VisualTreeHelper.GetParent(target);
				if(target == null)
					return;
			}
			OpenGameDetails(DataGridOverallGames.SelectedItem as GameStats);
		}

		private void DGridOverall_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var enabled = DataGridOverallGames.SelectedItems.Count > 0;
			BtnOverallDelete.IsEnabled = enabled;
			BtnOverallDetails.IsEnabled = enabled;
			BtnOverallNote.IsEnabled = enabled;
			BtnOverallShowOpponentDeck.IsEnabled = enabled;
			BtnOverallMoveToOtherDeck.IsEnabled = enabled;
			BtnOverallEditGame.IsEnabled = enabled;
			if(DataGridOverallGames.SelectedItems.Count > 0)
			{
				var selectedGames = DataGridOverallGames.SelectedItems.Cast<GameStats>().ToList();
				var allTheSameHero = selectedGames.All(g => g.PlayerHero == selectedGames[0].PlayerHero);
				BtnOverallMoveToOtherDeck.IsEnabled = allTheSameHero;
				if(Core.MainWindow.FlyoutOpponentDeck.IsOpen)
				{
					var game = DataGridOverallGames.SelectedItem as GameStats;
					if(game != null)
						ImportOpponentDeck(game);
				}
			}
		}

		private void ComboboxGameMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SelectedStatsFilterGameMode = (GameMode)ComboboxGameMode.SelectedValue;
			Config.Save();
			Refresh();
		}

		private void ComboboxTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SelectedStatsFilterTimeFrame = (TimeFrame)ComboboxTime.SelectedItem;
			Config.Save();
			Refresh();
		}

		private void DGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var enabled = DataGridGames.SelectedItems.Count > 0;
			BtnDelete.IsEnabled = enabled;
			BtnDetails.IsEnabled = enabled;
			BtnShowOpponentDeck.IsEnabled = enabled;
			BtnNote.IsEnabled = enabled;
			BtnMoveToOtherDeck.IsEnabled = enabled;
			BtnEditGame.IsEnabled = enabled;
			if(Core.MainWindow.FlyoutOpponentDeck.IsOpen)
			{
				var game = DataGridGames.SelectedItem as GameStats;
				if(game != null)
					ImportOpponentDeck(game);
			}
		}

		private void BtnEditNote_Click(object sender, RoutedEventArgs e)
		{
			EditNote(DataGridGames.SelectedItem as GameStats);
		}

		private async void EditNote(GameStats selected)
		{
			if(selected == null)
				return;
			var settings = new MessageDialogs.Settings {DefaultText = selected.Note};
			string newNote;
			if(Config.Instance.StatsInWindow)
				newNote = await Core.Windows.StatsWindow.ShowInputAsync("Note", "", settings);
			else
				newNote = await Core.MainWindow.ShowInputAsync("Note", "", settings);
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
			Refresh();
		}

		private void BtnMoveToOtherDeck_Click(object sender, RoutedEventArgs e)
		{
			MoveGameToOtherDeck(DataGridGames.SelectedItems.Cast<GameStats>().ToList());
		}

		private void MoveGameToOtherDeck(List<GameStats> selectedGames)
		{
			if(selectedGames == null)
				return;

			var heroes = new Dictionary<string, int>();
			foreach(var game in selectedGames)
			{
				if(!heroes.ContainsKey(game.PlayerHero))
					heroes.Add(game.PlayerHero, 0);
				heroes[game.PlayerHero]++;
			}

			var heroPlayed = heroes.Any() ? heroes.OrderByDescending(x => x.Value).First().Key : "Any";
			var possibleTargets = DeckList.Instance.Decks.Where(d => d.Class == heroPlayed || heroPlayed == "Any");

			var dialog = new MoveGameDialog(possibleTargets);
			if(Config.Instance.StatsInWindow)
				dialog.Owner = Core.Windows.StatsWindow;
			else
				dialog.Owner = Helper.MainWindow;

			dialog.ShowDialog();
			var selectedDeck = dialog.SelectedDeck;

			if(selectedDeck == null)
				return;
			foreach(var game in selectedGames)
			{
				var defaultDeck = DefaultDeckStats.Instance.DeckStats.FirstOrDefault(ds => ds.Games.Contains(game));
				if(defaultDeck != null)
				{
					defaultDeck.Games.Remove(game);
					DefaultDeckStats.Save();
				}
				else
				{
					var deck = DeckList.Instance.Decks.FirstOrDefault(d => game.DeckId == d.DeckId);
					if(deck != null)
						deck.DeckStats.Games.Remove(game);
				}
                game.PlayerDeckVersion = dialog.SelectedVersion;
                game.HearthStatsDeckVersionId = selectedDeck.GetVersion(dialog.SelectedVersion).HearthStatsDeckVersionId;
                game.DeckId = selectedDeck.DeckId;
                game.DeckName = selectedDeck.Name;
				selectedDeck.DeckStats.Games.Add(game);
				if(HearthStatsAPI.IsLoggedIn && Config.Instance.HearthStatsAutoUploadNewGames)
					HearthStatsManager.MoveMatchAsync(game, selectedDeck, background: true);
			}
			DeckStatsList.Save();
			DeckList.Save();
			Refresh();
			Core.MainWindow.DeckPickerList.UpdateDecks();
		}

		private bool VerifyHeroes(GameStats game)
		{
			// If its Brawl skip verification
			if(game.GameMode == GameMode.Brawl)
			{
				game.VerifiedHeroes = true;
				return false;
			}

			var modifiedHero = false;
			var playerHeroes = new Dictionary<string, int>();
			var opponentHeroes = new Dictionary<string, int>();
			foreach(var turn in game.TurnStats)
			{
				foreach(var play in turn.Plays)
				{
					if(string.IsNullOrEmpty(play.CardId))
						continue;
					if(play.Type.ToString().Contains("Player"))
					{
						var hero = Database.GetCardFromId(play.CardId).PlayerClass;
						if(hero == null)
							continue;
						if(!playerHeroes.ContainsKey(hero))
							playerHeroes.Add(hero, 0);
						playerHeroes[hero]++;
					}
					else if(play.Type.ToString().Contains("Opponent"))
					{
						var hero = Database.GetCardFromId(play.CardId).PlayerClass;
						if(hero == null)
							continue;
						if(!opponentHeroes.ContainsKey(hero))
							opponentHeroes.Add(hero, 0);
						opponentHeroes[hero]++;
					}
				}
			}
			if(playerHeroes.Count > 0)
			{
				var pHero = playerHeroes.OrderByDescending(x => x.Value).First().Key;
				if(game.PlayerHero != pHero)
				{
                    game.PlayerHero = pHero;
					modifiedHero = true;
				}
			}
			if(opponentHeroes.Count > 0)
			{
				var oHero = opponentHeroes.OrderByDescending(x => x.Value).First().Key;
				if(game.OpponentHero != oHero)
				{
                    game.OpponentHero = oHero;
					modifiedHero = true;
				}
			}

            game.VerifiedHeroes = true;
			return modifiedHero;
		}

		public void LoadOverallStats()
		{
			var needToSaveDeckStats = false;
			DataGridOverallWinLoss.Items.Clear();
			DataGridOverallGames.Items.Clear();
			var sortedCol = DataGridOverallGames.Columns.FirstOrDefault(col => col.SortDirection != null);
			var total = new List<GameStats>();
			var modified = false;
			var classes = Enum.GetNames(typeof(HeroClass)).Concat(DefaultDeckStats.Instance.DeckStats.Select(x => x.Name)).Distinct();
            foreach(var @class in classes)
			{
				var allGames = new List<GameStats>();
				if(Config.Instance.StatsOverallFilterDeckMode == FilterDeckMode.WithDeck
				   || Config.Instance.StatsOverallFilterDeckMode == FilterDeckMode.All)
					allGames.AddRange(DeckList.Instance.Decks.Where(x => x.Class == @class && MatchesTagFilters(x)).SelectMany(d => d.DeckStats.Games));
				if(Config.Instance.StatsOverallFilterDeckMode == FilterDeckMode.WithoutDeck
				   || Config.Instance.StatsOverallFilterDeckMode == FilterDeckMode.All)
					allGames.AddRange(DefaultDeckStats.Instance.GetDeckStats(@class).Games);

				allGames = FilterGames(allGames).Where(g => !g.IsClone).ToList();

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
					if(!game.VerifiedHeroes && VerifyHeroes(game))
						modified = true;
					if((Config.Instance.StatsOverallFilterPlayerHeroClass == HeroClassAll.All
					    || game.PlayerHero == Config.Instance.StatsOverallFilterPlayerHeroClass.ToString())
					   && (Config.Instance.StatsFilterOpponentHeroClass == HeroClassAll.All
					       || game.OpponentHero == Config.Instance.StatsFilterOpponentHeroClass.ToString()))
						DataGridOverallGames.Items.Add(game);
				}
			}
			if(needToSaveDeckStats || modified)
				DeckStatsList.Save();
			DataGridOverallWinLoss.Items.Add(new WinLoss(total, CheckboxPercent.IsChecked ?? true, "Total"));
			if(sortedCol != null)
			{
				var prevSorted = DataGridOverallGames.Columns.FirstOrDefault(col => col.Header == sortedCol.Header);
				if(prevSorted != null)
					prevSorted.SortDirection = sortedCol.SortDirection;
			}
			DataGridOverallGames.Items.Refresh();
		}

		private bool MatchesTagFilters(Deck deck)
		{
			return !Config.Instance.StatsOverallApplyTagFilters || Config.Instance.SelectedTags.Contains("All")
			       || deck.Tags.Any(tag => Config.Instance.SelectedTags.Contains(tag));
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

		private void ComboboxUnassigned_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.StatsOverallFilterDeckMode = (FilterDeckMode)ComboboxUnassigned.SelectedItem;
			Config.Save();
			LoadOverallStats();
		}

		private void CheckBoxApplyTagFilters_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.StatsOverallApplyTagFilters = true;
			Config.Save();
			LoadOverallStats();
		}

		private void CheckBoxApplyTagFilters_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.StatsOverallApplyTagFilters = false;
			Config.Save();
			LoadOverallStats();
		}

		private void ComboBoxPlayerClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.StatsOverallFilterPlayerHeroClass = (HeroClassAll)ComboBoxPlayerClass.SelectedItem;
			Config.Save();
			LoadOverallStats();
		}

		private void ComboBoxOpponentClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			if(_opponentCb == null)
			{
				_opponentCb = (HeroClassAll)((ComboBox)sender).SelectedItem;
				if(!Equals(sender, ComboBoxOpponentClassD))
					ComboBoxOpponentClassD.SelectedItem = _opponentCb.Value;
				if(!Equals(sender, ComboBoxOpponentClassOG))
					ComboBoxOpponentClassOG.SelectedItem = _opponentCb.Value;
				Config.Instance.StatsFilterOpponentHeroClass = _opponentCb.Value;
				Config.Save();
				Refresh();
				_opponentCb = null;
			}
		}

		private void TabItemDeck_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			StackPanelUnassignedFilter.Visibility = Visibility.Collapsed;
		}

		private void TabItemOverall_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			StackPanelUnassignedFilter.Visibility = Visibility.Visible;
		}

		private void BtnOverallShowOpponentDeck_Click(object sender, RoutedEventArgs e)
		{
			if(Core.MainWindow.FlyoutOpponentDeck.IsOpen)
				CloseOpponentDeckFlyout();
			else
			{
				var game = DataGridOverallGames.SelectedItem as GameStats;
				if(game != null)
					ImportOpponentDeck(game);
			}
		}

		private void BtnShowOpponentDeck_Click(object sender, RoutedEventArgs e)
		{
			if(Core.MainWindow.FlyoutOpponentDeck.IsOpen)
				CloseOpponentDeckFlyout();
			else
			{
				var game = DataGridGames.SelectedItem as GameStats;
				if(game != null)
					ImportOpponentDeck(game);
			}
		}

		private void CloseOpponentDeckFlyout()
		{
			Core.MainWindow.FlyoutOpponentDeck.IsOpen = false;
			BtnOverallShowOpponentDeck.Content = BtnOpponentDeckTextShow;
			BtnShowOpponentDeck.Content = BtnOpponentDeckTextShow;
		}

		private void ImportOpponentDeck(GameStats stats)
		{
			if(stats == null)
				return;
			Core.MainWindow.OpponentDeckFlyout.SetDeck(stats.GetOpponentDeck());
			Core.MainWindow.FlyoutOpponentDeck.IsOpen = true;
			BtnOverallShowOpponentDeck.Content = BtnOpponentDeckTextHide;
			BtnShowOpponentDeck.Content = BtnOpponentDeckTextHide;
		}

		private async void BtnAddNewGame_Click(object sender, RoutedEventArgs e)
		{
			var addedGame = await Core.MainWindow.ShowAddGameDialog(_deck);
			if(addedGame)
				Refresh();
		}

		private void BtnEditGame_Click(object sender, RoutedEventArgs e)
		{
			var game = DataGridGames.SelectedItem as GameStats;
			if(game != null)
				EditGame(game);
		}

		private void BtnOverallEditGame_Click(object sender, RoutedEventArgs e)
		{
			var game = DataGridOverallGames.SelectedItem as GameStats;
			if(game != null)
				EditGame(game);
		}

		private async void EditGame(GameStats game)
		{
			if(game == null)
				return;
			var edited = await Core.MainWindow.ShowEditGameDialog(game);
			if(edited)
				Refresh();
		}

		private void TabControlDeck_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			TabControlOverall.SelectedIndex = TabControlDeck.SelectedIndex;
		}

		private void TabControlOverall_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			TabControlDeck.SelectedIndex = TabControlOverall.SelectedIndex;
		}

		public class WinLoss
		{
			private readonly bool _percent;
			private readonly string _playerHero;
			private readonly List<GameStats> _stats;

			public WinLoss(List<GameStats> stats, string text) : this(stats, text, null)
			{
			}

			public WinLoss(List<GameStats> stats, string text, SerializableVersion version)
			{
				_percent = text == "%";
				_stats = stats;
				Text = text;
				if(version == null)
					Version = "ALL";
				else
					Version = version.ToString("v{M}.{m}");
			}

			public WinLoss(List<GameStats> stats, bool percent, string playerHero)
			{
				_percent = percent;
				_stats = stats;
				_playerHero = playerHero;
			}

			public string Version { get; set; }

			public BitmapImage PlayerHeroImage
			{
				get
				{
					HeroClassAll playerHero;
					if(Enum.TryParse(_playerHero, out playerHero))
						return ImageCache.GetClassIcon(playerHero);
					return new BitmapImage();
				}
			}

			public Visibility VisibilityImage
			{
				get { return _playerHero != "Total" ? Visibility.Visible : Visibility.Collapsed; }
			}

			public Visibility VisibilityText
			{
				get { return _playerHero == "Total" ? Visibility.Visible : Visibility.Collapsed; }
			}

			public string PlayerText
			{
				get { return _playerHero.ToUpper(); }
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
				if(_stats == null)
					return "0 - 0";
				var wins = _stats.Count(s => s.Result == GameResult.Win && (hsClass == null || s.OpponentHero == hsClass));
				var losses = _stats.Count(s => s.Result == GameResult.Loss && (hsClass == null || s.OpponentHero == hsClass));
				return wins + " - " + losses;
			}

			private string GetPercent(string hsClass = null)
			{
				if(_stats == null)
					return "-";
				var wins = _stats.Count(s => s.Result == GameResult.Win && (hsClass == null || s.OpponentHero == hsClass));
				var total = _stats.Count(s => s.Result != GameResult.None && (hsClass == null || s.OpponentHero == hsClass));
				return total > 0 ? Math.Round(100.0 * wins / total, 1) + "%" : "-";
			}

			private string GetClassDisplayString(string hsClass)
			{
				return _percent ? GetPercent(hsClass) : GetWinLoss(hsClass);
			}
		}
	}
}