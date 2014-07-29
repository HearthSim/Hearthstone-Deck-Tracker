using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for DeckNotes.xaml
	/// </summary>
	public partial class DeckNotes
	{
		private Deck _currentDeck;

		public DeckNotes()
		{
			InitializeComponent();
		}

		public void SetDeck(Deck deck)
		{
			_currentDeck = deck;
			Textbox.Text = deck.Note;
		}

		private void Textbox_TextChanged(object sender, TextChangedEventArgs e)
		{
			_currentDeck.Note = Textbox.Text;
		}
	}
}