#region

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public static class ScreenCapture
	{
		public static Bitmap CaptureHearthstone(Point point, int width, int height, IntPtr wndHandle = default(IntPtr),
												bool requireInForeground = true) => Task.Run(async () => await CaptureHearthstoneAsync(point, width, height, wndHandle, requireInForeground)).Result;

		public static async Task<Bitmap> CaptureHearthstoneAsync(Point point, int width, int height, IntPtr wndHandle = default(IntPtr),
																 bool requireInForeground = true, bool? altScreenCapture = null)
		{
			if(wndHandle == default(IntPtr))
				wndHandle = User32.GetHearthstoneWindow();

			if(requireInForeground && !User32.IsHearthstoneInForeground())
				return null;

			try
			{
				if(altScreenCapture ?? Config.Instance.AlternativeScreenCapture)
					return await Task.Run(() => CaptureWindow(wndHandle, point, width, height));
				return await Task.Run(() => CaptureScreen(wndHandle, point, width, height));
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}

		public static Bitmap CaptureWindow(IntPtr wndHandle, Point point, int width, int height)
		{
			User32.GetWindowRect(wndHandle, out var windowRect);
			var windowWidth = windowRect.right - windowRect.left;
			var windowHeight = windowRect.bottom - windowRect.top;
			var bmp = new Bitmap(windowWidth, windowHeight, PixelFormat.Format32bppArgb);
			using(var graphics = Graphics.FromImage(bmp))
			{
				var hdc = graphics.GetHdc();

				try
				{
					User32.PrintWindow(wndHandle, hdc, 0);
				}
				finally
				{
					graphics.ReleaseHdc(hdc);
				}
			}
			var cRect = new User32.Rect();
			User32.GetClientRect(wndHandle, ref cRect);
			var cWidth = cRect.right - cRect.left;
			var cHeight = cRect.bottom - cRect.top;
			var captionHeight = windowHeight - cHeight > 0 ? SystemInformation.CaptionHeight : 0;
			return
				bmp.Clone(
						  new Rectangle((windowWidth - cWidth) / 2 + point.X, (windowHeight - cHeight - captionHeight) / 2 + captionHeight + point.Y,
										width, height), PixelFormat.Format32bppArgb);
		}

		public static Bitmap CaptureScreen(IntPtr wndHandle, Point point, int width, int height)
		{
			User32.ClientToScreen(wndHandle, ref point);
			var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			var graphics = Graphics.FromImage(bmp);
			graphics.CopyFromScreen(point.X, point.Y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
			return bmp;
		}
	}
}
