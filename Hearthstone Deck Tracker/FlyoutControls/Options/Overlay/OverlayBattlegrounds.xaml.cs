#region

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
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
			CheckboxShowBattlegroundsTiers.IsChecked = Config.Instance.ShowBattlegroundsTiers;
			CheckboxShowBattlegroundsTurnCounter.IsChecked = Config.Instance.ShowBattlegroundsTurnCounter;

			CheckboxRunCombatSimulations.IsChecked = Config.Instance.RunBobsBuddy;

			CheckboxShowResultsDuringCombat.IsChecked = Config.Instance.ShowBobsBuddyDuringCombat;
			CheckboxShowResultsDuringCombat.IsEnabled = Config.Instance.RunBobsBuddy;

			CheckboxShowResultsDuringShopping.IsChecked = Config.Instance.ShowBobsBuddyDuringShopping;
			CheckboxShowResultsDuringShopping.IsEnabled = Config.Instance.RunBobsBuddy;

			CheckboxAlwaysShowAverageDamage.IsChecked = Config.Instance.AlwaysShowAverageDamage;
			CheckboxAlwaysShowAverageDamage.IsEnabled = Config.Instance.RunBobsBuddy;

			CheckboxShowSessionRecap.IsChecked = Config.Instance.ShowSessionRecap;
			CheckboxShowMinionsBanned.IsChecked = Config.Instance.ShowSessionRecapMinionsBanned;
			CheckboxShowStartCurrentMMR.IsChecked = Config.Instance.ShowSessionRecapStartCurrentMMR;
			CheckboxShowLatestGames.IsChecked = Config.Instance.ShowSessionRecapLatestGames;
			CheckboxShowSessionRecapBetweenGames.IsChecked = Config.Instance.ShowSessionRecapBetweenGames;
			CheckboxShowExternalWindow.IsChecked = Config.Instance.BattlegroundsSessionRecapWindowOnStart;

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
			CheckboxShowResultsDuringCombat.IsEnabled = enabled && Config.Instance.RunBobsBuddy;
			CheckboxShowResultsDuringShopping.IsEnabled = enabled && Config.Instance.RunBobsBuddy;
			CheckboxAlwaysShowAverageDamage.IsEnabled = enabled && Config.Instance.RunBobsBuddy;
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Core.Overlay.Update(true);
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
			CheckboxShowResultsDuringCombat.IsEnabled = true;
			CheckboxShowResultsDuringShopping.IsEnabled = true;
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
			CheckboxShowResultsDuringCombat.IsEnabled = false;
			CheckboxShowResultsDuringShopping.IsEnabled = false;
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

			if(
				!Config.Instance.ShowSessionRecapMinionsBanned &&
				!Config.Instance.ShowSessionRecapStartCurrentMMR &&
				!Config.Instance.ShowSessionRecapLatestGames
			)
			{
				CheckboxShowMinionsBanned.IsChecked = true;
				CheckboxShowStartCurrentMMR.IsChecked = true;
				CheckboxShowLatestGames.IsChecked = true;
			}

			SaveConfig(true);
			if(Core.Game.IsBattlegroundsMatch || (Config.Instance.ShowSessionRecapBetweenGames && Core.Game.CurrentMode == Mode.BACON))
				Core.Overlay.ShowBattlegroundsSession();
			Influx.OnSessionRecapEnabledChanged(true);
		}

		private void CheckboxShowSessionRecap_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecap = false;
			SaveConfig(true);
			if(Core.Game.IsBattlegroundsMatch || Core.Game.CurrentMode == Mode.BACON)
				Core.Overlay.HideBattlegroundsSession();
			Influx.OnSessionRecapEnabledChanged(false);
		}

		private void CheckboxShowSessionRecapBetweenGames_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecapBetweenGames = true;
			SaveConfig(true);
			if (Core.Game.CurrentMode == Mode.BACON)
				Core.Overlay.ShowBattlegroundsSession();
		}

		private void CheckboxShowSessionRecapBetweenGames_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowSessionRecapBetweenGames = false;
			SaveConfig(true);
			if (!Core.Game.IsBattlegroundsMatch)
				Core.Overlay.HideBattlegroundsSession();
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
			if(
				!Config.Instance.ShowSessionRecapStartCurrentMMR &&
				!Config.Instance.ShowSessionRecapLatestGames
			)
			{
				CheckboxShowSessionRecap.IsChecked = false;
				CheckboxShowExternalWindow.IsChecked = false;
			}

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
			if(
				!Config.Instance.ShowSessionRecapMinionsBanned &&
				!Config.Instance.ShowSessionRecapLatestGames
			)
			{
				CheckboxShowSessionRecap.IsChecked = false;
				CheckboxShowExternalWindow.IsChecked = false;
			}

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
			if(
				!Config.Instance.ShowSessionRecapMinionsBanned &&
				!Config.Instance.ShowSessionRecapStartCurrentMMR
			)
			{
				CheckboxShowSessionRecap.IsChecked = false;
				CheckboxShowExternalWindow.IsChecked = false;
			}

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
			if(
				!Config.Instance.ShowSessionRecapMinionsBanned &&
				!Config.Instance.ShowSessionRecapStartCurrentMMR &&
				!Config.Instance.ShowSessionRecapLatestGames
			)
			{
				CheckboxShowMinionsBanned.IsChecked = true;
				CheckboxShowStartCurrentMMR.IsChecked = true;
				CheckboxShowLatestGames.IsChecked = true;
			}

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
	}
}
