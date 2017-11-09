using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Controls.DeckSetIcons
{
	public partial class DeckSetIconsView
	{
		public DeckSetIconsView()
		{
			InitializeComponent();
		}

		public Brush Fill
		{
			get => (Brush)GetValue(FillProperty);
			set => SetValue(FillProperty, value);
		}

		public static readonly DependencyProperty FillProperty =
			DependencyProperty.Register("Fill", typeof(Brush), typeof(DeckSetIconsView),
				new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender));

		public void Update(Deck deck) => ((DeckSetIconsViewModel)DataContext).Deck = deck;
	}
}
