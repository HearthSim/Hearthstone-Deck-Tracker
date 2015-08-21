#region

using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.LogReader;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay
{
	/// <summary>
	/// Interaction logic for Player.xaml
	/// </summary>
	public partial class OverlayPlayer
	{
	    private GameV2 _game;
	    private bool _initialized;

		public OverlayPlayer()
		{
		    
		    InitializeComponent();
		}

	    public void Load(GameV2 game)
		{
            _game = game;
            CheckboxHighlightCardsInHand.IsChecked = Config.Instance.HighlightCardsInHand;
			CheckboxRemoveCards.IsChecked = Config.Instance.RemoveCardsFromDeck;
			CheckboxHighlightLastDrawn.IsChecked = Config.Instance.HighlightLastDrawn;
			CheckboxShowPlayerGet.IsChecked = Config.Instance.ShowPlayerGet;
			SliderPlayerOpacity.Value = Config.Instance.PlayerOpacity;
			SliderOverlayPlayerScaling.Value = Config.Instance.OverlayPlayerScaling;
			CheckboxSameScaling.IsChecked = Config.Instance.UseSameScaling;

			ElementSorterPlayer.IsPlayer = true;
			foreach(var itemName in Config.Instance.PanelOrderPlayer)
			{
				switch(itemName)
				{
					case "Deck Title":
						ElementSorterPlayer.AddItem(new ElementSorterItem("Deck Title", Config.Instance.ShowDeckTitle,
						                                                  value => Config.Instance.ShowDeckTitle = value, true));
						break;
					case "Cards":
						ElementSorterPlayer.AddItem(new ElementSorterItem("Cards", !Config.Instance.HidePlayerCards,
						                                                  value => Config.Instance.HidePlayerCards = !value, true));
						break;
					case "Card Counter":
						ElementSorterPlayer.AddItem(new ElementSorterItem("Card Counter", !Config.Instance.HidePlayerCardCount,
						                                                  value => Config.Instance.HidePlayerCardCount = !value, true));
						break;
					case "Fatigue Counter":
						ElementSorterPlayer.AddItem(new ElementSorterItem("Fatigue Counter", !Config.Instance.HidePlayerFatigueCount,
						                                                  value => Config.Instance.HidePlayerFatigueCount = !value, true));
						break;
					case "Draw Chances":
						ElementSorterPlayer.AddItem(new ElementSorterItem("Draw Chances", !Config.Instance.HideDrawChances,
						                                                  value => Config.Instance.HideDrawChances = !value, true));
						break;
					case "Wins":
						ElementSorterPlayer.AddItem(new ElementSorterItem("Wins", Config.Instance.ShowDeckWins,
						                                                  value => Config.Instance.ShowDeckWins = value, true));
						break;
				}
			}
			Helper.MainWindow.Overlay.UpdatePlayerLayout();
			Helper.MainWindow.PlayerWindow.UpdatePlayerLayout();
			_initialized = true;
		}

		private void CheckboxHighlightCardsInHand_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightCardsInHand = true;
			_game.HighlightCardsInHand = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightCardsInHand_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightCardsInHand = false;
			_game.HighlightCardsInHand = false;
			SaveConfig(true);
		}

		private void SliderPlayerOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized)
				return;
			Config.Instance.PlayerOpacity = SliderPlayerOpacity.Value;
			SaveConfig(true);
		}

		private void SliderOverlayPlayerScaling_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized)
				return;
			var scaling = SliderOverlayPlayerScaling.Value;
			Config.Instance.OverlayPlayerScaling = scaling;
			SaveConfig(false);
			Helper.MainWindow.Overlay.UpdateScaling();

			if(Config.Instance.UseSameScaling && Helper.OptionsMain.OptionsOverlayOpponent.SliderOverlayOpponentScaling.Value != scaling)
				Helper.OptionsMain.OptionsOverlayOpponent.SliderOverlayOpponentScaling.Value = scaling;
		}

		private void CheckboxRemoveCards_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized || !_game.IsUsingPremade)
				return;
			Config.Instance.RemoveCardsFromDeck = true;
			SaveConfig(false);
			_game.Reset();
			if(DeckList.Instance.ActiveDeck != null)
				_game.SetPremadeDeck((Deck)DeckList.Instance.ActiveDeck.Clone());
			HsLogReaderV2.Instance.Reset(true);
			Helper.MainWindow.Overlay.Update(true);
		}

		private void CheckboxRemoveCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized || !_game.IsUsingPremade)
				return;
			Config.Instance.RemoveCardsFromDeck = false;
			SaveConfig(false);
			_game.Reset();
			if(DeckList.Instance.ActiveDeck != null)
				_game.SetPremadeDeck((Deck)DeckList.Instance.ActiveDeck.Clone());
			HsLogReaderV2.Instance.Reset(true);
			Helper.MainWindow.Overlay.Update(true);
		}

		private void CheckboxHighlightLastDrawn_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightLastDrawn = true;
			SaveConfig(false);
		}

		private void CheckboxHighlightLastDrawn_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightLastDrawn = false;
			SaveConfig(false);
		}

		private void CheckboxShowPlayerGet_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowPlayerGet = true;
			Config.Save();
			Helper.MainWindow.Overlay.Update(true);
		}

		private void CheckboxShowPlayerGet_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowPlayerGet = false;
			Config.Save();
			Helper.MainWindow.Overlay.Update(true);
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Helper.MainWindow.Overlay.Update(true);
		}

		private void CheckboxSameScaling_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Helper.OptionsMain.OptionsOverlayOpponent.CheckboxSameScaling.IsChecked = true;
			Config.Instance.UseSameScaling = true;
			Config.Save();
		}

		private void CheckboxSameScaling_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Helper.OptionsMain.OptionsOverlayOpponent.CheckboxSameScaling.IsChecked = false;
			Config.Instance.UseSameScaling = false;
			Config.Save();
		}
	}
}