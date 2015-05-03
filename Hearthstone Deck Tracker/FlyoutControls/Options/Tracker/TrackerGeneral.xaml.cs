#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
			CheckboxHideManaCurveMyDecks.IsChecked = Config.Instance.ManaCurveMyDecks;
			CheckboxTrackerCardToolTips.IsChecked = Config.Instance.TrackerCardToolTips;
			CheckboxFullTextSearch.IsChecked = Config.Instance.UseFullTextSearch;
			CheckboxAutoSelectDeck.IsEnabled = Config.Instance.AutoDeckDetection;
			CheckboxAutoSelectDeck.IsChecked = Config.Instance.AutoSelectDetectedDeck;
			CheckboxBringHsToForegorund.IsChecked = Config.Instance.BringHsToForeground;
			CheckboxFlashHs.IsChecked = Config.Instance.FlashHsOnTurnStart;
			CheckboxNoteDialog.IsChecked = Config.Instance.ShowNoteDialogAfterGame;
			CheckboxTimerAlert.IsChecked = Config.Instance.TimerAlert;
			CheckboxNoteDialogDelayed.IsChecked = Config.Instance.NoteDialogDelayed;
			CheckboxNoteDialogDelayed.IsEnabled = Config.Instance.ShowNoteDialogAfterGame;
			CheckboxCardFrameRarity.IsChecked = Config.Instance.RarityCardFrames;
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
			Helper.MainWindow.ManaCurveMyDecks.Visibility = Visibility.Visible;
			Config.Save();
		}

		private void CheckboxManaCurveMyDecks_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ManaCurveMyDecks = false;
			Helper.MainWindow.ManaCurveMyDecks.Visibility = Visibility.Collapsed;
			Config.Save();
		}

		private async void CheckboxTrackerCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized)
				return;
			Config.Instance.TrackerCardToolTips = true;
			Config.Save();
			await Helper.MainWindow.Restart();
		}

		private async void CheckboxTrackerCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized)
				return;
			Config.Instance.TrackerCardToolTips = false;
			Config.Save();
			await Helper.MainWindow.Restart();
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

		private void CheckboxNoteDialog_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowNoteDialogAfterGame = true;
			CheckboxNoteDialogDelayed.IsEnabled = true;
			Config.Save();
		}

		private void CheckboxNoteDialog_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowNoteDialogAfterGame = false;
			CheckboxNoteDialogDelayed.IsEnabled = false;
			Config.Save();
		}

		private void CheckboxNoteDialogDelay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.NoteDialogDelayed = false;
			Config.Save();
		}

		private void CheckboxNoteDialogDelay_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.NoteDialogDelayed = true;
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
	}
}