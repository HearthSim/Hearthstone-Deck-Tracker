using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Stats.Arena.Charts;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker.Controls.Stats.Arena
{
	/// <summary>
	/// Interaction logic for ArenaOverview.xaml
	/// </summary>
	public partial class ArenaOverview : INotifyPropertyChanged
	{
		private readonly bool _initialized;
		private object _chartWinsControl = new ChartWins();

		public ArenaOverview()
		{
			InitializeComponent();
			_initialized = true;
		}

		public object ChartWinsControl
		{
			get { return _chartWinsControl; }
			set
			{
				_chartWinsControl = value; 
				OnPropertyChanged();
			}
		}

		public GameStats SelectedGame { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if(handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		private void ButtonEditRewards_OnClick(object sender, RoutedEventArgs e)
		{
			var run = DataGridArenaRuns.SelectedItem as ArenaRun;
			if(run == null)
				return;
			var rewardDialog = new ArenaRewardDialog(run.Deck) {WindowStartupLocation = WindowStartupLocation.CenterOwner};
			rewardDialog.ShowDialog();
			if(rewardDialog.SaveButtonWasClicked)
				CompiledStats.Instance.UpdateArenaRuns();
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
				CompiledStats.Instance.UpdateArenaStats();
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
				CompiledStats.Instance.UpdateArenaStats();
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
				Logger.WriteLine("Deleted game " + SelectedGame, "ArenaOverview.ButtonDeleteGame");
			}
			if(HearthStatsAPI.IsLoggedIn && SelectedGame.HasHearthStatsId && await window.ShowCheckHearthStatsMatchDeletionDialog())
				HearthStatsManager.DeleteMatchesAsync(new List<GameStats> {SelectedGame});
			DeckStatsList.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
			CompiledStats.Instance.UpdateArenaStats();
		}

		private void ButtonShowReplay_OnClick(object sender, RoutedEventArgs e)
		{

		}

		private void ButtonShowDeck_OnClick(object sender, RoutedEventArgs e)
		{

		}
	}
}
