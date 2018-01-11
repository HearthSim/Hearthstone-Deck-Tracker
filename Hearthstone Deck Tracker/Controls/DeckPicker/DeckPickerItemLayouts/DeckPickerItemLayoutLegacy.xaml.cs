#region

using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

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
		    if(!(DataContext is Deck deck))
				return;
			if(deck.Equals(DeckList.Instance.ActiveDeck))
				return;
			Core.MainWindow.DeckPickerList.SelectDeck(deck);
			Core.MainWindow.SelectDeck(deck, true);
			Core.MainWindow.DeckPickerList.RefreshDisplayedDecks();
		}
	}
}
