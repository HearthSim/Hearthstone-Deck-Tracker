#region

using System;
using System.Windows.Controls;

#endregion

namespace Hearthstone_Deck_Tracker.API
{
	[Obsolete("Use API.Core.OverlayCanvas", true)]
	public class Overlay
	{
		[Obsolete("Use API.Core.OverlayCanvas", true)]
		public static Canvas OverlayCanvas => Hearthstone_Deck_Tracker.Core.Overlay.CanvasInfo;
	}
}