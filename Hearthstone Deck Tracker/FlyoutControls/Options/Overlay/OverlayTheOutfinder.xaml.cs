using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.RelatedCardsPanel;

namespace Hearthstone_Deck_Tracker.FlyoutControls.Options.Overlay;

public partial class OverlayTheOutfinder : UserControl
{
	public OverlayTheOutfinder()
	{
		InitializeComponent();
	}

	private bool _initialized;

	public void Load()
	{
		CheckboxOutfinderEnabled.IsChecked = Config.Instance.OutfinderEnabled;
		CheckboxOutfinderInDeck.IsChecked = Config.Instance.OutfinderInDeck;
		CheckboxOutfinderInHand.IsChecked = Config.Instance.OutfinderInHand;
		CheckboxOutfinderUsePercentages.IsChecked = Config.Instance.OutfinderUsePercentages;
		CheckboxOutfinderUseCardTiles.IsChecked = Config.Instance.OutfinderUseCardTiles;

		_initialized = true;
	}

	private void CheckboxOutfinderEnabled_Checked(object sender, RoutedEventArgs e)
	{
		if(!_initialized) return;
		Config.Instance.OutfinderEnabled = true;
		Config.Save();
	}

	private void CheckboxOutfinderEnabled_Unchecked(object sender, RoutedEventArgs e)
	{
		if(!_initialized) return;
		Config.Instance.OutfinderEnabled = false;
		Config.Save();
	}

	private void CheckboxOutfinderInDeck_Checked(object sender, RoutedEventArgs e)
	{
		if(!_initialized) return;
		Config.Instance.OutfinderInDeck = true;
		Config.Save();
	}

	private void CheckboxOutfinderInDeck_Unchecked(object sender, RoutedEventArgs e)
	{
		if(!_initialized) return;
		Config.Instance.OutfinderInDeck = false;
		Config.Save();
	}

	private void CheckboxOutfinderInHand_Checked(object sender, RoutedEventArgs e)
	{
		if(!_initialized) return;
		Config.Instance.OutfinderInHand = true;
		Config.Save();
	}

	private void CheckboxOutfinderInHand_Unchecked(object sender, RoutedEventArgs e)
	{
		if(!_initialized) return;
		Config.Instance.OutfinderInHand = false;
		Config.Save();
	}

	private void CheckboxOutfinderUsePercentages_Checked(object sender, RoutedEventArgs e)
	{
		if(!_initialized) return;
		Config.Instance.OutfinderUsePercentages = true;
		Config.Save();
	}

	private void CheckboxOutfinderUsePercentages_Unchecked(object sender, RoutedEventArgs e)
	{
		if(!_initialized) return;
		Config.Instance.OutfinderUsePercentages = false;
		Config.Save();
	}

	private void CheckboxOutfinderUseCardTiles_Checked(object sender, RoutedEventArgs e)
	{
		if(!_initialized) return;
		SetUseCardTiles(true);
	}

	private void CheckboxOutfinderUseCardTiles_Unchecked(object sender, RoutedEventArgs e)
	{
		if(!_initialized) return;
		SetUseCardTiles(false);
	}

	private static void SetUseCardTiles(bool value)
	{
		Config.Instance.OutfinderUseCardTiles = value;
		Config.Save();
		RelatedCardsPanelViewModel.RaiseDisplayModeChanged();
	}
}
