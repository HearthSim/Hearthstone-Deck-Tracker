using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay
{
	public partial class OverlayMercenaries : UserControl
	{
		private bool _initialized;

		public OverlayMercenaries()
		{
			InitializeComponent();
		}

		public void Load()
		{
			CheckboxShowMercsOpponentHoverAbilities.IsChecked = Config.Instance.ShowMercsOpponentHover;
			CheckboxShowMercsPlayerHoverAbilities.IsChecked = Config.Instance.ShowMercsPlayerHover;

			CheckboxShowMercsOpponentAbilityIcons.IsChecked = Config.Instance.ShowMercsOpponentAbilityIcons;
			CheckboxShowMercsPlayerAbilityIcons.IsChecked = Config.Instance.ShowMercsPlayerAbilityIcons;

			_initialized = true;
		}

		private void SaveConfig(bool updateOverlay)
		{
			Config.Save();
			if(updateOverlay)
				Core.Overlay.UpdateMercenariesOverlay();
		}

		private void CheckboxShowMercsOpponentHoverAbilities_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMercsOpponentHover = true;
			SaveConfig(true);
		}

		private void CheckboxShowMercsOpponentHoverAbilities_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMercsOpponentHover = false;
			SaveConfig(true);
		}

		private void CheckboxShowMercsPlayerHoverAbilities_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMercsPlayerHover = true;
			SaveConfig(true);
		}

		private void CheckboxShowMercsPlayerHoverAbilities_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMercsPlayerHover = false;
			SaveConfig(true);
		}

		private void CheckboxShowMercsOpponentAbilityIcons_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMercsOpponentAbilityIcons = true;
			SaveConfig(true);
		}

		private void CheckboxShowMercsOpponentAbilityIcons_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMercsOpponentAbilityIcons = false;
			SaveConfig(true);
		}

		private void CheckboxShowMercsPlayerAbilityIcons_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMercsPlayerAbilityIcons = true;
			SaveConfig(true);
		}

		private void CheckboxShowMercsPlayerAbilityIcons_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ShowMercsPlayerAbilityIcons = false;
			SaveConfig(true);
		}
	}
}
