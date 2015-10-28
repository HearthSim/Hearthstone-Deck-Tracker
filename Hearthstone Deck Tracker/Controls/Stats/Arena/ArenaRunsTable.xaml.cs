using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;
using Control = System.Windows.Controls.Control;
using UserControl = System.Windows.Controls.UserControl;

namespace Hearthstone_Deck_Tracker.Controls.Stats.Arena
{
	/// <summary>
	/// Interaction logic for ArenaRunsTable.xaml
	/// </summary>
	public partial class ArenaRunsTable : UserControl
	{
		private ArenaRun _selectedRun;

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

		public ArenaRunsTable()
		{
			InitializeComponent();
        }

		private void ButtonEditRewards_OnClick(object sender, RoutedEventArgs e)
		{
			var run = DataGridArenaRuns.SelectedItem as ArenaRun;
			if(run == null)
				return;
			var rewardDialog = new ArenaRewardDialog(run.Deck) { WindowStartupLocation = WindowStartupLocation.CenterOwner };
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
				Logger.WriteLine("Deleted game " + SelectedGame, "Runs.ButtonDeleteGame");
			}
			if(HearthStatsAPI.IsLoggedIn && SelectedGame.HasHearthStatsId && await window.ShowCheckHearthStatsMatchDeletionDialog())
				HearthStatsManager.DeleteMatchesAsync(new List<GameStats> { SelectedGame });
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

		private void ButtonShowDeck_OnClick(object sender, RoutedEventArgs e)
		{
			if(SelectedGame == null)
				return;
			Core.MainWindow.OpponentDeckFlyout.SetDeck(SelectedGame.GetOpponentDeck());
			Core.MainWindow.FlyoutOpponentDeck.IsOpen = true;
		}

		//http://stackoverflow.com/questions/3498686/wpf-remove-scrollviewer-from-treeview
		private void ForwardScrollEvent(object sender, MouseWheelEventArgs e)
		{
			if(!e.Handled)
			{
				e.Handled = true;
				var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = MouseWheelEvent, Source = sender };
				var parent = ((Control)sender).Parent as UIElement;
				if(parent != null)
					parent.RaiseEvent(eventArg);
			}
		}

		private void DataGridArenaRuns_OnTargetUpdated(object sender, DataTransferEventArgs e)
		{
			DataGridArenaRuns.SelectedItem = SelectedRun;
		}
	}
}
