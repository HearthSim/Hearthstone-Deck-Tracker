#region

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Utility.Logging;
using static Hearthstone_Deck_Tracker.User32;
using static Hearthstone_Deck_Tracker.User32.MouseEventFlags;

#endregion

namespace Hearthstone_Deck_Tracker.Exporting
{
	public class MouseActions
	{
		public static async Task ClickOnPoint(IntPtr wndHandle, Point clientPoint)
		{
			ClientToScreen(wndHandle, ref clientPoint);

			Cursor.Position = new Point(clientPoint.X, clientPoint.Y);
			Log.Debug("Clicking " + Cursor.Position);

			//mouse down
			if(SystemInformation.MouseButtonsSwapped)
				mouse_event((uint)RightDown, 0, 0, 0, UIntPtr.Zero);
			else
				mouse_event((uint)LeftDown, 0, 0, 0, UIntPtr.Zero);

			await Task.Delay(Config.Instance.DeckExportDelay);

			//mouse up
			if(SystemInformation.MouseButtonsSwapped)
				mouse_event((uint)RightUp, 0, 0, 0, UIntPtr.Zero);
			else
				mouse_event((uint)LeftUp, 0, 0, 0, UIntPtr.Zero);

			await Task.Delay(Config.Instance.DeckExportDelay);
		}
	}
}