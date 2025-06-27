#region

using System;
using System.Runtime.CompilerServices;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay
{
	/// <summary>
	/// Interaction logic for Overlay.xaml
	/// </summary>
	public partial class OverlayGeneral
	{
		private bool _initialized;

		public OverlayGeneral()
		{
			InitializeComponent();
		}

		public void Load()
		{
			// Note: The wording on this setting is inverted!
			CheckboxShowOverlayInBackground.IsChecked = !Config.Instance.HideInBackground;
			CheckboxShowMenuOverlayInBackground.IsChecked = !Config.Instance.HideMenuOverlayInBackground;

			CheckboxHideOverlayInMenu.IsChecked = Config.Instance.HideInMenu;
			CheckboxHideOverlay.IsChecked = Config.Instance.HideOverlay;
			CheckboxHideDecksInOverlay.IsChecked = Config.Instance.HideDecksInOverlay;
			CheckboxHideOverlayInSpectator.IsChecked = Config.Instance.HideOverlayInSpectator;
			CheckboxOverlayCardMarkToolTips.IsChecked = Config.Instance.OverlayCardMarkToolTips;
			SliderOverlayOpacity.Value = Config.Instance.OverlayOpacity;
			CheckboxHideTimers.IsChecked = Config.Instance.HideTimers;
			CheckboxOverlayCardToolTips.IsChecked = Config.Instance.OverlayCardToolTips;
			CheckboxAutoGrayoutSecrets.IsChecked = Config.Instance.AutoGrayoutSecrets;
			CheckboxKeepDecksVisible.IsChecked = Config.Instance.KeepDecksVisible;
			CheckBoxBatteryStatus.IsChecked = Config.Instance.ShowBatteryLife;
			CheckBoxBatteryStatusText.IsChecked = Config.Instance.ShowBatteryLifePercent;
			CheckBoxFlavorText.IsChecked = Config.Instance.ShowFlavorText;
			CheckBoxOverlayUseAnimations.IsChecked = Config.Instance.OverlayCardAnimations;
			CheckBoxRemoveSecrets.IsChecked = Config.Instance.RemoveSecretsFromList;

			var isPremium = (HSReplayNetOAuth.AccountData?.IsPremium ?? false);
			var mulliganGuideDisabled = Remote.Config.Data?.MulliganGuide?.Disabled ?? false;
			CheckboxEnableMulliganGuide.IsChecked = Config.Instance.EnableMulliganGuide;
			CheckboxAutoShowMulliganGuide.IsChecked = Config.Instance.AutoShowMulliganGuide;
			CheckboxShowMulliganGuidePreLobby.IsChecked = Config.Instance.ShowMulliganGuidePreLobby;
			StackPanelMulliganGuide.Visibility = isPremium && !mulliganGuideDisabled ? Visibility.Visible : Visibility.Collapsed;

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

		private void CheckboxOverlaySetToolTips_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlaySetToolTips = true;
			SaveConfig(false);
		}

		private void CheckboxOverlaySetToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.OverlaySetToolTips = false;
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
			if(this.ParentMainWindow() is not { } window)
				return;
			var result = await window.ShowMessageAsync("Resetting overlay to default",
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

			Config.Instance.Reset(nameof(Config.TimersVerticalPosition));
			Config.Instance.Reset(nameof(Config.TimersVerticalSpacing));

			Config.Instance.Reset(nameof(Config.SecretsTop));
			Config.Instance.Reset(nameof(Config.SecretsLeft));
			Config.Instance.Reset(nameof(Config.SecretsPanelHeight));

			Config.Instance.Reset(nameof(Config.PlayerActiveEffectsHorizontal));
			Config.Instance.Reset(nameof(Config.PlayerActiveEffectsVertical));

			Config.Instance.Reset(nameof(Config.OpponentActiveEffectsHorizontal));
			Config.Instance.Reset(nameof(Config.OpponentActiveEffectsVertical));

			Config.Instance.Reset(nameof(Config.PlayerCountersHorizontal));
			Config.Instance.Reset(nameof(Config.PlayerCountersVertical));

			Config.Instance.Reset(nameof(Config.OpponentCountersHorizontal));
			Config.Instance.Reset(nameof(Config.OpponentCountersVertical));

			Config.Instance.Reset(nameof(Config.PlayerMaxResourcesHorizontal));
			Config.Instance.Reset(nameof(Config.PlayerMaxResourcesVertical));

			Config.Instance.Reset(nameof(Config.OpponentMaxResourcesHorizontal));
			Config.Instance.Reset(nameof(Config.OpponentMaxResourcesVertical));

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
			ConfigWrapper.Bindable.OverlayCardTooltips = true;
		}

		private void CheckboxOverlayCardToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			ConfigWrapper.Bindable.OverlayCardTooltips = false;
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
			ConfigWrapper.Bindable.OverlayCardMarkTooltips = true;
		}

		private void CheckboxOverlayCardMarkToolTips_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			ConfigWrapper.Bindable.OverlayCardMarkTooltips = false;
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

		private void CheckboxShowOverlayInBackground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			// Note: The wording on this setting is inverted!
			Config.Instance.HideInBackground = false;
			SaveConfig(true);
		}

		private void CheckboxShowOverlayInBackground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			// Note: The wording on this setting is inverted!
			Config.Instance.HideInBackground = true;
			SaveConfig(true);
		}

		private void CheckboxShowMenuOverlayInBackground_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			// Note: The wording on this setting is inverted!
			Config.Instance.HideMenuOverlayInBackground = false;
			SaveConfig(true);
		}

		private void CheckboxShowMenuOverlayInBackground_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			// Note: The wording on this setting is inverted!
			Config.Instance.HideMenuOverlayInBackground = true;
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

		private void CheckBoxRemoveSecrets_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RemoveSecretsFromList = true;
			Config.Save();
		}

		private void CheckBoxRemoveSecrets_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RemoveSecretsFromList = false;
			Config.Save();
		}

		private void CheckboxEnableMulliganGuide_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.EnableMulliganGuide = true;
			Config.Save();
			Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
		}

		private void CheckboxEnableMulliganGuide_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.EnableMulliganGuide = false;
			Config.Save();
			Core.Overlay.HideMulliganGuideStats();
			// Clear the Mulligan overlay if it's visible
			Core.Game.Player.MulliganCardStats = null;
			Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
		}

		private void CheckboxAutoShowMulliganGuide_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoShowMulliganGuide = true;
			Config.Save();
		}

		private void CheckboxAutoShowMulliganGuide_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AutoShowMulliganGuide = false;
			Config.Save();
		}

		private void CheckboxShowMulliganGuidePreLobby_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMulliganGuidePreLobby = true;
			Config.Save();
			Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
		}

		private void CheckboxShowMulliganGuidePreLobby_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMulliganGuidePreLobby = false;
			Config.Save();
			Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
		}
	}
}
