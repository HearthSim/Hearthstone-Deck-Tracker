using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public class CardTextImageBuilder
	{
		public static GeometryDrawing[] GetOutlinedText(string text, double maxFontSize, Rect rect, Brush background, Brush stroke, Typeface typeFace, Brush foreground, double pixelsPerDip,
												  double strokeThickness = 2, bool centered = false)
		{
			var fText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, typeFace, maxFontSize, foreground, pixelsPerDip);
			if(!double.IsNaN(rect.Width))
			{
				if(fText.Width > rect.Width)
					fText.SetFontSize((int)(maxFontSize * rect.Width / fText.Width));
				fText.MaxTextWidth = rect.Width;
			}

			var point = new Point(rect.X + (centered && !double.IsNaN(rect.Width) ? (rect.Width - fText.Width) / 2 : 0), !double.IsNaN(rect.Height) ? (rect.Height - fText.Height) / 2 + fText.Height  * 0.05: rect.Y);
			var drawings = new[]
			{
				new GeometryDrawing(stroke, new Pen(Brushes.Black, strokeThickness) {LineJoin = PenLineJoin.Round}, fText.BuildGeometry(point)),
				new GeometryDrawing(background, new Pen(foreground, 0), fText.BuildGeometry(point))
			};
			return drawings;
		}
	}
}
