using System.Windows.Controls;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class HoverCardDisplay : UserControl
	{
		public HoverCardDisplay()
		{
			InitializeComponent();
		}

		public void SetCard(Hearthstone.Card card)
		{
			CardObj.DataContext = card;
		}
	}
}
