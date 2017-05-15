using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckHistory
{
	public partial class DeckHistoryView : UserControl
	{
		public DeckHistoryView()
		{
			InitializeComponent();
		}

		public Deck Deck
		{
			get => ((DeckHistoryViewModel)DataContext).Deck;
			set => ((DeckHistoryViewModel)DataContext).Deck = value;
		}
	}
}
