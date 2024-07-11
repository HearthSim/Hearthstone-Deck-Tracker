#region

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay
{
	/// <summary>
	/// Interaction logic for Overlay.xaml
	/// </summary>
	public partial class OverlayBattlegrounds : INotifyPropertyChanged
	{
		private bool _initialized;

		public OverlayBattlegrounds()
		{
			InitializeComponent();
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			var handler = PropertyChanged;
			handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public double SessionRecapScaling
		{
			get { return Config.Instance.OverlaySessionRecapScaling; }
			set
			{
				if(!_initialized)
					return;
				value = Math.Round(value);
				if(value < SliderSessionRecapScaling.Minimum)
					value = SliderSessionRecapScaling.Minimum;
				else if(value > SliderSessionRecapScaling.Maximum)
					value = SliderSessionRecapScaling.Maximum;
				Config.Instance.OverlaySessionRecapScaling = value;
				Config.Save();
				Core.Overlay.UpdateScaling();
				Core.Windows.BattlegroundsSessionWindow.UpdateScaling();
				OnPropertyChanged();
			}
		}

		public void Load()
		{
			// Note: The wording on this setting is inverted!
			CheckboxShowOverlayInBackground.IsChecked = !Config.Instance.HideInBackground;
			CheckboxShowMenuOverlayInBackground.IsChecked = !Config.Instance.HideMenuOverlayInBackground;

			CheckboxEnableTier7.IsChecked = Config.Instance.EnableBattlegroundsTier7Overlay;
			CheckboxShowTier7PreLobby.IsChecked = Config.Instance.ShowBattlegroundsTier7PreLobby;
			CheckboxShowTier7PreLobby.Visibility = (HSReplayNetOAuth.AccountData?.IsTier7 ?? false) ? Visibility.Visible : Visibility.Collapsed;

			CheckboxShowTier7CompStats.IsChecked = Config.Instance.ShowBattlegroundsTier7SessionCompStats;
			CheckboxShowBattlegroundsHeroPicking.IsChecked = Config.Instance.ShowBattlegroundsHeroPicking;
			CheckboxShowBattlegroundsQuestPicking.IsChecked = Config.Instance.ShowBattlegroundsQuestPicking;
			CheckboxShowBattlegroundsQuestPickingComps.IsChecked = Config.Instance.ShowBattlegroundsQuestPickingComps;
			CheckboxShowBattlegroundsTiers.IsChecked = Config.Instance.ShowBattlegroundsTiers;
			CheckboxShowBattlegroundsTurnCounter.IsChecked = Config.Instance.ShowBattlegroundsTurnCounter;

			CheckboxRunCombatSimulations.IsChecked = Config.Instance.RunBobsBuddy;
			CheckboxShowResultsDuringCombat.IsChecked = Config.Instance.ShowBobsBuddyDuringCombat;
			CheckboxShowResultsDuringShopping.IsChecked = Config.Instance.ShowBobsBuddyDuringShopping;
			CheckboxAlwaysShowAverageDamage.IsChecked = Config.Instance.AlwaysShowAverageDamage;

			CheckboxShowSessionRecap.IsChecked = Config.Instance.ShowSessionRecap;
			CheckboxShowMinionsAvailable.IsChecked = Config.Instance.ShowSessionRecapMinionsAvailable;
			CheckboxShowMinionsBanned.IsChecked = Config.Instance.ShowSessionRecapMinionsBanned;
			CheckboxShowStartCurrentMMR.IsChecked = Config.Instance.ShowSessionRecapStartCurrentMMR;
			CheckboxShowLatestGames.IsChecked = Config.Instance.ShowSessionRecapLatestGames;
			CheckboxShowSessionRecapBetweenGames.IsChecked = Config.Instance.ShowSessionRecapBetweenGames;
			CheckboxShowExternalWindow.IsChecked = Config.Instance.BattlegroundsSessionRecapWindowOnStart;

			ConfigWrapper.ShowBattlegroundsHeroPickingChanged += () =>
				CheckboxShowBattlegroundsHeroPicking.IsChecked = Config.Instance.ShowBattlegroundsHeroPicking;
			_initialized = true;
		}

		internal void UpdateDisabledState()
		{
			var enabled = true;
			if(Remote.Config.Data?.BobsBuddy?.Disabled ?? false)
			{
				TextBobsBuddyDisabled.Text = "Temporarily Disabled";
				enabled = false;
			}
			else
			{
				var verStr = Remote.Config.Data?.BobsBuddy?.MinRequiredVersion;
				if(Version.TryParse(verStr, out var requiredVersion) && requiredVersion > Helper.GetCurrentVersion())
				{
					TextBobsBuddyDisabled.Text = $"Requires HDT v{requiredVersion}";
					enabled = false;
				}
			}
			TextBobsBuddyDisabled.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Core.Overlay.Update(true);
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

		private void CheckboxShowBattlegroundsHeroPicking_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsHeroPicking = true;
			SaveConfig(true);
			if(Core.Game.IsBattlegroundsMatch)
				Core.Overlay.BattlegroundsHeroPickingViewModel.StatsVisibility = Visibility.Visible;
		}

		private void CheckboxShowBattlegroundsHeroPicking_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsHeroPicking = false;
			SaveConfig(true);
			if(Core.Game.IsBattlegroundsMatch)
				Core.Overlay.BattlegroundsHeroPickingViewModel.StatsVisibility = Visibility.Collapsed;
		}

		private void CheckboxShowBattlegroundsCompStats_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsTier7SessionCompStats = true;
			SaveConfig(true);
			Core.Overlay.BattlegroundsSessionViewModelVM.UpdateCompositionStatsVisibility();
		}

		private void CheckboxShowBattlegroundsCompStats_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsTier7SessionCompStats = false;
			SaveConfig(true);
			Core.Overlay.BattlegroundsSessionViewModelVM.UpdateCompositionStatsVisibility();
		}

		private void CheckboxShowBattlegroundsQuestPicking_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsQuestPicking = true;
			SaveConfig(true);
			if(Core.Game.IsBattlegroundsMatch)
				Core.Overlay.BattlegroundsQuestPickingViewModel.Visibility = Visibility.Visible;
		}

		private void CheckboxShowBattlegroundsQuestPicking_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsQuestPicking = false;
			SaveConfig(true);
			if(Core.Game.IsBattlegroundsMatch)
				Core.Overlay.BattlegroundsQuestPickingViewModel.Visibility = Visibility.Collapsed;
		}

		private void CheckboxShowBattlegroundsQuestPickingComps_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsQuestPickingComps = true;
			SaveConfig(true);
		}

		private void CheckboxShowBattlegroundsQuestPickingComps_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsQuestPickingComps = false;
			SaveConfig(true);
		}

		private void CheckboxShowBattlegroundsTiers_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsTiers = true;
			SaveConfig(true);
			if(Core.Game.IsBattlegroundsMatch)
				Core.Overlay.BattlegroundsMinionsPanel.Visibility = Visibility.Visible;
		}

		private void CheckboxShowBattlegroundsTiers_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsTiers = false;
			SaveConfig(true);
			if(Core.Game.IsBattlegroundsMatch)
				Core.Overlay.BattlegroundsMinionsPanel.Visibility = Visibility.Collapsed;
		}

		private void CheckboxShowBattlegroundsTurnCounter_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsTurnCounter = true;
			SaveConfig(true);
			if(Core.Game.IsBattlegroundsMatch)
				Core.Overlay.TurnCounter.Visibility = Visibility.Visible;
		}

		private void CheckboxShowBattlegroundsTurnCounter_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsTurnCounter = false;
			SaveConfig(true);
			if(Core.Game.IsBattlegroundsMatch)
				Core.Overlay.TurnCounter.Visibility = Visibility.Collapsed;
		}

		private void CheckboxRunCombatSimulations_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RunBobsBuddy = true;
			SaveConfig(true);
			if(Core.Game.IsBattlegroundsMatch)
				Core.Overlay.ShowBobsBuddyPanel();
			Influx.OnBobsBuddyEnabledChanged(true);
		}

		private void CheckboxRunCombatSimulations_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.RunBobsBuddy = false;
			SaveConfig(true);
			if(Core.Game.IsBattlegroundsMatch)
				Core.Overlay.HideBobsBuddyPanel();
			Influx.OnBobsBuddyEnabledChanged(false);
		}

		private void CheckboxShowResultsDuringCombat_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBobsBuddyDuringCombat = true;
			SaveConfig(true);
		}

		private void CheckboxShowResultsDuringCombat_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBobsBuddyDuringCombat = false;
			SaveConfig(true);
		}

		private void CheckboxShowResultsDuringShopping_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBobsBuddyDuringShopping = true;
			SaveConfig(true);
		}

		private void CheckboxShowResultsDuringShopping_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBobsBuddyDuringShopping = false;
			SaveConfig(true);
		}

		private void CheckboxAlwaysShowAverageDamage_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AlwaysShowAverageDamage = true;
			Core.Overlay.BobsBuddyDisplay.AttemptToExpandAverageDamagePanels(false, false);
			SaveConfig(true);
		}

		private void CheckboxAlwaysShowAverageDamage_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.AlwaysShowAverageDamage = false;
			Core.Overlay.BobsBuddyDisplay.ShowAverageDamagesPanels(false);
			SaveConfig(true);
		}

		private void OverlayHelpButtonClick(object sender, RoutedEventArgs e)
		{
			e.Handled = true;
			Core.MainWindow.Options.TreeViewItemStreamingCapturableOverlay.IsSelected = true;
		}

		private void CheckboxShowSessionRecap_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecap = true;
			UpdateSessionRecap(true);
			SaveConfig(true);

			Core.Game.BattlegroundsSessionViewModel.UpdateSectionsVisibilities();
			Core.Overlay.UpdateBattlegroundsSessionVisibility();
			Influx.OnSessionRecapEnabledChanged(true);
		}

		private void CheckboxShowSessionRecap_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecap = false;
			SaveConfig(true);
			Core.Game.BattlegroundsSessionViewModel.UpdateSectionsVisibilities();
			Core.Overlay.UpdateBattlegroundsSessionVisibility();
			Influx.OnSessionRecapEnabledChanged(false);
		}

		private void CheckboxShowSessionRecapBetweenGames_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecapBetweenGames = true;
			SaveConfig(true);
			Core.Overlay.UpdateBattlegroundsSessionVisibility();
		}

		private void CheckboxShowSessionRecapBetweenGames_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecapBetweenGames = false;
			SaveConfig(true);
			Core.Overlay.UpdateBattlegroundsSessionVisibility();
		}

		private void CheckboxShowMinionsAvailable_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecapMinionsAvailable = true;
			SaveConfig(true);
			Core.Game.BattlegroundsSessionViewModel.UpdateSectionsVisibilities();
		}

		private void CheckboxShowMinionsAvailable_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecapMinionsAvailable = false;
			UpdateSessionRecap();
			SaveConfig(true);
			Core.Game.BattlegroundsSessionViewModel.UpdateSectionsVisibilities();
		}

		private void CheckboxShowMinionsBanned_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecapMinionsBanned = true;
			SaveConfig(true);
			Core.Game.BattlegroundsSessionViewModel.UpdateSectionsVisibilities();
		}

		private void CheckboxShowMinionsBanned_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecapMinionsBanned = false;
			UpdateSessionRecap();
			SaveConfig(true);
			Core.Game.BattlegroundsSessionViewModel.UpdateSectionsVisibilities();
		}

		private void CheckboxShowStartCurrentMMR_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecapStartCurrentMMR = true;
			SaveConfig(true);
			Core.Game.BattlegroundsSessionViewModel.UpdateSectionsVisibilities();
		}

		private void CheckboxShowStartCurrentMMR_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecapStartCurrentMMR = false;
			UpdateSessionRecap();
			SaveConfig(true);
			Core.Game.BattlegroundsSessionViewModel.UpdateSectionsVisibilities();
		}

		private void CheckboxShowLatestGames_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecapLatestGames = true;
			SaveConfig(true);
			Core.Game.BattlegroundsSessionViewModel.UpdateSectionsVisibilities();
		}

		private void CheckboxShowLatestGames_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecapLatestGames = false;
			UpdateSessionRecap();
			SaveConfig(true);
			Core.Game.BattlegroundsSessionViewModel.UpdateSectionsVisibilities();
		}

		private void CheckboxShowExternalWindow_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Core.Windows.BattlegroundsSessionWindow.Show();
			Core.Windows.BattlegroundsSessionWindow.Activate();
			Config.Instance.BattlegroundsSessionRecapWindowOnStart = true;
			UpdateSessionRecap(true);
			Config.Save();
		}

		private void CheckboxShowExternalWindow_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Core.Windows.BattlegroundsSessionWindow.Hide();
			Config.Instance.BattlegroundsSessionRecapWindowOnStart = false;
			Config.Save();
		}

		private void TextBoxSessionRecapScaling_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			if(!char.IsDigit(e.Text, e.Text.Length - 1))
				e.Handled = true;
		}

		public ICommand ResetSessionCommand => new Command(async () =>
		{
			var result = await Core.MainWindow.ShowMessageAsync(
				LocUtil.Get("Options_Overlay_Battlegrounds_Dialog_ResetSession_Title"),
				LocUtil.Get("Options_Overlay_Battlegrounds_Dialog_ResetSession_Desc"),
				MessageDialogStyle.AffirmativeAndNegative,
				new MessageDialogs.Settings { AffirmativeButtonText = LocUtil.Get("Options_Overlay_Battlegrounds_Dialog_Confirmation_Reset") }
			);
			if(result != MessageDialogResult.Affirmative)
				return;

			BattlegroundsLastGames.Instance.Reset();
			Core.Game.BattlegroundsSessionViewModel.Update();
		});

		private async void BtnUnlockOverlay_Click(object sender, RoutedEventArgs e)
		{
			if(User32.GetHearthstoneWindow() == IntPtr.Zero)
				return;
			BtnUnlockOverlay.Content = await Core.Overlay.UnlockUi(true) ? "Lock" : "Unlock";
		}

		private async void BtnResetOverlay_Click(object sender, RoutedEventArgs e)
		{
			var result =
				await
				Core.MainWindow.ShowMessageAsync(LocUtil.Get("Options_Overlay_Battlegrounds_Dialog_ResetPos_Title"),
												 LocUtil.Get("Options_Overlay_Battlegrounds_Dialog_ResetPos_Title"),
												 MessageDialogStyle.AffirmativeAndNegative);
			if(result != MessageDialogResult.Affirmative)
				return;

			if((string)BtnUnlockOverlay.Content == "Lock")
			{
				await Core.Overlay.UnlockUi(true);
				BtnUnlockOverlay.Content = "Unlock";
			}

			Config.Instance.Reset(nameof(Config.SessionRecapTop));
			Config.Instance.Reset(nameof(Config.SessionRecapLeft));
			SaveConfig(true);
		}

		private void CheckboxEnableTier7_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.EnableBattlegroundsTier7Overlay = true;
			SaveConfig(true);
			Core.Overlay.UpdateTier7PreLobbyVisibility();
		}

		private void CheckboxEnableTier7_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.EnableBattlegroundsTier7Overlay = false;
			SaveConfig(true);
			Core.Overlay.UpdateTier7PreLobbyVisibility();
		}

		private void CheckboxShowTier7PreLobby_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsTier7PreLobby = true;
			SaveConfig(true);
			Core.Overlay.UpdateTier7PreLobbyVisibility();
		}

		private void CheckboxShowTier7PreLobby_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowBattlegroundsTier7PreLobby = false;
			SaveConfig(true);
			Core.Overlay.UpdateTier7PreLobbyVisibility();
		}

		private void UpdateSessionRecap(bool changedVisibility = false)
		{

			if(
				!Config.Instance.ShowSessionRecapMinionsAvailable &&
				!Config.Instance.ShowSessionRecapMinionsBanned &&
				!Config.Instance.ShowSessionRecapStartCurrentMMR &&
				!Config.Instance.ShowSessionRecapLatestGames
			)
			{
				if(changedVisibility)
				{
					CheckboxShowMinionsBanned.IsChecked = true;
					CheckboxShowStartCurrentMMR.IsChecked = true;
					CheckboxShowLatestGames.IsChecked = true;
				}
				else
				{
					CheckboxShowSessionRecap.IsChecked = false;
					CheckboxShowExternalWindow.IsChecked = false;
				}
			}
		}
	}
}
