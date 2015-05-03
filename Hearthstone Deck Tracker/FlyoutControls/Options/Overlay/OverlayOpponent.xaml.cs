#region

using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay
{
	/// <summary>
	/// Interaction logic for Opponent.xaml
	/// </summary>
	public partial class OverlayOpponent
	{
		private bool _initialized;

		public OverlayOpponent()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckboxHighlightDiscarded.IsChecked = Config.Instance.HighlightDiscarded;
			SliderOpponentOpacity.Value = Config.Instance.OpponentOpacity;
			SliderOverlayOpponentScaling.Value = Config.Instance.OverlayOpponentScaling;
			CheckboxSameScaling.IsChecked = Config.Instance.UseSameScaling;

			ElementSorterOpponent.IsPlayer = false;
			foreach(var itemName in Config.Instance.PanelOrderOpponent)
			{
				switch(itemName)
				{
					case "Cards":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Cards", !Config.Instance.HideOpponentCards,
						                                                    value => Config.Instance.HideOpponentCards = !value, false));
						break;
					case "Card Counter":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Card Counter", !Config.Instance.HideOpponentCardCount,
						                                                    value => Config.Instance.HideOpponentCardCount = !value, false));
						break;
					case "Fatigue Counter":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Fatigue Counter", !Config.Instance.HideOpponentFatigueCount,
						                                                    value => Config.Instance.HideOpponentFatigueCount = !value, false));
						break;
					case "Draw Chances":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Draw Chances", !Config.Instance.HideOpponentDrawChances,
						                                                    value => Config.Instance.HideOpponentDrawChances = !value, false));
						break;
					case "Win Rate":
						ElementSorterOpponent.AddItem(new ElementSorterItem("Win Rate", Config.Instance.ShowWinRateAgainst,
						                                                    value => Config.Instance.ShowWinRateAgainst = value, false));
						break;
				}
			}
			Helper.MainWindow.Overlay.UpdateOpponentLayout();
			Helper.MainWindow.OpponentWindow.UpdateOpponentLayout();
			_initialized = true;
		}

		private void CheckboxHighlightDiscarded_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightDiscarded = true;
			Game.HighlightDiscarded = true;
			SaveConfig(true);
		}

		private void CheckboxHighlightDiscarded_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HighlightDiscarded = false;
			Game.HighlightDiscarded = false;
			SaveConfig(true);
		}

		private void SliderOverlayOpponentScaling_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized)
				return;
			var scaling = SliderOverlayOpponentScaling.Value;
			Config.Instance.OverlayOpponentScaling = scaling;
			SaveConfig(false);
			Helper.MainWindow.Overlay.UpdateScaling();

			if(Config.Instance.UseSameScaling && Helper.OptionsMain.OptionsOverlayPlayer.SliderOverlayPlayerScaling.Value != scaling)
				Helper.OptionsMain.OptionsOverlayPlayer.SliderOverlayPlayerScaling.Value = scaling;
		}

		private void SliderOpponentOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized)
				return;
			Config.Instance.OpponentOpacity = SliderOpponentOpacity.Value;
			SaveConfig(true);
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
			Helper.OptionsMain.OptionsOverlayPlayer.CheckboxSameScaling.IsChecked = true;
			Config.Instance.UseSameScaling = true;
			Config.Save();
		}

		private void CheckboxSameScaling_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Helper.OptionsMain.OptionsOverlayPlayer.CheckboxSameScaling.IsChecked = false;
			Config.Instance.UseSameScaling = false;
			Config.Save();
		}
	}
}