using System;
using System.IO;
using System.Linq;
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
		public OverlayTheme OverlayTheme { get; }

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
			var overlayDir = new DirectoryInfo(dir).Parent?.GetDirectories().FirstOrDefault(x => x.Name == "Overlay");
			OverlayTheme = new OverlayTheme(Name, overlayDir?.FullName);
		}

		public override string ToString() => Name.Substring(0, 1).ToUpperInvariant() + (Name.Length > 1 ? Name.Substring(1) : "");
	}

	public class OverlayTheme
	{
		public OverlayTheme(string name, string dir = null)
		{
			Name = name;
			Directory = dir ?? LocalDirectory;
		}

		public string Directory { get; }
		public string Name { get; }
		public const string LocalDirectory = @"Images\Themes\Overlay";

		private ImageBrush _cardCounterFrame;
		private ImageBrush _playerChanceFrame;
		private ImageBrush _opponentChangeFrame;
		public ImageBrush CardCounterFrame => _cardCounterFrame ?? (_cardCounterFrame = GetOverlayImage(Name, "card-counter-frame.png"));
		public ImageBrush PlayerChanceFrame => _playerChanceFrame ?? (_playerChanceFrame = GetOverlayImage(Name, "player-chance-frame.png"));
		public ImageBrush OpponentChanceFrame => _opponentChangeFrame ?? (_opponentChangeFrame = GetOverlayImage(Name, "opponent-chance-frame.png"));

		private ImageBrush GetOverlayImage(string name, string image, bool forceLocal = false)
		{
			var file = Path.Combine(forceLocal ? LocalDirectory : Directory, name, image);
			if(File.Exists(file))
				return new ImageBrush(new BitmapImage(new Uri(file, UriKind.Relative)));
			if(name != "default")
				return GetOverlayImage("default", image);
			return new ImageBrush();
		}
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
