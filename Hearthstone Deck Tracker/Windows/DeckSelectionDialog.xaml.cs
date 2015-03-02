#region

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for DeckSelectionDialog.xaml
	/// </summary>
	public partial class DeckSelectionDialog
	{
		public Deck SelectedDeck;

		public DeckSelectionDialog(IEnumerable<Deck> decks)
		{
			InitializeComponent();

			WindowStartupLocation = WindowStartupLocation.CenterScreen;
			ListViewDecks.Items.Clear();
			foreach(var deck in decks.OrderByDescending(d => d.Name))
				ListViewDecks.Items.Add(new NewDeckPickerItem(deck));
		}

		private void ListViewDecks_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var item = ListViewDecks.SelectedItem as NewDeckPickerItem;
			if(item != null)
				SelectedDeck = item.Deck;
			Close();
		}
	}
}