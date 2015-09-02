#region

using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.API
{
	public class Core
	{
		public static GameV2 Game { get; internal set; }

		public static Canvas OverlayCanvas
		{
			get { return Helper.MainWindow.Overlay.CanvasInfo; }
		}

		public static MainWindow MainWindow
		{
			get { return Helper.MainWindow; }
		}
	}
}