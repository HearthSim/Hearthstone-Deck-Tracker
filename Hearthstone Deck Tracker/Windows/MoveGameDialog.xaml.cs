using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for MoveGameDialog.xaml
	/// </summary>
	public partial class MoveGameDialog
	{
		public Deck SelectedDeck;

		public MoveGameDialog(IEnumerable<Deck> decks)
		{
			InitializeComponent();

			WindowStartupLocation = WindowStartupLocation.CenterOwner;
			ListboxPicker.Items.Clear();
			foreach(var deck in decks)
				ListboxPicker.Items.Add(deck);
		}

		private void DeckPickerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectedDeck = ListboxPicker.SelectedItem as Deck;
			Close();
		}
	}
}