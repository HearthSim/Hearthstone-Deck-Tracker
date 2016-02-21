#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for TrackerGeneral.xaml
	/// </summary>
	public partial class TrackerGeneral : UserControl
	{
		private bool _initialized;

		public TrackerGeneral()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckBoxAutoUse.IsChecked = Config.Instance.AutoUseDeck;
			CheckboxHideManaCurveMyDecks.IsChecked = Config.Instance.ManaCurveMyDecks;
			CheckboxTrackerCardToolTips.IsChecked = Config.Instance.TrackerCardToolTips;
			CheckboxFullTextSearch.IsChecked = Config.Instance.UseFullTextSearch;
			CheckboxAutoSelectDeck.IsChecked = Config.Instance.AutoSelectDetectedDeck;
			CheckboxBringHsToForegorund.IsChecked = Config.Instance.BringHsToForeground;
			CheckboxFlashHs.IsChecked = Config.Instance.FlashHsOnTurnStart;
			CheckboxTimerAlert.IsChecked = Config.Instance.TimerAlert;
			CheckboxCardFrameRarity.IsChecked = Config.Instance.RarityCardFrames;
			CheckboxCardGemRarity.IsChecked = Config.Instance.RarityCardGems;
			CheckboxTurnTime.IsChecked = Config.Instance.TimerTurnTime == 75;
			CheckboxSpectatorUseNoDeck.IsChecked = Config.Instance.SpectatorUseNoDeck;
			CheckBoxClassCardsFirst.IsChecked = Config.Instance.CardSortingClassFirst;
			TextboxTimerAlert.Text = Config.Instance.TimerAlertSeconds.ToString();
			_initialized = true;
		}

		private void CheckboxAutoSelectDeck_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoSelectDetectedDeck = true;
			Config.Save();
		}

		private void CheckboxAutoSelectDeck_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoSelectDetectedDeck = false;
			Config.Save();
		}

		private void CheckboxManaCurveMyDecks_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ManaCurveMyDecks = true;
			Core.MainWindow.ManaCurveMyDecks.Visibility = Visibility.Visible;
			Config.Save();
		}

		private void CheckboxManaCurveMyDecks_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ManaCurveMyDecks = false;
			Core.MainWindow.ManaCurveMyDecks.Visibility = Visibility.Collapsed;
			Config.Save();
		}

		private void CheckboxTrackerCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized)
				return;
			Config.Instance.TrackerCardToolTips = true;
			Config.Save();
			Core.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.").Forget();
		}

		private void CheckboxTrackerCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized)
				return;
			Config.Instance.TrackerCardToolTips = false;
			Config.Save();
			Core.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.").Forget();
		}

		private void CheckboxFullTextSearch_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseFullTextSearch = true;
			Config.Save();
		}

		private void CheckboxFullTextSearch_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseFullTextSearch = false;
			Config.Save();
		}

		private void TextboxTimerAlert_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if(!char.IsDigit(e.Text, e.Text.Length - 1))
				e.Handled = true;
		}

		private void TextboxTimerAlert_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(!_initialized || CheckboxTimerAlert.IsChecked != true)
				return;
			int mTimerAlertValue;
			if(int.TryParse(TextboxTimerAlert.Text, out mTimerAlertValue))
			{
				if(mTimerAlertValue < 0)
				{
					TextboxTimerAlert.Text = "0";
					mTimerAlertValue = 0;
				}

				if(mTimerAlertValue > 90)
				{
					TextboxTimerAlert.Text = "90";
					mTimerAlertValue = 90;
				}

				Config.Instance.TimerAlertSeconds = mTimerAlertValue;
				Config.Save();
			}
		}

		private void CheckboxBringHsToForegorund_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.BringHsToForeground = true;
			Config.Save();
		}

		private void CheckboxBringHsToForegorund_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.BringHsToForeground = false;
			Config.Save();
		}

		private void CheckboxFlashHs_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.FlashHsOnTurnStart = true;
			Config.Save();
		}

		private void CheckboxFlashHs_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.FlashHsOnTurnStart = false;
			Config.Save();
		}

		private void CheckboxTimerAlert_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerAlert = true;
			TextboxTimerAlert.IsEnabled = true;
			Config.Save();
		}

		private void CheckboxTimerAlert_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerAlert = false;
			TextboxTimerAlert.IsEnabled = false;
			Config.Save();
		}

		private void CheckboxCardFrameRarity_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RarityCardFrames = true;
			Config.Save();
		}

		private void CheckboxCardFrameRarity_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RarityCardFrames = false;
			Config.Save();
		}

		private void CheckboxCardGemRarity_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RarityCardGems = true;
			Config.Save();
		}

		private void CheckboxCardGemRarity_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RarityCardGems = false;
			Config.Save();
		}

		private void CheckBoxAutoUse_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoUseDeck = true;
			Config.Save();
			Core.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.").Forget();
		}

		private void CheckBoxAutoUse_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoUseDeck = false;
			Config.Save();
			Core.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this setting to take effect.").Forget();
		}

		private void CheckboxTurnTime_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerTurnTime = 75;
			Config.Save();
			TurnTimer.Instance.SetTurnTime(75);
		}

		private void CheckboxTurnTime_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerTurnTime = 90;
			Config.Save();
			TurnTimer.Instance.SetTurnTime(90);
		}

		private void CheckboxSpectatorUseNoDeck_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SpectatorUseNoDeck = true;
			Config.Save();
		}

		private void CheckboxSpectatorUseNoDeck_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.SpectatorUseNoDeck = false;
			Config.Save();
		}

		private void CheckBoxClassCardsFirst_Checked(object sender, RoutedEventArgs e) => Core.MainWindow.SortClassCardsFirst(true);

		private void CheckBoxClassCardsFirst_Unchecked(object sender, RoutedEventArgs e) => Core.MainWindow.SortClassCardsFirst(false);

	}
}