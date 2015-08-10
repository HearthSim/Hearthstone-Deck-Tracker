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

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker.DeckPickerItemLayouts
{
	/// <summary>
	/// Interaction logic for DeckPickerItemLayoutLegacy.xaml
	/// </summary>
	public partial class DeckPickerItemLayoutLegacy : UserControl
	{
		public DeckPickerItemLayoutLegacy()
		{
			InitializeComponent();
		}

		private void UseButton_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			var deck = DataContext as Deck;
			if(deck == null)
				return;
			if(deck.Equals(DeckList.Instance.ActiveDeck))
				return;
			Helper.MainWindow.DeckPickerList.SelectDeck(deck);
			Helper.MainWindow.SelectDeck(deck, true);
			Helper.MainWindow.DeckPickerList.RefreshDisplayedDecks();
		}
	}
}
