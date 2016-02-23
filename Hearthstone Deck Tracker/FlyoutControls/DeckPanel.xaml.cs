#region

using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	/// <summary>
	/// Interaction logic for DeckPanel.xaml
	/// </summary>
	public partial class DeckPanel : UserControl
	{
		private Deck _deck;

		public DeckPanel()
		{
			InitializeComponent();
		}

		private void ButtonImport_OnClick(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.SetNewDeck(_deck);
			Core.MainWindow.FlyoutDeckStats.IsOpen = false;
			Core.MainWindow.FlyoutDeck.IsOpen = false;
		}

		public void SetDeck(Deck deck, bool showImportButton = true)
		{
			_deck = deck;
			ListViewDeck.Items.Clear();
			foreach(var card in deck.Cards.ToSortedCardList())
				ListViewDeck.Items.Add(card);
			Helper.SortCardCollection(ListViewDeck.Items, false);
			ButtonImport.Visibility = showImportButton ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}