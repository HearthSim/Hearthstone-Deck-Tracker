using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckExport
{
	public partial class DeckExportView
	{
		public DeckExportView()
		{
			InitializeComponent();
		}

		public Deck Deck
		{
			get { return ((DeckExportViewModel)DataContext).Deck; }
			set { ((DeckExportViewModel)DataContext).Deck = value; }
		}
	}
}
