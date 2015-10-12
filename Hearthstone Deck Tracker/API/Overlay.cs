#region

using System;
using System.Windows.Controls;

#endregion

namespace Hearthstone_Deck_Tracker.API
{
	[Obsolete("Use API.Core.OverlayCanvas")]
	public class Overlay
	{
		[Obsolete("Use API.Core.OverlayCanvas")]
		public static Canvas OverlayCanvas
		{
			get { return Hearthstone_Deck_Tracker.Core.Overlay.CanvasInfo; }
		}
	}
}