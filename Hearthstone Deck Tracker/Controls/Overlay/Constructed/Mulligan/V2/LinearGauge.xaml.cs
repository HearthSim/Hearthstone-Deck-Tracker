using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan.V2;

public partial class LinearGauge : UserControl
{
	public LinearGauge()
	{
		InitializeComponent();
	}

	private readonly Dictionary<string, TaskCompletionSource<bool>> _runningStoryBoards = new();

	private void OnSizeChanged(object sender, SizeChangedEventArgs e)
	{
		var sb = (Storyboard)FindResource("StoryboardFadeOut");
		sb.Stop();
		sb.Begin();
	}
}

