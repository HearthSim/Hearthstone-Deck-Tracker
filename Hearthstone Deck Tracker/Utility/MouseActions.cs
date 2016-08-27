#region

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Exporting;using Hearthstone_Deck_Tracker.Utility.Exceptions;using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public class MouseActions	{		private ExportingInfo _info;
		private readonly Func<Task<bool>> _onUnexpectedMousePos;

		public MouseActions(ExportingInfo info, Func<Task<bool>> onUnexpectedMousePos)		{			_info = info;
			_onUnexpectedMousePos = onUnexpectedMousePos;
		}

		private Point _previousCursorPos = Point.Empty;
		public async Task ClickOnPoint(Point clientPoint)
		{
			if(!User32.IsHearthstoneInForeground() || (_onUnexpectedMousePos != null && _previousCursorPos != Point.Empty &&
				(Math.Abs(_previousCursorPos.X - Cursor.Position.X) > 10 || Math.Abs(_previousCursorPos.Y - Cursor.Position.Y) > 10)))
			{
				if(!(_onUnexpectedMousePos == null || await _onUnexpectedMousePos()))
					throw new ExportingInterruptedException("Export interrupted, not continuing");
				if((_info = await ExportingHelper.EnsureHearthstoneInForeground(_info)) == null)
					throw new ExportingInterruptedException("Export interrupted - could not re-focus hearthstone");
				await Task.Delay(500);
			}

			User32.ClientToScreen(_info.HsHandle, ref clientPoint);
			Cursor.Position = _previousCursorPos = new Point(clientPoint.X, clientPoint.Y);
			Log.Debug("Clicking " + Cursor.Position);

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