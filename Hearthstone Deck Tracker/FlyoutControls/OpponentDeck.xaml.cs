#region

using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls
{
	/// <summary>
	/// Interaction logic for OpponentDeck.xaml
	/// </summary>
	public partial class OpponentDeck : UserControl
	{
		private Deck _deck;

		public OpponentDeck()
		{
			InitializeComponent();
		}

		private void ButtonImport_OnClick(object sender, RoutedEventArgs e)
		{
			Core.MainWindow.SetNewDeck(_deck);
			Core.MainWindow.FlyoutDeckStats.IsOpen = false;
			Core.MainWindow.FlyoutOpponentDeck.IsOpen = false;
		}

		public void SetDeck(Deck deck)
		{
			_deck = deck;
			ListViewDeck.Items.Clear();
			foreach(var card in deck.Cards)
				ListViewDeck.Items.Add(card);
			Helper.SortCardCollection(ListViewDeck.Items, false);
		}
	}
}