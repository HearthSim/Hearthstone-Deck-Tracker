using System.Windows;
using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;

public partial class OverlayMulligan : UserControl
{
	public OverlayMulligan()
	{
		InitializeComponent();
	}

	private bool _initialized;

	public void Load()
	{
		CheckboxEnableMulliganGV2.IsChecked = Config.Instance.EnableMulliganGV2;
		CheckboxShowMulliganStatsPreLobby.IsChecked = Config.Instance.ShowMulliganGuidePreLobby;

		CheckboxEnableMulliganStats.IsChecked = Config.Instance.EnableMulliganGuide;
		CheckboxAutoShowMulliganStats.IsChecked = Config.Instance.AutoShowMulliganGuide;

		_initialized = true;
	}

	private void SaveConfig(bool updateOverlay)
	{
		Config.Save();
		if(updateOverlay)
			Core.Overlay.Update(true);
	}

	private void CheckboxEnableMulliganGV2_Checked(object sender, RoutedEventArgs e)
	{
		if(!_initialized)
			return;
		Config.Instance.EnableMulliganGV2 = true;
		Config.Save();
		Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
		SaveConfig(true);
	}

	private void CheckboxEnableMulliganGV2_Unchecked(object sender, RoutedEventArgs e)
	{
		if(!_initialized)
			return;
		Config.Instance.EnableMulliganGV2 = false;
		Config.Save();
		Core.Overlay.HideMulliganGuideStats();
		Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
		SaveConfig(true);
	}

	private void CheckboxEnableMulliganStats_Checked(object sender, RoutedEventArgs e)
	{
		if(!_initialized)
			return;
		Config.Instance.EnableMulliganGuide = true;
		Config.Save();
		Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
	}

	private void CheckboxEnableMulliganStats_Unchecked(object sender, RoutedEventArgs e)
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

	private void CheckboxAutoShowMulliganStats_Checked(object sender, RoutedEventArgs e)
	{
		if(!_initialized)
			return;
		Config.Instance.AutoShowMulliganGuide = true;
		Config.Save();
	}

	private void CheckboxAutoShowMulliganStats_Unchecked(object sender, RoutedEventArgs e)
	{
		if(!_initialized)
			return;
		Config.Instance.AutoShowMulliganGuide = false;
		Config.Save();
	}

	private void CheckboxShowMulliganStatsPreLobby_Checked(object sender, RoutedEventArgs e)
	{
		if(!_initialized)
			return;
		Config.Instance.ShowMulliganGuidePreLobby = true;
		Config.Save();
		Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
	}

	private void CheckboxShowMulliganStatsPreLobby_Unchecked(object sender, RoutedEventArgs e)
	{
		if(!_initialized)
			return;
		Config.Instance.ShowMulliganGuidePreLobby = false;
		Config.Save();
		Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
	}

}

