#region

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
			CheckBoxUnexpectedOnly.IsChecked = Config.Instance.GameResultNotificationsUnexpectedOnly;
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

		private void CheckBoxUnexpectedOnly_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameResultNotificationsUnexpectedOnly = true;
			Config.Save();
		}

		private void CheckBoxUnexpectedOnly_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.GameResultNotificationsUnexpectedOnly = false;
			Config.Save();
		}
	}
}