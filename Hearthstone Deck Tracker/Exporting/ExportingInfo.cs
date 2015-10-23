#region

using System;
using System.Drawing;

#endregion

namespace Hearthstone_Deck_Tracker.Exporting
{
	public class ExportingInfo
	{
		public ExportingInfo()
		{
			HsHandle = User32.GetHearthstoneWindow();
			HsRect = User32.GetHearthstoneRect(false);
			Ratio = (4.0 / 3.0) / ((double)HsRect.Width / HsRect.Height);
			SearchBoxPos = new Point((int)(Helper.GetScaledXPos(Config.Instance.ExportSearchBoxX, HsRect.Width, Ratio)),
			                         (int)(Config.Instance.ExportSearchBoxY * HsRect.Height));
			CardPosX = Helper.GetScaledXPos(Config.Instance.ExportCard1X, HsRect.Width, Ratio);
			Card2PosX = Helper.GetScaledXPos(Config.Instance.ExportCard2X, HsRect.Width, Ratio);
			CardPosY = Config.Instance.ExportCardsY * HsRect.Height;
		}

		public IntPtr HsHandle { get; set; }
		public Rectangle HsRect { get; set; }
		public double Ratio { get; set; }
		public Point SearchBoxPos { get; set; }
		public double CardPosX { get; set; }
		public double Card2PosX { get; set; }
		public double CardPosY { get; set; }
	}
}