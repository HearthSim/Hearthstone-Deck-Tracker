using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
			InitializeComponent();
			_initialized = true;
		}

		public void SetGame(GameStats gameStats)
		{
			_gameStats = gameStats;
			ReloadGameStats();
		}


		private void ReloadGameStats()
		{
			if(_gameStats != null)
			{
				var needSeparator = false;
				DataGridDetails.Items.Clear();
				foreach(var turn in _gameStats.TurnStats)
				{
					if(needSeparator)
					{
						DataGridDetails.Items.Add(new GameDetailItem());
						needSeparator = false;
					}
					foreach(var play in turn.Plays.Where(play => play != null))
					{
						if((play.Type == PlayType.PlayerPlay || play.Type == PlayType.PlayerHandDiscard || play.Type == PlayType.PlayerHeroPower) && !Config.Instance.GameDetails.ShowPlayerPlay
						   || (play.Type == PlayType.PlayerDraw || play.Type == PlayType.PlayerGet || play.Type == PlayType.PlayerDeckDiscard) && !Config.Instance.GameDetails.ShowPlayerDraw
						   || play.Type == PlayType.PlayerMulligan && !Config.Instance.GameDetails.ShowPlayerMulligan
						   || (play.Type == PlayType.OpponentPlay || play.Type == PlayType.OpponentSecretTriggered || play.Type == PlayType.OpponentHandDiscard || play.Type == PlayType.OpponentHeroPower) && !Config.Instance.GameDetails.ShowOpponentPlay
						   || (play.Type == PlayType.OpponentDraw || play.Type == PlayType.OpponentGet || play.Type == PlayType.OpponentBackToHand || play.Type == PlayType.OpponentDeckDiscard) && !Config.Instance.GameDetails.ShowOpponentDraw
						   || play.Type == PlayType.OpponentMulligan && !Config.Instance.GameDetails.ShowOpponentMulligan)
							continue;
						needSeparator = true;
						DataGridDetails.Items.Add(new GameDetailItem(play, turn.Turn));
					}
				}
			}
		}

		public void LoadConfig()
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
			ReloadGameStats();
		}

		private void CheckboxPlayerPlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowPlayerPlay = false;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxOpponentPlay_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowOpponentPlay = true;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxOpponentPlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowOpponentPlay = false;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxPlayerDraw_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowPlayerDraw = true;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxPlayerDraw_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowPlayerDraw = false;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxOpponentDraw_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowOpponentDraw = true;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxOpponentDraw_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowOpponentDraw = false;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxPlayerMulligan_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowPlayerMulligan = true;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxPlayerMulligan_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowPlayerMulligan = false;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxOpponentMulligan_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowOpponentMulligan = true;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxOpponentMulligan_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized) return;
			Config.Instance.GameDetails.ShowOpponentMulligan = false;
			Config.Save();
			ReloadGameStats();
		}

		private void BtnImportDeck_Click(object sender, RoutedEventArgs e)
		{
			var ignoreCards = new List<Card>();
			var deck = new Deck {Class = _gameStats.OpponentHero};
			foreach(var turn in _gameStats.TurnStats)
			{
				foreach(var play in turn.Plays)
				{
					if(play.Type == PlayType.OpponentPlay || play.Type == PlayType.OpponentDeckDiscard || play.Type == PlayType.OpponentHandDiscard || play.Type == PlayType.OpponentSecretTriggered)
					{
						var card = Game.GetCardFromId(play.CardId);
						if(Game.IsActualCard(card))
						{
							if(ignoreCards.Contains(card))
							{
								ignoreCards.Remove(card);
								continue;
							}
							var deckCard = deck.Cards.FirstOrDefault(c => c.Id == card.Id);
							if(deckCard != null)
								deckCard.Count++;
							else deck.Cards.Add(card);
						}
					}
					else if(play.Type == PlayType.OpponentBackToHand)
					{
						var card = Game.GetCardFromId(play.CardId);
						if(Game.IsActualCard(card))
							ignoreCards.Add(card);
					}
				}
			}
			Helper.MainWindow.SetNewDeck(deck);
			Helper.MainWindow.TabControlTracker.SelectedIndex = 1;
			Helper.MainWindow.FlyoutGameDetails.IsOpen = false;
			Helper.MainWindow.FlyoutDeckStats.IsOpen = false;
		}

		public class GameDetailItem
		{
			public GameDetailItem(TurnStats.Play play, int turn)
			{
				Turn = turn.ToString();
				Player = play.Type.ToString().StartsWith("Player") ? "Player" : "Opponent";
				Action = play.Type.ToString().Replace("Player", string.Empty).Replace("Opponent", string.Empty);
				Card = Game.GetCardFromId(play.CardId);

				if(play.Type == PlayType.PlayerHandDiscard || play.Type == PlayType.OpponentHandDiscard && (Card != null && Card.Type == "Spell"))
					Action = "Play/Discard";
			}

			public GameDetailItem()
			{
			}

			public string Turn { get; set; }
			public string Player { get; set; }
			public string Action { get; set; }
			public Card Card { get; set; }
		}
	}
}