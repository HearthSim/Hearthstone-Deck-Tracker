#region

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using static Hearthstone_Deck_Tracker.Enums.PlayType;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for GameDetails.xaml
	/// </summary>
	public partial class GameDetails
	{
		private GameStats _gameStats;
		private bool _initialized;

		public GameDetails()
		{
			InitializeComponent();
		}

		public void SetGame(GameStats gameStats)
		{
			_gameStats = gameStats;
			ReloadGameStats();
		}

		private void ReloadGameStats()
		{
			if(_gameStats == null)
				return;
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
					if((play.Type == PlayerPlay || play.Type == PlayerHandDiscard || play.Type == PlayerHeroPower
						|| play.Type == PlayerSecretPlayed) && !Config.Instance.GameDetails.ShowPlayerPlay
					   || (play.Type == PlayerDraw || play.Type == PlayerGet || play.Type == PlayerDeckDiscard)
					   && !Config.Instance.GameDetails.ShowPlayerDraw
					   || play.Type == PlayerMulligan && !Config.Instance.GameDetails.ShowPlayerMulligan
					   || (play.Type == OpponentPlay || play.Type == OpponentSecretTriggered
						   || play.Type == OpponentHandDiscard || play.Type == OpponentHeroPower
						   || play.Type == OpponentSecretPlayed) && !Config.Instance.GameDetails.ShowOpponentPlay
					   || (play.Type == OpponentDraw || play.Type == OpponentGet || play.Type == OpponentBackToHand
						   || play.Type == OpponentDeckDiscard) && !Config.Instance.GameDetails.ShowOpponentDraw
					   || play.Type == OpponentMulligan && !Config.Instance.GameDetails.ShowOpponentMulligan)
						continue;
					needSeparator = true;
					DataGridDetails.Items.Add(new GameDetailItem(play, turn.Turn));
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
			_initialized = true;
		}

		private void CheckboxPlayerPlay_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameDetails.ShowPlayerPlay = true;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxPlayerPlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameDetails.ShowPlayerPlay = false;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxOpponentPlay_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameDetails.ShowOpponentPlay = true;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxOpponentPlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameDetails.ShowOpponentPlay = false;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxPlayerDraw_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameDetails.ShowPlayerDraw = true;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxPlayerDraw_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameDetails.ShowPlayerDraw = false;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxOpponentDraw_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameDetails.ShowOpponentDraw = true;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxOpponentDraw_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameDetails.ShowOpponentDraw = false;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxPlayerMulligan_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameDetails.ShowPlayerMulligan = true;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxPlayerMulligan_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameDetails.ShowPlayerMulligan = false;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxOpponentMulligan_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameDetails.ShowOpponentMulligan = true;
			Config.Save();
			ReloadGameStats();
		}

		private void CheckboxOpponentMulligan_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameDetails.ShowOpponentMulligan = false;
			Config.Save();
			ReloadGameStats();
		}

		private void BtnImportDeck_Click(object sender, RoutedEventArgs e)
		{
			var ignoreCards = new List<Card>();
			var deck = new Deck {Class = _gameStats.OpponentHero};
			foreach(var play in _gameStats.TurnStats.SelectMany(turn => turn.Plays))
			{
				switch(play.Type)
				{
					case OpponentPlay:
					case OpponentDeckDiscard:
					case OpponentHandDiscard:
					case OpponentSecretTriggered:
					{
						var card = Database.GetCardFromId(play.CardId);
						if(!Database.IsActualCard(card))
							continue;
						if(ignoreCards.Remove(card))
							continue;
						var deckCard = deck.Cards.FirstOrDefault(c => c.Id == card.Id);
						if(deckCard != null)
							deckCard.Count++;
						else
							deck.Cards.Add(card);
					}
						break;
					case OpponentBackToHand:
					{
						var card = Database.GetCardFromId(play.CardId);
						if(Database.IsActualCard(card))
							ignoreCards.Add(card);
					}
						break;
				}
			}
			Core.MainWindow.SetNewDeck(deck);
			Core.MainWindow.FlyoutGameDetails.IsOpen = false;
			Core.MainWindow.FlyoutDeckStats.IsOpen = false;
		}
	}
}