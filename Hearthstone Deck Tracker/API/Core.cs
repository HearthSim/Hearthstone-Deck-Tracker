#region

using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.API
{
	public class Core
	{
		public static GameV2 Game { get { return Hearthstone_Deck_Tracker.Core.Game; } }

		public static Canvas OverlayCanvas
		{
			get { return Hearthstone_Deck_Tracker.Core.Overlay.CanvasInfo; }
		}

		public static OverlayWindow OverlayWindow
		{
			get { return Hearthstone_Deck_Tracker.Core.Overlay; }
		}

		public static Windows.MainWindow MainWindow
		{
			get { return Hearthstone_Deck_Tracker.Core.MainWindow; }
		}
	}
}