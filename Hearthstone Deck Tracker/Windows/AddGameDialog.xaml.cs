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
		private readonly TaskCompletionSource<GameStats> _tcs;

		public AddGameDialog(Deck deck)
		{
			InitializeComponent();
			_tcs = new TaskCompletionSource<GameStats>();
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
				var lastGame = deck.DeckStats.Games.LastOrDefault();
				if(lastGame != null)
				{
					ComboBoxMode.SelectedItem = lastGame.GameMode;
					if(lastGame.GameMode == GameMode.Ranked)
						TextBoxRank.Text = lastGame.Rank.ToString();
				}
			}

			_deck = deck;
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
				var gs = new GameStats
				{
					Result = (GameResult)ComboBoxResult.SelectedItem,
					GameMode = (GameMode)ComboBoxMode.SelectedItem,
					OpponentHero = ComboBoxOpponent.SelectedValue.ToString(),
					StartTime = DateTime.Now,
					GameId = Guid.NewGuid(),
					Coin = (YesNo)ComboBoxCoin.SelectedValue == YesNo.Yes,
					EndTime = DateTime.Now.AddMinutes(duration),
					Rank = rank,
					Note = TextBoxNote.Text,
					OpponentName = TextBoxOppName.Text,
					PlayerHero = _deck.Class,
					Turns = turns,
					WasConceded = (YesNo)ComboBoxConceded.SelectedValue == YesNo.Yes,
					VerifiedHeroes = true,
					PlayerDeckVersion = _deck.SelectedVersion
				};
				_tcs.SetResult(gs);
			}
			catch(Exception ex)
			{
				Logger.WriteLine(ex.ToString(), "AddGameDialog");
				_tcs.SetResult(null);
			}
		}

		internal Task<GameStats> WaitForButtonPressAsync()
		{
			return _tcs.Task;
		}

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

		private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
		{
			_tcs.SetResult(null);
		}
	}
}