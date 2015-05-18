#region

using System.Windows.Controls;

#endregion

namespace Hearthstone_Deck_Tracker.API
{
	public class Overlay
	{
		public static Canvas OverlayCanvas
		{
			get { return Helper.MainWindow.Overlay.CanvasInfo; }
		}
	}
}