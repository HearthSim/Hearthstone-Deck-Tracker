using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public class CardTextImageBuilder
	{
		private static readonly Typeface TypeFace = new Typeface(new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Belwe Bd BT"),
															FontStyles.Normal, FontWeights.Normal, FontStretches.Condensed);

		public static GeometryDrawing[] GetOutlinedText(string text, double maxFontSize, Rect rect, Brush fill, Brush stroke,
												  double strokeThickness = 2, bool centered = false)
		{
			var fText = new FormattedText(text, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, TypeFace, maxFontSize, fill);
			if(!double.IsNaN(rect.Width))
			{
				if(fText.Width > rect.Width)
					fText.SetFontSize((int)(maxFontSize * rect.Width / fText.Width));
				fText.MaxTextWidth = rect.Width;
			}
			var point = new Point(rect.X + (centered && !double.IsNaN(rect.Width) ? (rect.Width - fText.Width) / 2 : 0), !double.IsNaN(rect.Height) ? (rect.Height - fText.Height) / 2 : rect.Y);
			var drawings = new[]
			{
				new GeometryDrawing(stroke, new Pen(Brushes.Black, strokeThickness), fText.BuildGeometry(point)),
				new GeometryDrawing(fill, new Pen(Brushes.White, 0), fText.BuildGeometry(point))
			};
			return drawings;
		}
	}
}