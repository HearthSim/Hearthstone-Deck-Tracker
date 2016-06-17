#region

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats.Arena
{
	/// <summary>
	/// Interaction logic for ArenaRunsTable.xaml
	/// </summary>
	public partial class ArenaRunsTable : UserControl
	{
		private ArenaRun _selectedRun;

		public ArenaRunsTable()
		{
			InitializeComponent();
		}

		public ArenaRun SelectedRun
		{
			get { return _selectedRun ?? (DataGridArenaRuns.Items.IsEmpty ? null : (ArenaRun)DataGridArenaRuns.Items.GetItemAt(0)); }
			set
			{
				if(value != null)
					_selectedRun = value;
			}
		}

		public GameStats SelectedGame { get; set; }

		private void ButtonEditRewards_OnClick(object sender, RoutedEventArgs e)
		{
			var run = DataGridArenaRuns.SelectedItem as ArenaRun;
			if(run == null)
				return;
			var rewardDialog = new ArenaRewardDialog(run.Deck) {WindowStartupLocation = WindowStartupLocation.CenterOwner};
			rewardDialog.ShowDialog();
		}

		private async void ButtonAddGame_OnClick(object sender, RoutedEventArgs e)
		{
			var run = DataGridArenaRuns.SelectedItem as ArenaRun;
			if(run == null)
				return;
			var window = Helper.GetParentWindow(this);
			if(window == null)
				return;
			var addedGame = await window.ShowAddGameDialog(run.Deck);
			if(addedGame)
				ArenaStats.Instance.UpdateArenaStats();
		}

		private async void ButtonEditGame_OnClick(object sender, RoutedEventArgs e)
		{
			if(SelectedGame == null)
				return;
			var window = Helper.GetParentWindow(this);
			if(window == null)
				return;
			var edited = await window.ShowEditGameDialog(SelectedGame);
			if(edited)
				ArenaStats.Instance.UpdateArenaStats();
		}

		private async void ButtonDeleteGame_OnClick(object sender, RoutedEventArgs e)
		{
			if(SelectedGame == null)
				return;
			var run = DataGridArenaRuns.SelectedItem as ArenaRun;
			if(run == null)
				return;
			var window = Helper.GetParentWindow(this);
			if(await window.ShowDeleteGameStatsMessage(SelectedGame) != MessageDialogResult.Affirmative)
				return;
			if(run.Deck.DeckStats.Games.Contains(SelectedGame))
			{
				SelectedGame.DeleteGameFile();
				run.Deck.DeckStats.Games.Remove(SelectedGame);
				Log.Info("Deleted game " + SelectedGame);
			}
			if(HearthStatsAPI.IsLoggedIn && SelectedGame.HasHearthStatsId && await window.ShowCheckHearthStatsMatchDeletionDialog())
				HearthStatsManager.DeleteMatchesAsync(new List<GameStats> {SelectedGame}).Forget();
			DeckStatsList.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
			ArenaStats.Instance.UpdateArenaStats();
		}

		private void ButtonShowReplay_OnClick(object sender, RoutedEventArgs e)
		{
			if(SelectedGame == null)
				return;
			if(SelectedGame.HasReplayFile)
				ReplayReader.LaunchReplayViewer(SelectedGame.ReplayFile);
		}

		private void ButtonShowOppDeck_OnClick(object sender, RoutedEventArgs e)
		{
			if(SelectedGame == null)
				return;
			if(Config.Instance.StatsInWindow)
			{
				Core.Windows.StatsWindow.DeckFlyout.SetDeck(SelectedGame.OpponentCards);
				Core.Windows.StatsWindow.FlyoutDeck.IsOpen = true;
			}
			else
			{
				Core.MainWindow.DeckFlyout.SetDeck(SelectedGame.OpponentCards);
				Core.MainWindow.FlyoutDeck.IsOpen = true;
			}
		}

		//http://stackoverflow.com/questions/3498686/wpf-remove-scrollviewer-from-treeview
		private void ForwardScrollEvent(object sender, MouseWheelEventArgs e)
		{
			if(e.Handled)
				return;
			e.Handled = true;
			var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {RoutedEvent = MouseWheelEvent, Source = sender};
			var parent = ((Control)sender).Parent as UIElement;
			parent?.RaiseEvent(eventArg);
		}

		private void DataGridArenaRuns_OnTargetUpdated(object sender, DataTransferEventArgs e) => DataGridArenaRuns.SelectedItem = SelectedRun;

		private void ButtonShowDeck_OnClick(object sender, RoutedEventArgs e)
		{
			var run = DataGridArenaRuns.SelectedItem as ArenaRun;
			if(run == null)
				return;
			Core.MainWindow.DeckFlyout.SetDeck(run.Deck, false);
			Core.MainWindow.FlyoutDeck.Header = run.Deck.Name;
			Core.MainWindow.FlyoutDeck.IsOpen = true;
		}
	}
}