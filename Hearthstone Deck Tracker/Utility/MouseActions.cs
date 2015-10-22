#region

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

#endregion

namespace Hearthstone_Deck_Tracker.Exporting
{
	public class MouseActions
	{
		public static async Task ClickOnPoint(IntPtr wndHandle, Point clientPoint)
		{
			User32.ClientToScreen(wndHandle, ref clientPoint);

			Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

			//mouse down
			if(SystemInformation.MouseButtonsSwapped)
				User32.mouse_event((uint)User32.MouseEventFlags.RightDown, 0, 0, 0, UIntPtr.Zero);
			else
				User32.mouse_event((uint)User32.MouseEventFlags.LeftDown, 0, 0, 0, UIntPtr.Zero);

			await Task.Delay(Config.Instance.DeckExportDelay);

			//mouse up
			if(SystemInformation.MouseButtonsSwapped)
				User32.mouse_event((uint)User32.MouseEventFlags.RightUp, 0, 0, 0, UIntPtr.Zero);
			else
				User32.mouse_event((uint)User32.MouseEventFlags.LeftUp, 0, 0, 0, UIntPtr.Zero);

			await Task.Delay(Config.Instance.DeckExportDelay);
		}
	}
}