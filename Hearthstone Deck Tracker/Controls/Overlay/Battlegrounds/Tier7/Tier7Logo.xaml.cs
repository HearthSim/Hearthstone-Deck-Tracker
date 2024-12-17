using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7
{
	public partial class Tier7Logo : UserControl
	{
		// Register a DependencyProperty for the brush
		public static readonly DependencyProperty LogoBrushProperty =
			DependencyProperty.Register("LogoBrush", typeof(Brush), typeof(Tier7Logo), new PropertyMetadata(Application.Current.Resources["Tier7Orange"]));


		public Brush LogoBrush
		{
			get { return (Brush)GetValue(LogoBrushProperty); }
			set { SetValue(LogoBrushProperty, value); }
		}

		public Tier7Logo()
		{
			InitializeComponent();
		}
	}
}
