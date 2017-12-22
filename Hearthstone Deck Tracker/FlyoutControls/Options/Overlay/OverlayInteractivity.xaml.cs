#region

using System.Windows;
using System.Windows.Controls;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay
{
	/// <summary>
	/// Interaction logic for OverlayInteractivity.xaml
	/// </summary>
	public partial class OverlayInteractivity : UserControl
	{
		private bool _initialized;

		public OverlayInteractivity()
		{
			InitializeComponent();
		}

		public void Load()
		{
			ToggleSwitchExtraFeatures.IsChecked = Config.Instance.ExtraFeatures;
			CheckBoxForceExtraFeatures.IsChecked = Config.Instance.ForceMouseHook;
			CheckBoxSecrets.IsChecked = Config.Instance.ExtraFeaturesSecrets;
			CheckBoxFriendslist.IsChecked = Config.Instance.ExtraFeaturesFriendslist;
			_initialized = true;
		}

		private void ToggleSwitchExtraFeatures_Checked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExtraFeatures = true;
			Config.Save();
		}

		private void ToggleSwitchExtraFeatures_Unchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExtraFeatures = false;
			Config.Save();
		}

		private void CheckBoxForceExtraFeatures_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ForceMouseHook = true;
			Core.Overlay.HookMouse();
			Config.Save();
		}

		private void CheckBoxForceExtraFeatures_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ForceMouseHook = false;
			Core.Overlay.UnHookMouse();
			Config.Save();
		}

		private void CheckBoxSecrets_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExtraFeaturesSecrets = true;
			Config.Save();
		}

		private void CheckBoxSecrets_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExtraFeaturesSecrets = false;
			Config.Save();
		}

		private void CheckBoxFriendslist_OnChecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExtraFeaturesFriendslist = true;
			Config.Save();
		}

		private void CheckBoxFriendslist_OnUnchecked(object sender, RoutedEventArgs e)
		{
			if(!_initialized)
				return;
			Config.Instance.ExtraFeaturesFriendslist = false;
			Config.Save();
		}
	}
}
