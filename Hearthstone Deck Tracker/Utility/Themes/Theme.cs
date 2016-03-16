using System;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public class Theme
	{
		public string Name { get; set; }
		public string Directory { get; set; }
		public Type BuildType { get; set; }

		public Theme(string name, string dir, Type buildType)
		{
			Name = name.ToLowerInvariant();
			Directory = dir;
			BuildType = buildType;
		}

		public override string ToString()
		{
			return Name.Substring(0,1).ToUpperInvariant() 
				+ (Name.Length > 1 ? Name.Substring(1) : "");
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
		GoldenFrame,
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