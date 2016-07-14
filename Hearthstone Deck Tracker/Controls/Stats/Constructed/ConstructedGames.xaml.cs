#region

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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

		public bool ButtonAddGameIsEnabled => !DeckList.Instance.ActiveDeck?.IsArenaDeck ?? false;

		public string ButtonAddGameIsEnabledToolTip
			=> DeckList.Instance.ActiveDeck == null
					? "No active deck" : (DeckList.Instance.ActiveDeck.IsArenaDeck ? "Active deck is an arena deck" : "Deck: " + DeckList.Instance.ActiveDeck.Name);

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
			await window.ShowEditGameDialog(SelectedGame);
			DeckStatsList.Save();
			DefaultDeckStats.Save();
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
			DefaultDeckStats.Save();
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
			if(Core.MainWindow.FlyoutDeck.IsOpen && SelectedGame != null)
				Core.MainWindow.DeckFlyout.SetDeck(SelectedGame.OpponentCards);
		}

		private void ButtonMove_OnClick(object sender, RoutedEventArgs e)
			=> GameStatsHelper.MoveGamesToOtherDeckWithDialog(this, SelectedGame);

		private void ButtonMultiMove_OnClick(object sender, RoutedEventArgs e)
			=> GameStatsHelper.MoveGamesToOtherDeckWithDialog(this, SelectedGames.ToArray());

		private async void ButtonDelete_OnClick(object sender, RoutedEventArgs e)
			=> await GameStatsHelper.DeleteGamesWithDialog(this, SelectedGame);

		private async void ButtonMultiDelete_OnClick(object sender, RoutedEventArgs e)
			=> await GameStatsHelper.DeleteGamesWithDialog(this, SelectedGames.ToArray());

		public void UpdateVisuals()
		{
			OnPropertyChanged(nameof(ReplayIconVisual));
			OnPropertyChanged(nameof(OppDeckIconVisual));
			OnPropertyChanged(nameof(EditIconVisual));
			OnPropertyChanged(nameof(NoteIconVisual));
			OnPropertyChanged(nameof(MoveIconVisual));
			OnPropertyChanged(nameof(DeleteIconVisual));
			OnPropertyChanged(nameof(AddIconVisual));
		}

		public void UpdateAddGameButton()
		{
			OnPropertyChanged(nameof(ButtonAddGameIsEnabled));
			OnPropertyChanged(nameof(ButtonAddGameIsEnabledToolTip));
		}

		public Visual ReplayIconVisual => TryFindResource("appbar_control_play_" + VisualColor) as Visual;
		public Visual OppDeckIconVisual => TryFindResource("appbar_layer_" + VisualColor) as Visual;
		public Visual EditIconVisual => TryFindResource("appbar_edit_" + VisualColor) as Visual;
		public Visual NoteIconVisual => TryFindResource("appbar_edit_box_" + VisualColor) as Visual;
		public Visual MoveIconVisual => TryFindResource("appbar_page_arrow_" + VisualColor) as Visual;
		public Visual DeleteIconVisual => TryFindResource("appbar_delete_" + VisualColor) as Visual;
		public Visual AddIconVisual => TryFindResource("appbar_add_" + VisualColor) as Visual;

		private string VisualColor => Config.Instance.StatsInWindow && Config.Instance.ThemeName != "BaseDark" ? "black" : "white";

		private void ConstructedGames_OnLoaded(object sender, RoutedEventArgs e) => UpdateVisuals();

		private async void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = DeckList.Instance.ActiveDeck;
			if(deck == null || deck.IsArenaDeck)
				return;
			var dialog = Helper.GetParentWindow(Core.StatsOverview)?.ShowAddGameDialog(deck);
			if(dialog != null && await dialog)
				ConstructedStats.Instance.UpdateGames();
		}

		private void ButtonSelectDeck_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = DeckList.Instance.Decks.FirstOrDefault(x => x.DeckId == SelectedGame.DeckId);
			if(deck?.Equals(DeckList.Instance.ActiveDeck) ?? true)
				return;
			Core.MainWindow.SelectDeck(deck, true);
		}
	}
}