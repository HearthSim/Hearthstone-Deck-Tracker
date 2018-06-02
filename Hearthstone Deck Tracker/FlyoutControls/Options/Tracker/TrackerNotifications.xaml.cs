#region

using Hearthstone_Deck_Tracker.Enums;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Tracker
{
	/// <summary>
	/// Interaction logic for TrackerHotKeys.xaml
	/// </summary>
	public partial class TrackerNotifications
	{
		private bool _initialized;

		public TrackerNotifications()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckBoxShowNotifications.IsChecked = Config.Instance.ShowGameResultNotifications;
			CheckboxNoteDialog.IsChecked = Config.Instance.ShowNoteDialogAfterGame;
			CheckboxNoteDialogDelayed.IsChecked = Config.Instance.NoteDialogDelayed;
			CheckboxNoteDialogDelayed.IsEnabled = Config.Instance.ShowNoteDialogAfterGame;
			CheckboxArenaRewardDialog.IsChecked = Config.Instance.ArenaRewardDialog;
			ComboboxTurnAction.ItemsSource = Enum.GetValues(typeof(HsActionType)).Cast<HsActionType>();
			ComboboxTurnAction.SelectedIndex = (int)Config.Instance.TurnStartAction;
			ComboboxChallengeAction.ItemsSource = Enum.GetValues(typeof(HsActionType)).Cast<HsActionType>();
			ComboboxChallengeAction.SelectedIndex = (int)Config.Instance.ChallengeAction;


			CheckboxTimerAlert2.IsChecked = Config.Instance.TimerAlert;
			TextboxTimerAlert2.Text = Config.Instance.TimerAlertSeconds.ToString();

			_initialized = true;
		}

		private void TextboxTimerAlert_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if(!char.IsDigit(e.Text, e.Text.Length - 1))
				e.Handled = true;
		}

		private void TextboxTimerAlert_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(!_initialized)
				return;
			int mTimerAlertValue;
			if(int.TryParse(TextboxTimerAlert.Text, out mTimerAlertValue))
			{
				if(mTimerAlertValue < 1)
				{
					TextboxTimerAlert.Text = "1";
					mTimerAlertValue = 0;
				}

				if(mTimerAlertValue > 10)
				{
					TextboxTimerAlert.Text = "10";
					mTimerAlertValue = 10;
				}

				Config.Instance.NotificationFadeOutDelay = mTimerAlertValue;
				Config.Save();
			}
		}

		private void CheckBoxShowNotifications_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowGameResultNotifications = true;
			Config.Save();
		}

		private void CheckBoxShowNotifications_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowGameResultNotifications = false;
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

		private void CheckboxArenaRewardDialog_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ArenaRewardDialog = true;
			Config.Save();
		}

		private void CheckboxArenaRewardDialog_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ArenaRewardDialog = false;
			Config.Save();
		}

		private void CheckboxTimerAlert2_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerAlert = true;
			TextboxTimerAlert2.IsEnabled = true;
			Config.Save();
		}

		private void CheckboxTimerAlert2_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TimerAlert = false;
			TextboxTimerAlert2.IsEnabled = false;
			Config.Save();
		}

		private void TextboxTimerAlert2_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(!_initialized || CheckboxTimerAlert2.IsChecked != true)
				return;
			if(int.TryParse(TextboxTimerAlert2.Text, out var mTimerAlertValue))
			{
				if (mTimerAlertValue < 0)
				{
					TextboxTimerAlert2.Text = "0";
					mTimerAlertValue = 0;
				}

				if (mTimerAlertValue > 90)
				{
					TextboxTimerAlert2.Text = "90";
					mTimerAlertValue = 90;
				}

				Config.Instance.TimerAlertSeconds = mTimerAlertValue;
				Config.Save();
			}
		}

		private void TextboxTimerAlert2_PreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if(!char.IsDigit(e.Text, e.Text.Length - 1))
				e.Handled = true;
		}

		private void ComboboxTurnAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.TurnStartAction = (HsActionType)ComboboxTurnAction.SelectedIndex;
			Config.Save();
		}

		private void ComboboxChallengeAction_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ChallengeAction = (HsActionType)ComboboxChallengeAction.SelectedIndex;
			Config.Save();
		}
	}
}
