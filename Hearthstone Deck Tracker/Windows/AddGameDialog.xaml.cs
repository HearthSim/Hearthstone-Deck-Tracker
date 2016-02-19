#region

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	/// <summary>
	/// Interaction logic for AddGameDialog.xaml
	/// </summary>
	public partial class AddGameDialog : CustomDialog
	{
		private readonly Deck _deck;
		private readonly bool _editing;
		private readonly GameStats _game;
		private readonly TaskCompletionSource<GameStats> _tcs;

		public AddGameDialog(Deck deck)
		{
			InitializeComponent();
			_tcs = new TaskCompletionSource<GameStats>();
			_editing = false;
			var lastGame = deck.DeckStats.Games.LastOrDefault();
			if(deck.IsArenaDeck)
			{
				ComboBoxMode.SelectedItem = GameMode.Arena;
				ComboBoxMode.IsEnabled = false;
				TextBoxRank.IsEnabled = false;
			}
			else
			{
				ComboBoxMode.IsEnabled = true;
				TextBoxRank.IsEnabled = true;
				if(lastGame != null)
				{
					ComboBoxMode.SelectedItem = lastGame.GameMode;
					if(lastGame.GameMode == GameMode.Ranked)
						TextBoxRank.Text = lastGame.Rank.ToString();
				}
			}
			if(lastGame != null)
			{
				TextBoxPlayerName.Text = lastGame.PlayerName;
				if(lastGame.Region != Region.UNKNOWN)
					ComboBoxRegion.SelectedItem = lastGame.Region;
			}
			_deck = deck;
			_game = new GameStats();
			BtnSave.Content = "add game";
			Title = "Add new game";
		}

		public AddGameDialog(GameStats game)
		{
			InitializeComponent();
			_tcs = new TaskCompletionSource<GameStats>();
			_editing = true;
			_game = game;
			if(game == null)
				return;
			ComboBoxResult.SelectedItem = game.Result;
			HeroClass heroClass;
			if(Enum.TryParse(game.OpponentHero, out heroClass))
				ComboBoxOpponent.SelectedItem = heroClass;
			ComboBoxMode.SelectedItem = game.GameMode;
			ComboBoxRegion.SelectedItem = game.Region;
			if(game.GameMode == GameMode.Ranked)
				TextBoxRank.Text = game.Rank.ToString();
			TextBoxRank.IsEnabled = game.GameMode == GameMode.Ranked;
			ComboBoxCoin.SelectedItem = game.Coin ? YesNo.Yes : YesNo.No;
			ComboBoxConceded.SelectedItem = game.WasConceded ? YesNo.Yes : YesNo.No;
			TextBoxTurns.Text = game.Turns.ToString();
			TextBoxDuration.Text = game.Duration;
			TextBoxDuration.IsEnabled = false;
			TextBoxNote.Text = game.Note;
			TextBoxOppName.Text = game.OpponentName;
			TextBoxPlayerName.Text = game.PlayerName;
			BtnSave.Content = "save";
			Title = "Edit game";
		}

		private void BtnSave_OnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				int duration;
				int.TryParse(TextBoxDuration.Text, out duration);
				int rank;
				int.TryParse(TextBoxRank.Text, out rank);
				int turns;
				int.TryParse(TextBoxTurns.Text, out turns);
				if(!_editing)
				{
					_game.StartTime = DateTime.Now;
					_game.GameId = Guid.NewGuid();
					_game.EndTime = DateTime.Now.AddMinutes(duration);
					_game.PlayerHero = _deck.Class;
					_game.PlayerDeckVersion = _deck.SelectedVersion;
					_game.VerifiedHeroes = true;
				}
				_game.Result = (GameResult)ComboBoxResult.SelectedItem;
				_game.GameMode = (GameMode)ComboBoxMode.SelectedItem;
				_game.OpponentHero = ComboBoxOpponent.SelectedValue.ToString();
				_game.Coin = (YesNo)ComboBoxCoin.SelectedValue == YesNo.Yes;
				_game.Rank = rank;
				_game.Note = TextBoxNote.Text;
				_game.OpponentName = TextBoxOppName.Text;
				_game.PlayerName = TextBoxPlayerName.Text;
				_game.Turns = turns;
				_game.WasConceded = (YesNo)ComboBoxConceded.SelectedValue == YesNo.Yes;
				_game.Region = (Region)ComboBoxRegion.SelectedItem;
				_tcs.SetResult(_game);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				_tcs.SetResult(null);
			}
		}

		internal Task<GameStats> WaitForButtonPressAsync() => _tcs.Task;

		private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if(!char.IsDigit(e.Text, e.Text.Length - 1))
				e.Handled = true;
		}

		private void ComboBoxMode_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(IsLoaded)
				TextBoxRank.IsEnabled = e.AddedItems.Contains(GameMode.Ranked);
		}

		private void BtnCancel_OnClick(object sender, RoutedEventArgs e) => _tcs.SetResult(null);
	}
}