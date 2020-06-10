#region

using System;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay
{
	/// <summary>
	/// Interaction logic for Overlay.xaml
	/// </summary>
	public partial class OverlayBattlegrounds
	{
		private bool _initialized;

		public OverlayBattlegrounds()
		{
			InitializeComponent();
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
			_initialized = true;
		}

		internal void UpdateDisabledState()
		{
			var enabled = true;
			if(RemoteConfig.Instance.Data?.BobsBuddy?.Disabled ?? false)
			{
				TextBobsBuddyDisabled.Text = "Temporarily Disabled";
				enabled = false;
			}
			else
			{
				var verStr = RemoteConfig.Instance.Data?.BobsBuddy?.MinRequiredVersion;
				if(Version.TryParse(verStr, out var requiredVersion) && requiredVersion > Helper.GetCurrentVersion())
				{
					TextBobsBuddyDisabled.Text = $"Requires HDT v{requiredVersion}";
					enabled = false;
				}
			}
			TextBobsBuddyDisabled.Visibility = enabled ? Visibility.Collapsed : Visibility.Visible;
			CheckboxShowResultsDuringCombat.IsEnabled = enabled && Config.Instance.RunBobsBuddy;
			CheckboxShowResultsDuringShopping.IsEnabled = enabled && Config.Instance.RunBobsBuddy;
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
	}
}
