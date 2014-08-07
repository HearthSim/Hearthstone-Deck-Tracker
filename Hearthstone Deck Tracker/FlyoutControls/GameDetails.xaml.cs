using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for GameDetails.xaml
	/// </summary>
	public partial class GameDetails
	{
		private readonly bool _initialized;
		private GameStats _gameStats;

		public GameDetails()
		{
			_initialized = false;
			InitializeComponent();
			LoadConfig();
			_initialized = true;
		}

		public void SetGame(GameStats gameStats)
		{
			_gameStats = gameStats;
			ReloadTreeView();
		}

		private void ReloadTreeView()
		{
			if(_gameStats != null)
			{
				var tvItemSource = new List<TreeViewItem>();
				foreach(var turn in _gameStats.TurnStats)
				{
					var treeViewTurn = new TreeViewItem();
					treeViewTurn.Header = "Turn " + turn.Turn;
					foreach(TreeViewItem item in TreeviewGameDetail.Items)
					{
						if(item.Header.Equals(treeViewTurn.Header))
						{
							treeViewTurn.IsExpanded = item.IsExpanded;
							break;
						}
					}
					foreach(var play in turn.Plays)
					{
						if((play.Type == PlayType.PlayerPlay || play.Type == PlayType.PlayerHandDiscard || play.Type == PlayType.PlayerHeroPower) && !Config.Instance.GameDetails.ShowPlayerPlay
						   || (play.Type == PlayType.PlayerDraw || play.Type == PlayType.PlayerGet || play.Type == PlayType.PlayerDeckDiscard) && !Config.Instance.GameDetails.ShowPlayerDraw
						   || play.Type == PlayType.PlayerMulligan && !Config.Instance.GameDetails.ShowPlayerMulligan
						   || (play.Type == PlayType.OpponentPlay || play.Type == PlayType.OpponentSecretTriggered || play.Type == PlayType.OpponentHandDiscard || play.Type == PlayType.OpponentHeroPower) && !Config.Instance.GameDetails.ShowOpponentPlay
						   || (play.Type == PlayType.OpponentDraw || play.Type == PlayType.OpponentGet || play.Type == PlayType.OpponentBackToHand || play.Type == PlayType.OpponentDeckDiscard) && !Config.Instance.GameDetails.ShowOpponentDraw
						   || play.Type == PlayType.OpponentMulligan && !Config.Instance.GameDetails.ShowOpponentMulligan)
							continue;

						treeViewTurn.Items.Add(new GameHistoryItem(play));
						treeViewTurn.IsExpanded = true;
					}
					tvItemSource.Add(treeViewTurn);
				}
				TreeviewGameDetail.ItemsSource = tvItemSource;
			}
		}

		private void LoadConfig()
		{
			CheckboxPlayerDraw.IsChecked = Config.Instance.GameDetails.ShowPlayerDraw;
			CheckboxOpponentDraw.IsChecked = Config.Instance.GameDetails.ShowOpponentDraw;
			CheckboxPlayerPlay.IsChecked = Config.Instance.GameDetails.ShowPlayerPlay;
			CheckboxOpponentPlay.IsChecked = Config.Instance.GameDetails.ShowOpponentPlay;
			CheckboxPlayerMulligan.IsChecked = Config.Instance.GameDetails.ShowPlayerMulligan;
			CheckboxOpponentMulligan.IsChecked = Config.Instance.GameDetails.ShowOpponentMulligan;
		}

		private void CheckboxPlayerPlay_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowPlayerPlay = true;
			Config.Save();
			ReloadTreeView();
		}

		private void CheckboxPlayerPlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowPlayerPlay = false;
			Config.Save();
			ReloadTreeView();
		}

		private void CheckboxOpponentPlay_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowOpponentPlay = true;
			Config.Save();
			ReloadTreeView();
		}

		private void CheckboxOpponentPlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowOpponentPlay = false;
			Config.Save();
			ReloadTreeView();
		}

		private void CheckboxPlayerDraw_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowPlayerDraw = true;
			Config.Save();
			ReloadTreeView();
		}

		private void CheckboxPlayerDraw_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowPlayerDraw = false;
			Config.Save();
			ReloadTreeView();
		}

		private void CheckboxOpponentDraw_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowOpponentDraw = true;
			Config.Save();
			ReloadTreeView();
		}

		private void CheckboxOpponentDraw_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowOpponentDraw = false;
			Config.Save();
			ReloadTreeView();
		}

		private void CheckboxPlayerMulligan_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowPlayerMulligan = true;
			Config.Save();
			ReloadTreeView();
		}

		private void CheckboxPlayerMulligan_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowPlayerMulligan = false;
			Config.Save();
			ReloadTreeView();
		}

		private void CheckboxOpponentMulligan_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowOpponentMulligan = true;
			Config.Save();
			ReloadTreeView();
		}

		private void CheckboxOpponentMulligan_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowOpponentMulligan = false;
			Config.Save();
			ReloadTreeView();
		}

		private void BtnImportDeck_Click(object sender, RoutedEventArgs e)
		{
			var deck = new Deck();
			deck.Class = _gameStats.OpponentHero;
			foreach(var turn in _gameStats.TurnStats)
			{
				foreach(var play in turn.Plays)
				{
					if(play.Type == PlayType.OpponentPlay || play.Type == PlayType.OpponentDeckDiscard || play.Type == PlayType.OpponentHandDiscard || play.Type == PlayType.OpponentSecretTriggered)
					{
						var card = Game.GetCardFromId(play.CardId);
						if(Game.IsActualCard(card))
						{
							var deckCard = deck.Cards.FirstOrDefault(c => c.Id == card.Id);
							if(deckCard != null)
								deckCard.Count++;
							else deck.Cards.Add(card);
						}
					}
				}
			}
			Helper.MainWindow.SetNewDeck(deck);
			Helper.MainWindow.TabControlTracker.SelectedIndex = 1;
			Helper.MainWindow.FlyoutGameDetails.IsOpen = false;
			Helper.MainWindow.FlyoutDeckStats.IsOpen = false;
		}
	}
}