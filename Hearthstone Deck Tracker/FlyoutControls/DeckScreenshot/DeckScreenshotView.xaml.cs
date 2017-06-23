using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Deck = Hearthstone_Deck_Tracker.Hearthstone.Deck;

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckScreenshot
{
	public partial class DeckScreenshotView : UserControl
	{
		public DeckScreenshotView()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty DeckProperty = DependencyProperty.Register(
			"Deck", typeof(Deck), typeof(DeckScreenshotView), new PropertyMetadata(default(Deck)));

		public Deck Deck
		{
			set { ((DeckScreenshotViewModel)DataContext).Deck = value; }
		}

		private void Hyperlink_OnRequestNavigate(object sender, RequestNavigateEventArgs e)
			=> Helper.TryOpenUrl(e.Uri.AbsoluteUri);
	}
}
