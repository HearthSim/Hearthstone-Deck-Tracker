using System.Windows.Controls;
using System.Windows.Input;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.RelatedCardsPanel;

public partial class RelatedCardsPanel : UserControl
{
	public RelatedCardsPanel()
	{
		InitializeComponent();
	}

	private void CardTile_OnLoaded(object sender, System.Windows.RoutedEventArgs e)
		=> (sender as CardTile)?.Subscribe();

	private void CardTile_OnUnloaded(object sender, System.Windows.RoutedEventArgs e)
		=> (sender as CardTile)?.Unsubscribe();

	private void CardTile_OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		=> e.Handled = true;
}
