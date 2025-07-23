using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public partial class ArenasmithLogo : UserControl
{
	// Register a DependencyProperty for the brush
	public static readonly DependencyProperty LogoBrushProperty =
		DependencyProperty.Register("LogoBrush", typeof(Brush), typeof(ArenasmithLogo), new PropertyMetadata(Application.Current.Resources["Tier7Orange"]));


	public Brush LogoBrush
	{
		get { return (Brush)GetValue(LogoBrushProperty); }
		set { SetValue(LogoBrushProperty, value); }
	}

	public ArenasmithLogo()
	{
		InitializeComponent();
	}
}
