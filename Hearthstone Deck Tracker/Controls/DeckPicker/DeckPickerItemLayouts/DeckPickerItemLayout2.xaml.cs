#region

using System.Windows.Input;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Controls.DeckPicker.DeckPickerItemLayouts
{
	/// <summary>
	/// Interaction logic for DeckPickerItemLayout2.xaml
	/// </summary>
	public partial class DeckPickerItemLayout2
	{
		public DeckPickerItemLayout2()
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
			Core.MainWindow.DeckPickerList.SelectDeck(deck);
			Core.MainWindow.SelectDeck(deck, true);
			Core.MainWindow.DeckPickerList.RefreshDisplayedDecks();
		}
	}
}
