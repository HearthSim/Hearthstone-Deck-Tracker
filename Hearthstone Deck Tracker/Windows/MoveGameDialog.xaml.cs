using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls;

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
