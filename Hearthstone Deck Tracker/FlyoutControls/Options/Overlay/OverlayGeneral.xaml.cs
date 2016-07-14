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
			CheckBoxBatteryStatus.IsChecked = Config.Instance.ShowBatteryLife;
			CheckBoxBatteryStatusText.IsChecked = Config.Instance.ShowBatteryLifePercent;
			CheckBoxFlavorText.IsChecked = Config.Instance.ShowFlavorText;
			CheckBoxOverlayUseAnimations.IsChecked = Config.Instance.OverlayCardAnimations;
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
			BtnUnlockOverlay.Content = await Core.Overlay.UnlockUi() ? "Lock" : "Unlock";
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
				await Core.Overlay.UnlockUi();
				BtnUnlockOverlay.Content = "Unlock";
			}


			Config.Instance.Reset(nameof(Config.PlayerDeckTop));
			Config.Instance.Reset(nameof(Config.PlayerDeckLeft));
			Config.Instance.Reset(nameof(Config.PlayerDeckHeight));

			Config.Instance.Reset(nameof(Config.PlayerDeckHeight));
			Config.Instance.Reset(nameof(Config.OpponentDeckLeft));
			Config.Instance.Reset(nameof(Config.OpponentDeckHeight));

			Config.Instance.Reset(nameof(Config.TimersHorizontalPosition));
			Config.Instance.Reset(nameof(Config.TimersHorizontalSpacing));

			Config.Instance.Reset(nameof(Config.TimersHorizontalSpacing));
			Config.Instance.Reset(nameof(Config.TimersVerticalSpacing));

			Config.Instance.Reset(nameof(Config.SecretsTop));
			Config.Instance.Reset(nameof(Config.SecretsLeft));

			Config.Instance.Reset(nameof(Config.WotogIconsPlayerHorizontal));
			Config.Instance.Reset(nameof(Config.WotogIconsPlayerVertical));

			Config.Instance.Reset(nameof(Config.WotogIconsOpponentHorizontal));
			Config.Instance.Reset(nameof(Config.WotogIconsOpponentVertical));

			Config.Instance.Reset(nameof(Config.AttackIconPlayerHorizontalPosition));
			Config.Instance.Reset(nameof(Config.AttackIconPlayerVerticalPosition));

			Config.Instance.Reset(nameof(Config.AttackIconOpponentHorizontalPosition));
			Config.Instance.Reset(nameof(Config.AttackIconOpponentVerticalPosition));

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

		private void CheckBoxBatteryStatus_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.ShowBatteryLife = true;
			Config.Save();
			Core.Overlay.EnableBatteryMonitor();
		}

		private void CheckBoxBatteryStatus_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.ShowBatteryLife = false;
			Config.Save();
			Core.Overlay.DisableBatteryMonitor();
		}

		private void CheckBoxBatteryStatusText_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.ShowBatteryLifePercent = true;
			Config.Save();
			Core.Overlay.UpdateBatteryStatus();
		}

		private void CheckBoxBatteryStatusText_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBatteryLifePercent = false;
			Config.Save();
			Core.Overlay.UpdateBatteryStatus();
		}

		private void CheckBoxFlavorText_Checked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.ShowFlavorText = true;
			Config.Save();
		}

		private void CheckBoxFlavorText_Unchecked(object sender, RoutedEventArgs e)
		{
			if (!_initialized)
				return;
			Config.Instance.ShowFlavorText = false;
			Config.Save();
			Core.Overlay.FlavorTextVisibility = Visibility.Collapsed;
		}

		private void CheckboxOverlayUseAnimations_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlayCardAnimations = true;
			Config.Save();
		}

		private void CheckboxOverlayUseAnimations_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlayCardAnimations = false;
			Config.Save();
		}
	}
}