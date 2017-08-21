using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using MahApps.Metro;
using Point = System.Windows.Point;
using Application = System.Windows.Application;

namespace Hearthstone_Deck_Tracker
{
	public static class UITheme
	{
		public static AppTheme CurrentTheme => GetAppTheme();
		public static Accent CurrentAccent => GetAppAccent();

		public static AppTheme GetAppTheme() => ThemeManager.AppThemes.FirstOrDefault(t => t.Name == Config.Instance.AppTheme.ToString()) ?? ThemeManager.DetectAppStyle().Item1;
		public static Accent GetAppAccent() => ThemeManager.Accents.FirstOrDefault(a => a.Name == Config.Instance.AccentName) ?? ThemeManager.GetAccent("Blue");

		public static void InitializeTheme()
		{
			// We need to check if user is using Windows8 or Windows10 to generate "Windows Accent" theme.
			if (Helper.IsWindows8() || Helper.IsWindows10())
			{
				if (Config.Instance.AccentName == "Windows Accent")
					CreateWindowsAccentStyle(true); // Change application style immediately for "Windows Accent" on HDT launch if user have set accent to "Windows Accent" in config.
				else
					CreateWindowsAccentStyle(); // Generate "Windows Accent", but dont change it immediately

				return; // Leave InitalizeTheme already for Windows8 or Windows10
			}

			// This happens on Windowses diffrent than Windows8 or Windows10
			if (Config.Instance.AccentName == "Windows Accent")
			{
				Config.Instance.AccentName = "Blue"; //In case if somehow user will get "Windows Accent" on Windows which not support this. (For example move whole HDT on diffrent machine instead of fresh install)
				Config.Save();
			}

			ThemeManager.ChangeAppStyle(Application.Current, CurrentAccent, CurrentTheme);
		}

		// \/ Methods are same, but separate them for future options like custom hex selector
		public static void UpdateTheme()
		{
			if (Config.Instance.AccentName == "Windows Accent")
				CreateWindowsAccentStyle(); //Rewrite .XAML style for new WindowsAccent color while changing Theme

			ThemeManager.ChangeAppStyle(Application.Current, CurrentAccent, CurrentTheme);
		}

		public static void UpdateAccent()
		{
			if (Config.Instance.AccentName == "Windows Accent")
				CreateWindowsAccentStyle(); //Rewrite .XAML style for new WindowsAccent color while changing Accent

			ThemeManager.ChangeAppStyle(Application.Current, CurrentAccent, CurrentTheme);
		}
		// /\

		public static void CreateWindowsAccentStyle(bool changeImmediately = false)
		{
			var resourceDictionary = new ResourceDictionary(); // Initialize new colors dictionary
			var color = SystemParameters.WindowGlassColor; // Detect WindowsAccent color

			resourceDictionary.Add("HighlightColor", color);
			resourceDictionary.Add("AccentColor", Color.FromArgb((byte)(204), color.R, color.G, color.B));
			resourceDictionary.Add("AccentColor2", Color.FromArgb((byte)(153), color.R, color.G, color.B));
			resourceDictionary.Add("AccentColor3", Color.FromArgb((byte)(102), color.R, color.G, color.B));
			resourceDictionary.Add("AccentColor4", Color.FromArgb((byte)(51), color.R, color.G, color.B));
			resourceDictionary.Add("HighlightBrush", new SolidColorBrush((Color)resourceDictionary["HighlightColor"]));
			resourceDictionary.Add("AccentColorBrush", new SolidColorBrush((Color)resourceDictionary["AccentColor"]));
			resourceDictionary.Add("AccentColorBrush2", new SolidColorBrush((Color)resourceDictionary["AccentColor2"]));
			resourceDictionary.Add("AccentColorBrush3", new SolidColorBrush((Color)resourceDictionary["AccentColor3"]));
			resourceDictionary.Add("AccentColorBrush4", new SolidColorBrush((Color)resourceDictionary["AccentColor4"]));
			resourceDictionary.Add("WindowTitleColorBrush", new SolidColorBrush((Color)resourceDictionary["AccentColor"]));
			resourceDictionary.Add("ProgressBrush", new LinearGradientBrush(new GradientStopCollection(new[]
				{
					new GradientStop((Color)resourceDictionary["HighlightColor"], 0),
					new GradientStop((Color)resourceDictionary["AccentColor3"], 1)
				}),
				new Point(0.001, 0.5), new Point(1.002, 0.5)));

			resourceDictionary.Add("CheckmarkFill", new SolidColorBrush((Color)resourceDictionary["AccentColor"]));
			resourceDictionary.Add("RightArrowFill", new SolidColorBrush((Color)resourceDictionary["AccentColor"]));

			resourceDictionary.Add("IdealForegroundColor", Colors.White);
			/* For IdealForegroundColor color can be created programmatically too, based on the brightness of the color parameter,
			 * like this: resourceDictionary.Add("IdealForegroundColor", (int)Math.Sqrt(color.R * color.R * .241 + color.G * color.G * .691 + color.B * color.B * .068) < 130 ? Colors.White : Colors.Black);
			*/

			resourceDictionary.Add("IdealForegroundColorBrush", new SolidColorBrush((Color)resourceDictionary["IdealForegroundColor"]));
			resourceDictionary.Add("AccentSelectedColorBrush", new SolidColorBrush((Color)resourceDictionary["IdealForegroundColor"]));
			resourceDictionary.Add("MetroDataGrid.HighlightBrush", new SolidColorBrush((Color)resourceDictionary["AccentColor"]));
			resourceDictionary.Add("MetroDataGrid.HighlightTextBrush", new SolidColorBrush((Color)resourceDictionary["IdealForegroundColor"]));
			resourceDictionary.Add("MetroDataGrid.MouseOverHighlightBrush", new SolidColorBrush((Color)resourceDictionary["AccentColor3"]));
			resourceDictionary.Add("MetroDataGrid.FocusBorderBrush", new SolidColorBrush((Color)resourceDictionary["AccentColor"]));
			resourceDictionary.Add("MetroDataGrid.InactiveSelectionHighlightBrush", new SolidColorBrush((Color)resourceDictionary["AccentColor2"]));
			resourceDictionary.Add("MetroDataGrid.InactiveSelectionHighlightTextBrush", new SolidColorBrush((Color)resourceDictionary["IdealForegroundColor"]));

			var accentName = "Windows Accent";
			var fileName = Path.Combine(Config.Instance.ConfigDir, "WindowsAccent.xaml"); // Combine path to HDT folder + xaml file.

			// Using FileStream to overwrite existing WindowsAccent file with new values.
			using (FileStream s = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
			using (var writer = System.Xml.XmlWriter.Create(s, new System.Xml.XmlWriterSettings { Indent = true }))
			{
				System.Windows.Markup.XamlWriter.Save(resourceDictionary, writer);
				writer.Close();
				s.Close();
			}
			resourceDictionary = new ResourceDictionary() { Source = new Uri(fileName, UriKind.Absolute) }; // Initialize new dictionary with provided URi and previously generated resources. Needed to find WindowsAccent and overwrite source patch.

			ThemeManager.AddAccent(accentName, resourceDictionary.Source); // Add "Windows Accent" to accents list in Options. This happens only once due to bug in MahApps.Metro - If the name is same, but diffrent resources - nothing will happen that is why we need to change them manualy.
			var oldWindowsAccent = ThemeManager.GetAccent("Windows Accent"); // Get "Windows Accent" accent and update its resources.
			oldWindowsAccent.Resources.Source = resourceDictionary.Source; // Only wasted 7 days and 10000 builds to figure it out. xD

			if (changeImmediately)
				ThemeManager.ChangeAppStyle(Application.Current, CurrentAccent, CurrentTheme); // Change application style at HDT start.
		}

		public static void ShowHDTLogo()
		{
			if (CurrentTheme.Name == MetroTheme.BaseLight.ToString())
			{
				Application.Current.Resources["GrayTextColorBrush"] = new SolidColorBrush((Color)Application.Current.Resources["GrayTextColor1"]);
				Application.Current.Resources["HsReplayIcon"] = Application.Current.Resources["HsReplayIconBlue"];
			}
			else
			{
				Application.Current.Resources["GrayTextColorBrush"] = new SolidColorBrush((Color)Application.Current.Resources["GrayTextColor2"]);
				Application.Current.Resources["HsReplayIcon"] = Application.Current.Resources["HsReplayIconWhite"];
			}
		}
	}
}
