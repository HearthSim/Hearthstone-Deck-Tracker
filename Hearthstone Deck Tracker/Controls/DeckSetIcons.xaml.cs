using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Hearthstone;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class DeckSetIcons : UserControl
	{
		public DeckSetIcons()
		{
			InitializeComponent();
		}

		public Brush Fill
		{
			get { return (Brush)GetValue(FillProperty); }
			set { SetValue(FillProperty, value); }
		}

		public static readonly DependencyProperty FillProperty =
			DependencyProperty.Register("Fill", typeof(Brush), typeof(DeckSetIcons),
				new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

		public void Update(Deck deck)
		{
			RectIconOg.Visibility = deck?.ContainsSet("Whispers of the Old Gods") ?? false ? Visible : Collapsed;
			RectIconLoe.Visibility = deck?.ContainsSet("League of Explorers") ?? false ? Visible : Collapsed;
			RectIconTgt.Visibility = deck?.ContainsSet("The Grand Tournament") ?? false ? Visible : Collapsed;
			RectIconBrm.Visibility = deck?.ContainsSet("Blackrock Mountain") ?? false ? Visible : Collapsed;
			RectIconGvg.Visibility = deck?.ContainsSet("Goblins vs Gnomes") ?? false ? Visible : Collapsed;
			RectIconNaxx.Visibility = deck?.ContainsSet("Curse of Naxxramas") ?? false ? Visible : Collapsed;
		}
	}
}