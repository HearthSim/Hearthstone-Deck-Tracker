#region

using System;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay
{
	/// <summary>
	/// Interaction logic for Overlay.xaml
	/// </summary>
	public partial class OverlayGeneral
	{
	    private GameV2 _game;
	    private bool _initialized;

		public OverlayGeneral()
		{
		    
		    InitializeComponent();
		}

	    public void Load(GameV2 game)
		{
            _game = game;
            CheckboxHideOverlayInBackground.IsChecked = Config.Instance.HideInBackground;
			CheckboxHideOpponentCardAge.IsChecked = Config.Instance.HideOpponentCardAge;
			CheckboxHideOpponentCardMarks.IsChecked = Config.Instance.HideOpponentCardMarks;
			CheckboxHideOverlayInMenu.IsChecked = Config.Instance.HideInMenu;
			CheckboxHideOverlay.IsChecked = Config.Instance.HideOverlay;
			CheckboxHideDecksInOverlay.IsChecked = Config.Instance.HideDecksInOverlay;
			CheckboxHideSecrets.IsChecked = Config.Instance.HideSecrets;
			CheckboxOverlaySecretToolTipsOnly.IsChecked = Config.Instance.OverlaySecretToolTipsOnly;
			CheckboxHideOverlayInSpectator.IsChecked = Config.Instance.HideOverlayInSpectator;
			CheckboxOverlayCardMarkToolTips.IsChecked = Config.Instance.OverlayCardMarkToolTips;
			SliderOverlayOpacity.Value = Config.Instance.OverlayOpacity;
			CheckboxHideTimers.IsChecked = Config.Instance.HideTimers;
			CheckboxOverlayCardToolTips.IsChecked = Config.Instance.OverlayCardToolTips;
			CheckboxOverlayAdditionalCardToolTips.IsEnabled = Config.Instance.OverlayCardToolTips;
			CheckboxOverlayAdditionalCardToolTips.IsChecked = Config.Instance.AdditionalOverlayTooltips;
			CheckboxAutoGrayoutSecrets.IsChecked = Config.Instance.AutoGrayoutSecrets;
			CheckboxKeepDecksVisible.IsChecked = Config.Instance.KeepDecksVisible;
			CheckboxAlwaysShowGoldProgress.IsChecked = Config.Instance.AlwaysShowGoldProgress;
		    CheckboxHidePlayerAttackIcon.IsChecked = Config.Instance.HidePlayerAttackIcon;
		    CheckboxHideOpponentAttackIcon.IsChecked = Config.Instance.HideOpponentAttackIcon;
			_initialized = true;
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Core.Overlay.Update(true);
		}

		private void CheckboxOverlayAdditionalCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AdditionalOverlayTooltips = true;
			SaveConfig(false);
		}

		private void CheckboxOverlayAdditionalCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AdditionalOverlayTooltips = false;
			SaveConfig(false);
		}

		private async void BtnUnlockOverlay_Click(object sender, RoutedEventArgs e)
		{
			if(User32.GetHearthstoneWindow() == IntPtr.Zero)
				return;
			BtnUnlockOverlay.Content = await Core.Overlay.UnlockUI() ? "Lock" : "Unlock";
		}

		private async void BtnResetOverlay_Click(object sender, RoutedEventArgs e)
		{
			var result =
				await
				Core.MainWindow.ShowMessageAsync("Resetting overlay to default",
				                                   "Positions of: Player Deck, Opponent deck, Timers and Secrets will be reset to default. Are you sure?",
				                                   MessageDialogStyle.AffirmativeAndNegative);
			if(result != MessageDialogResult.Affirmative)
				return;

			if((string)BtnUnlockOverlay.Content == "Lock")
			{
				await Core.Overlay.UnlockUI();
				BtnUnlockOverlay.Content = "Unlock";
			}


			Config.Instance.Reset("PlayerDeckTop");
			Config.Instance.Reset("PlayerDeckLeft");
			Config.Instance.Reset("PlayerDeckHeight");

			Config.Instance.Reset("PlayerDeckHeight");
			Config.Instance.Reset("OpponentDeckLeft");
			Config.Instance.Reset("OpponentDeckHeight");

			Config.Instance.Reset("TimersHorizontalPosition");
			Config.Instance.Reset("TimersHorizontalSpacing");

			Config.Instance.Reset("TimersHorizontalSpacing");
			Config.Instance.Reset("TimersVerticalSpacing");

			Config.Instance.Reset("SecretsTop");
			Config.Instance.Reset("SecretsLeft");

			SaveConfig(true);
		}

		private void CheckboxOverlayCardToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlayCardToolTips = true;
			CheckboxOverlayAdditionalCardToolTips.IsEnabled = true;
			CheckboxOverlaySecretToolTipsOnly.IsEnabled = true;
			SaveConfig(true);
		}

		private void CheckboxOverlayCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlayCardToolTips = false;
			CheckboxOverlayAdditionalCardToolTips.IsChecked = false;
			CheckboxOverlayAdditionalCardToolTips.IsEnabled = false;
			CheckboxOverlaySecretToolTipsOnly.IsEnabled = false;
			CheckboxOverlaySecretToolTipsOnly.IsChecked = false;
			SaveConfig(true);
		}

		private void CheckboxDeckSortingClassFirst_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.CardSortingClassFirst = true;
			SaveConfig(false);
			Helper.SortCardCollection(Core.MainWindow.ListViewDeck.ItemsSource, true);
			//Helper.SortCardCollection(ListViewNewDeck.Items, true);
		}

		private void CheckboxDeckSortingClassFirst_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.CardSortingClassFirst = false;
			SaveConfig(false);
			Helper.SortCardCollection(Core.MainWindow.ListViewDeck.ItemsSource, false);
			//Helper.SortCardCollection(ListViewNewDeck.Items, false);
		}

		private void CheckboxHideDecksInOverlay_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideDecksInOverlay = true;
			SaveConfig(true);
		}

		private void CheckboxHideDecksInOverlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideDecksInOverlay = false;
			SaveConfig(true);
		}

		private void CheckboxHideSecrets_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideSecrets = true;
			SaveConfig(false);
			Core.Overlay.HideSecrets();
		}

		private void CheckboxHideSecrets_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideSecrets = false;
			SaveConfig(false);
			if(!_game.IsInMenu)
				Core.Overlay.ShowSecrets();
		}

		private void CheckboxOverlaySecretToolTipsOnly_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlaySecretToolTipsOnly = true;
			Config.Save();
		}

		private void CheckboxOverlaySecretToolTipsOnly_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlaySecretToolTipsOnly = false;
			Config.Save();
		}

		private void CheckboxHideOverlayInSpectator_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOverlayInSpectator = true;
			Config.Save();
		}

		private void CheckboxHideOverlayInSpectator_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOverlayInSpectator = false;
			Config.Save();
		}

		private void CheckboxOverlayCardMarkToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlayCardMarkToolTips = true;
			Config.Save();
		}

		private void CheckboxOverlayCardMarkToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlayCardMarkToolTips = false;
			Config.Save();
		}

		private void CheckboxHideTimers_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideTimers = true;
			SaveConfig(true);
		}

		private void CheckboxHideTimers_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideTimers = false;
			SaveConfig(true);
		}

		private void SliderOverlayOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlayOpacity = SliderOverlayOpacity.Value;
			SaveConfig(true);
		}

		private void CheckboxHideOverlay_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOverlay = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlay_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOverlay = false;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInMenu_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideInMenu = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInMenu_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideInMenu = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardAge_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentCardAge = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardAge_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentCardAge = true;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardMarks_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentCardMarks = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentCardMarks_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentCardMarks = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInBackground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideInBackground = true;
			SaveConfig(true);
		}

		private void CheckboxHideOverlayInBackground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideInBackground = false;
			SaveConfig(true);
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

		private void CheckboxAlwaysShowGoldProgress_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AlwaysShowGoldProgress = true;
			SaveConfig(true);
		}

		private void CheckboxAlwaysShowGoldProgress_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AlwaysShowGoldProgress = false;
			SaveConfig(true);
		}

		private void CheckboxHidePlayerAttackIcon_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HidePlayerAttackIcon = true;
			SaveConfig(true);
		}

		private void CheckboxHidePlayerAttackIcon_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HidePlayerAttackIcon = false;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentAttackIcon_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentAttackIcon = true;
			SaveConfig(true);
		}

		private void CheckboxHideOpponentAttackIcon_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.HideOpponentAttackIcon = false;
			SaveConfig(true);
		}
	}
}