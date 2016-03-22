using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public class Theme
	{
		private ImageBrush _highlightImage;
		public string Name { get; set; }
		public string Directory { get; set; }
		public Type BuildType { get; set; }

		public ImageBrush HighlightImage => _highlightImage ?? (_highlightImage = GetHighlightImage());

		private ImageBrush GetHighlightImage()
		{
			var file = Path.Combine(Directory, "highlight.png");
			if(File.Exists(file))
				return new ImageBrush(new BitmapImage(new Uri(file, UriKind.Relative)));
			Log.Warn($"highlight.png for theme '{Name}' does not exist.");
			return new ImageBrush();
		}

		public Theme(string name, string dir, Type buildType)
		{
			Name = name.ToLowerInvariant();
			Directory = dir;
			BuildType = buildType;
		}

		public override string ToString() => Name.Substring(0, 1).ToUpperInvariant() + (Name.Length > 1 ? Name.Substring(1) : "");
	}

	public class ThemeElementInfo
	{
		public string FileName { get; set; }
		public Rect Rectangle { get; set; }

		public ThemeElementInfo()
		{
			FileName = null;
			Rectangle = new Rect();
		}

		public ThemeElementInfo(string fileName, Rect rect)
		{
			FileName = fileName;
			Rectangle = rect;
		}

		public ThemeElementInfo(string fileName,
			double x, double y, double w, double h)
		{
			FileName = fileName;
			Rectangle = new Rect(x, y, w, h);
		}
	}

	public enum ThemeElement
	{
		DefaultFrame,
		CommonFrame,
		RareFrame,
		EpicFrame,
		LegendaryFrame,
		DefaultGem,
		CommonGem,
		RareGem,
		EpicGem,
		LegendaryGem,
		DefaultCountBox,
		CommonCountBox,
		RareCountBox,
		EpicCountBox,
		LegendaryCountBox,
		LegendaryIcon,
		CreatedIcon,
		DarkOverlay,
		FadeOverlay
	}
}