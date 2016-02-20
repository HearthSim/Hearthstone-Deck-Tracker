#region

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Replay;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Stats.CompiledStats;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.Stats.Constructed
{
	/// <summary>
	/// Interaction logic for ConstructedGames.xaml
	/// </summary>
	public partial class ConstructedGames : INotifyPropertyChanged
	{
		private List<GameStats> _selectedGames;

		public ConstructedGames()
		{
			InitializeComponent();
		}

		public List<GameStats> SelectedGames
			=> _selectedGames ?? (_selectedGames = DataGridGames.SelectedItems?.Cast<GameStats>().ToList() ?? new List<GameStats>());

		public GameStats SelectedGame { get; set; }

		public DataGridRowDetailsVisibilityMode RowDetailVisibility
			=> SelectedGames.Count > 1 ? DataGridRowDetailsVisibilityMode.Collapsed : DataGridRowDetailsVisibilityMode.VisibleWhenSelected;

		public Visibility MultiSelectPanelVisibility => SelectedGames.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
		public bool ButtonMultiMoveEnabled => SelectedGames.All(g => g.PlayerHero == SelectedGames.First().PlayerHero);

		public event PropertyChangedEventHandler PropertyChanged;

		private void DataGridGames_OnTargetUpdated(object sender, DataTransferEventArgs e)
		{
		}

		private void ButtonShowOppDeck_OnClick(object sender, RoutedEventArgs e)
		{
			if(SelectedGame == null)
				return;
			Core.MainWindow.DeckFlyout.SetDeck(SelectedGame.GetOpponentDeck());
			Core.MainWindow.FlyoutDeck.Header = "Opponent";
			Core.MainWindow.FlyoutDeck.IsOpen = true;
		}

		private void ButtonShowReplay_OnClick(object sender, RoutedEventArgs e)
		{
			if(SelectedGame == null)
				return;
			if(SelectedGame.HasReplayFile)
				ReplayReader.LaunchReplayViewer(SelectedGame.ReplayFile);
		}

		private async void ButtonEdit_OnClick(object sender, RoutedEventArgs e)
		{
			if(SelectedGame == null)
				return;
			var window = Helper.GetParentWindow(this);
			if(window == null)
				return;
			var edited = await window.ShowEditGameDialog(SelectedGame);
			if(edited)
				ConstructedStats.Instance.UpdateConstructedStats();
		}


		private async void ButtonEditNote_OnClick(object sender, RoutedEventArgs e)
		{
			if(SelectedGame == null)
				return;
			var settings = new MessageDialogs.Settings {DefaultText = SelectedGame.Note};
			string newNote;
			if(Config.Instance.StatsInWindow)
				newNote = await Core.Windows.StatsWindow.ShowInputAsync("Note", "", settings);
			else
				newNote = await Core.MainWindow.ShowInputAsync("Note", "", settings);
			if(newNote == null)
				return;
			SelectedGame.Note = newNote;
			DeckStatsList.Save();
			ConstructedStats.Instance.UpdateConstructedStats();
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void DataGridGames_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedGames = null;
			OnPropertyChanged(nameof(RowDetailVisibility));
			OnPropertyChanged(nameof(ButtonMultiMoveEnabled));
			OnPropertyChanged(nameof(MultiSelectPanelVisibility));
		}

		private void ButtonMove_OnClick(object sender, RoutedEventArgs e)
			=> GameStatsHelper.MoveGamesToOtherDeckWithDialog(this, SelectedGame);

		private void ButtonMultiMove_OnClick(object sender, RoutedEventArgs e)
			=> GameStatsHelper.MoveGamesToOtherDeckWithDialog(this, SelectedGames.ToArray());

		private async void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
			=> await GameStatsHelper.DeleteGamesWithDialog(this, SelectedGame);

		private async void ButtonMultiDelete_OnClick(object sender, RoutedEventArgs e)
			=> await GameStatsHelper.DeleteGamesWithDialog(this, SelectedGames.ToArray());
	}
}