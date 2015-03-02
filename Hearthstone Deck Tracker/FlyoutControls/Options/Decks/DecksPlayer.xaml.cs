#region

using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Decks
{
	/// <summary>
	/// Interaction logic for Player.xaml
	/// </summary>
	public partial class DecksPlayer
	{
		private bool _initialized;

		public DecksPlayer()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckboxHighlightCardsInHand.IsChecked = Config.Instance.HighlightCardsInHand;
			CheckboxRemoveCards.IsChecked = Config.Instance.RemoveCardsFromDeck;
			CheckboxHighlightLastDrawn.IsChecked = Config.Instance.HighlightLastDrawn;
			CheckboxShowPlayerGet.IsChecked = Config.Instance.ShowPlayerGet;
			SliderPlayerOpacity.Value = Config.Instance.PlayerOpacity;
			SliderOverlayPlayerScaling.Value = Config.Instance.OverlayPlayerScaling;

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
			Game.HighlightCardsInHand = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightCardsInHand_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightCardsInHand = false;
			Game.HighlightCardsInHand = false;
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

			if(Config.Instance.UseSameScaling && Helper.OptionsMain.OptionsDecksOpponent.SliderOverlayOpponentScaling.Value != scaling)
				Helper.OptionsMain.OptionsDecksOpponent.SliderOverlayOpponentScaling.Value = scaling;
		}

		private void CheckboxRemoveCards_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized || !Game.IsUsingPremade)
				return;
			Config.Instance.RemoveCardsFromDeck = true;
			SaveConfig(false);
			Game.Reset();
			if(DeckList.Instance.ActiveDeck != null)
				Game.SetPremadeDeck((Deck)DeckList.Instance.ActiveDeck.Clone());
			HsLogReader.Instance.Reset(true);
			Helper.MainWindow.Overlay.Update(true);
		}

		private void CheckboxRemoveCards_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized || !Game.IsUsingPremade)
				return;
			Config.Instance.RemoveCardsFromDeck = false;
			SaveConfig(false);
			Game.Reset();
			if(DeckList.Instance.ActiveDeck != null)
				Game.SetPremadeDeck((Deck)DeckList.Instance.ActiveDeck.Clone());
			HsLogReader.Instance.Reset(true);
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
			Helper.MainWindow.Overlay.Update(true);
		}

		private void CheckboxShowPlayerGet_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowPlayerGet = false;
			Helper.MainWindow.Overlay.Update(true);
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Helper.MainWindow.Overlay.Update(true);
		}
	}
}