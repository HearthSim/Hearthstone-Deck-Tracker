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
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;
using static System.Windows.Visibility;
using static Hearthstone_Deck_Tracker.Enums.GameMode;
using static Hearthstone_Deck_Tracker.Enums.YesNo;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	/// <summary>
	/// Interaction logic for AddGameDialog.xaml
	/// </summary>
	public partial class AddGameDialog : CustomDialog
	{
		private const string LocAddGame = "AddGameDialog_Button_AddGame";
		private const string LocSaveGame = "AddGameDialog_Button_SaveGame";
		private const string LocEditTitle = "AddGameDialog_Title_Edit";
		private readonly Deck _deck;
		private readonly bool _editing;
		private readonly GameStats _game;
		private readonly TaskCompletionSource<GameStats> _tcs;

		private AddGameDialog()
		{
			InitializeComponent();
			ComboBoxResult.ItemsSource = new[] { GameResult.Win, GameResult.Loss, GameResult.Draw};
			ComboBoxOpponent.ItemsSource = Enum.GetValues(typeof(HeroClass));
			ComboBoxMode.ItemsSource = new[] {Ranked, Casual, Arena, Brawl, Friendly, Practice};
			ComboBoxFormat.ItemsSource = new[] {Format.Standard, Format.Wild};
			ComboBoxRegion.ItemsSource = new[] { Region.US, Region.EU, Region.ASIA, Region.CHINA};
			ComboBoxCoin.ItemsSource = Enum.GetValues(typeof(YesNo));
			ComboBoxConceded.ItemsSource = Enum.GetValues(typeof(YesNo));
			_tcs = new TaskCompletionSource<GameStats>();
		}

		public AddGameDialog(Deck deck) : this()
		{
			_editing = false;
			var lastGame = deck.DeckStats.Games.LastOrDefault();
			if(deck.IsArenaDeck)
			{
				ComboBoxMode.SelectedItem = Arena;
				ComboBoxMode.IsEnabled = false;
			}
			else
			{
				ComboBoxMode.IsEnabled = true;
				TextBoxRank.IsEnabled = true;
				TextBoxLegendRank.IsEnabled = true;
				if(lastGame != null)
				{
					ComboBoxFormat.SelectedItem = lastGame.Format;
					ComboBoxMode.SelectedItem = lastGame.GameMode;
					if(lastGame.GameMode == Ranked)
					{
						TextBoxRank.Text = lastGame.Rank.ToString();
						TextBoxLegendRank.Text = lastGame.LegendRank.ToString();
					}
				}
			}
			if(lastGame != null)
			{
				PanelRank.Visibility = PanelLegendRank.Visibility = lastGame.GameMode == Ranked ? Visible : Collapsed;
				PanelFormat.Visibility = lastGame.GameMode == Ranked || lastGame.GameMode == Casual ? Visible : Collapsed;
				TextBoxPlayerName.Text = lastGame.PlayerName;
				if(lastGame.Region != Region.UNKNOWN)
					ComboBoxRegion.SelectedItem = lastGame.Region;
			}
			_deck = deck;
			_game = new GameStats();
			BtnSave.Content = LocUtil.Get(LocAddGame);
			Title = _deck.Name;
		}

		public AddGameDialog(GameStats game) : this()
		{
			_editing = true;
			_game = game;
			if(game == null)
				return;
			ComboBoxResult.SelectedItem = game.Result;
			if(!string.IsNullOrWhiteSpace(game.OpponentHero) && Enum.TryParse(game.OpponentHero, out HeroClass heroClass))
				ComboBoxOpponent.SelectedItem = heroClass;
			ComboBoxMode.SelectedItem = game.GameMode;
			ComboBoxFormat.SelectedItem = game.Format;
			ComboBoxRegion.SelectedItem = game.Region;
			if(game.GameMode == Ranked)
			{
				TextBoxRank.Text = game.Rank.ToString();
				TextBoxLegendRank.Text = game.LegendRank.ToString();
			}
			PanelRank.Visibility = PanelLegendRank.Visibility = game.GameMode == Ranked ? Visible : Collapsed;
			PanelFormat.Visibility = game.GameMode == Ranked || game.GameMode == Casual ? Visible : Collapsed;
			ComboBoxCoin.SelectedItem = game.Coin ? Yes : No;
			ComboBoxConceded.SelectedItem = game.WasConceded ? Yes : No;
			TextBoxTurns.Text = game.Turns.ToString();
			TextBoxDuration.Text = game.Duration;
			TextBoxDuration.IsEnabled = false;
			TextBoxNote.Text = game.Note;
			TextBoxOppName.Text = game.OpponentName;
			TextBoxPlayerName.Text = game.PlayerName;
			BtnSave.Content = LocUtil.Get(LocSaveGame);
			Title = LocUtil.Get(LocEditTitle);
		}

		private void BtnSave_OnClick(object sender, RoutedEventArgs e)
		{
			BtnSave.IsEnabled = false;
			try
			{
				int.TryParse(TextBoxDuration.Text, out var duration);
				int.TryParse(TextBoxRank.Text, out var rank);
				int.TryParse(TextBoxLegendRank.Text, out var legendRank);
				int.TryParse(TextBoxTurns.Text, out var turns);
				if(!_editing)
				{
					_game.StartTime = DateTime.Now;
					_game.GameId = Guid.NewGuid();
					_game.EndTime = DateTime.Now.AddMinutes(duration);
					_game.PlayerHero = _deck.Class;
					_game.PlayerDeckVersion = _deck.SelectedVersion;
				}
				_game.Result = (GameResult)ComboBoxResult.SelectedItem;
				_game.GameMode = (GameMode)ComboBoxMode.SelectedItem;
				_game.OpponentHero = ComboBoxOpponent.SelectedValue.ToString();
				_game.Coin = (YesNo)ComboBoxCoin.SelectedValue == Yes;
				_game.Rank = rank;
				_game.LegendRank = legendRank;
				_game.Note = TextBoxNote.Text;
				_game.OpponentName = TextBoxOppName.Text;
				_game.PlayerName = TextBoxPlayerName.Text;
				_game.Turns = turns;
				_game.WasConceded = (YesNo)ComboBoxConceded.SelectedValue == Yes;
				_game.Region = (Region)ComboBoxRegion.SelectedItem;
				_game.HearthstoneBuild = Helper.GetHearthstoneBuild();
				if(_game.GameMode == Casual || _game.GameMode == Ranked)
					_game.Format = (Format)ComboBoxFormat.SelectedItem;
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
			{
				var ranked = e.AddedItems.Contains(Ranked);
				PanelRank.Visibility = PanelLegendRank.Visibility= ranked ? Visible : Collapsed;

				var format = ranked || e.AddedItems.Contains(Casual);
				PanelFormat.Visibility = format ? Visible : Collapsed;
			}
		}

		private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
		{
			BtnCancel.IsEnabled = false;
			_tcs.SetResult(null);
		}
	}
}
