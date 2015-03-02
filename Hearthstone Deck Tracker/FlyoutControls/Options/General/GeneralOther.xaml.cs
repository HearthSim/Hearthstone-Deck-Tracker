#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.General
{
	/// <summary>
	/// Interaction logic for Other.xaml
	/// </summary>
	public partial class GeneralOther
	{
		private bool _initialized;

		public GeneralOther()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckboxKeepDecksVisible.IsChecked = Config.Instance.KeepDecksVisible;
			CheckboxAutoSelectDeck.IsEnabled = Config.Instance.AutoDeckDetection;
			CheckboxAutoSelectDeck.IsChecked = Config.Instance.AutoSelectDetectedDeck;
			CheckboxBringHsToForegorund.IsChecked = Config.Instance.BringHsToForeground;
			CheckboxFlashHs.IsChecked = Config.Instance.FlashHsOnTurnStart;
			CheckboxFullTextSearch.IsChecked = Config.Instance.UseFullTextSearch;
			CheckboxStatsInWindow.IsChecked = Config.Instance.StatsInWindow;
			CheckboxDeleteDeckKeepStats.IsChecked = Config.Instance.KeepStatsWhenDeletingDeck;
			CheckboxNoteDialog.IsChecked = Config.Instance.ShowNoteDialogAfterGame;
			CheckboxTimerAlert.IsChecked = Config.Instance.TimerAlert;
			CheckboxNoteDialogDelayed.IsChecked = Config.Instance.NoteDialogDelayed;
			CheckboxNoteDialogDelayed.IsEnabled = Config.Instance.ShowNoteDialogAfterGame;
			CheckboxAutoGrayoutSecrets.IsChecked = Config.Instance.AutoGrayoutSecrets;
			CheckboxTrackerCardToolTips.IsChecked = Config.Instance.TrackerCardToolTips;
			_initialized = true;
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
				SaveConfig(false);
			}
		}

		private async void CheckboxTrackerCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized)
				return;
			Config.Instance.TrackerCardToolTips = true;
			SaveConfig(false);
			await Helper.MainWindow.Restart();
		}

		private async void CheckboxTrackerCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			//this is probably somehow possible without restarting
			if(!_initialized)
				return;
			Config.Instance.TrackerCardToolTips = false;
			SaveConfig(false);
			await Helper.MainWindow.Restart();
		}

		private void CheckboxBringHsToForegorund_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.BringHsToForeground = true;
			SaveConfig(false);
		}

		private void CheckboxBringHsToForegorund_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.BringHsToForeground = false;
			SaveConfig(false);
		}

		private void CheckboxFlashHs_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.FlashHsOnTurnStart = true;
			SaveConfig(false);
		}

		private void CheckboxFlashHs_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.FlashHsOnTurnStart = false;
			SaveConfig(false);
		}

		private void CheckboxFullTextSearch_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseFullTextSearch = true;
			SaveConfig(false);
		}

		private void CheckboxFullTextSearch_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.UseFullTextSearch = false;
			SaveConfig(false);
		}

		private void CheckboxStatsInWindow_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.StatsInWindow = true;
			Config.Save();
		}

		private void CheckboxStatsInWindow_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.StatsInWindow = false;
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

		private void CheckboxDeleteDeckKeepStats_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.KeepStatsWhenDeletingDeck = true;
			Config.Save();
		}

		private void CheckboxDeleteDeckKeepStats_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.KeepStatsWhenDeletingDeck = false;
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

		private void CheckboxAutoGrayoutSecrets_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoGrayoutSecrets = true;
			Config.Save();
		}

		private void CheckboxAutoGrayoutSecrets_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoGrayoutSecrets = false;
			Config.Save();
		}

		private void CheckboxAutoSelectDeck_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoSelectDetectedDeck = true;
			SaveConfig(false);
		}

		private void CheckboxAutoSelectDeck_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoSelectDetectedDeck = false;
			SaveConfig(false);
		}

		private void CheckboxKeepDecksVisible_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.KeepDecksVisible = true;
			SaveConfig(true);
		}

		private void CheckboxKeepDecksVisible_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.KeepDecksVisible = false;
			SaveConfig(true);
		}

		private void CheckboxTimerAlert_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerAlert = true;
			TextboxTimerAlert.IsEnabled = true;
			SaveConfig(false);
		}

		private void CheckboxTimerAlert_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerAlert = false;
			TextboxTimerAlert.IsEnabled = false;
			SaveConfig(false);
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Helper.MainWindow.Overlay.Update(true);
		}
	}
}