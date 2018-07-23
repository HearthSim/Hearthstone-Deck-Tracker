using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro;
using Point = System.Windows.Point;
using Application = System.Windows.Application;

namespace Hearthstone_Deck_Tracker
{
	// ReSharper disable once InconsistentNaming
	public static class UITheme
	{
		private const string WindowAccentName = "Windows Accent";
		private const string DefaultAccentName = "Blue";
		private static Color _currentWindowsAccent = SystemParameters.WindowGlassColor;

		public static AppTheme CurrentTheme => ThemeManager.AppThemes.FirstOrDefault(t => t.Name == Config.Instance.AppTheme.ToString()) ?? ThemeManager.DetectAppStyle().Item1;
		public static Accent CurrentAccent => ThemeManager.Accents.FirstOrDefault(a => a.Name == Config.Instance.AccentName) ?? ThemeManager.GetAccent(DefaultAccentName);

		public static async Task InitializeTheme()
		{
			UpdateIconColors();
			if(Helper.IsWindows8() || Helper.IsWindows10())
				await CreateWindowsAccentStyle();
			else if(Config.Instance.AccentName == WindowAccentName)
			{
				// In case if somehow user will get "Windows Accent" on Windows which not support this.
				// (For example move whole HDT on diffrent machine instead of fresh install)
				Config.Instance.AccentName = DefaultAccentName;
				Config.Save();
			}
			ThemeManager.ChangeAppStyle(Application.Current, CurrentAccent, CurrentTheme);
		}

		public static async Task UpdateTheme()
		{
			if(Config.Instance.AccentName == WindowAccentName)
				await CreateWindowsAccentStyle();

			ThemeManager.ChangeAppStyle(Application.Current, CurrentAccent, CurrentTheme);
			UpdateIconColors();
		}

		public static async Task UpdateAccent()
		{
			if(Config.Instance.AccentName == WindowAccentName)
				await CreateWindowsAccentStyle();

			ThemeManager.ChangeAppStyle(Application.Current, CurrentAccent, CurrentTheme);
			Core.MainWindow.DeckPickerList.UpdateDeckModeToggleButton();
		}

		private static async Task CreateWindowsAccentStyle(bool changeImmediately = false)
		{
			var resourceDictionary = new ResourceDictionary();

			var color = SystemParameters.WindowGlassColor;

			resourceDictionary.Add("HighlightColor", color);
			resourceDictionary.Add("AccentColor", Color.FromArgb(204, color.R, color.G, color.B));
			resourceDictionary.Add("AccentColor2", Color.FromArgb(153, color.R, color.G, color.B));
			resourceDictionary.Add("AccentColor3", Color.FromArgb(102, color.R, color.G, color.B));
			resourceDictionary.Add("AccentColor4", Color.FromArgb(51, color.R, color.G, color.B));
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

			resourceDictionary.Add("IdealForegroundColorBrush", new SolidColorBrush((Color)resourceDictionary["IdealForegroundColor"]));
			resourceDictionary.Add("AccentSelectedColorBrush", new SolidColorBrush((Color)resourceDictionary["IdealForegroundColor"]));
			resourceDictionary.Add("MetroDataGrid.HighlightBrush", new SolidColorBrush((Color)resourceDictionary["AccentColor"]));
			resourceDictionary.Add("MetroDataGrid.HighlightTextBrush", new SolidColorBrush((Color)resourceDictionary["IdealForegroundColor"]));
			resourceDictionary.Add("MetroDataGrid.MouseOverHighlightBrush", new SolidColorBrush((Color)resourceDictionary["AccentColor3"]));
			resourceDictionary.Add("MetroDataGrid.FocusBorderBrush", new SolidColorBrush((Color)resourceDictionary["AccentColor"]));
			resourceDictionary.Add("MetroDataGrid.InactiveSelectionHighlightBrush", new SolidColorBrush((Color)resourceDictionary["AccentColor2"]));
			resourceDictionary.Add("MetroDataGrid.InactiveSelectionHighlightTextBrush", new SolidColorBrush((Color)resourceDictionary["IdealForegroundColor"]));

			var fileName = Path.Combine(Config.Instance.ConfigDir, "WindowsAccent.xaml");

			try
			{
				using(var stream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
				using(var writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true }))
					XamlWriter.Save(resourceDictionary, writer);
			}
			catch(Exception e)
			{
				Log.Error("Error creating WindowsAccent: " + e);
				return;
			}

			resourceDictionary = new ResourceDictionary { Source = new Uri(Path.GetFullPath(fileName), UriKind.Absolute) };

			try
			{
				ThemeManager.AddAccent(WindowAccentName, resourceDictionary.Source);
			}
			catch (IOException e)
			{
				await Task.Delay(500);
				try
				{
					ThemeManager.AddAccent(WindowAccentName, resourceDictionary.Source);
				}
				catch (Exception e2)
				{
					Log.Error("Error adding WindowsAccent: " + e2);
					return;
				}
			}
			catch(Exception e)
			{
				Log.Error("Error adding WindowsAccent: " + e);
				return;
			}

			var oldWindowsAccent = ThemeManager.GetAccent(WindowAccentName);
			oldWindowsAccent.Resources.Source = resourceDictionary.Source;

			if(changeImmediately)
				ThemeManager.ChangeAppStyle(Application.Current, CurrentAccent, CurrentTheme);
		}

		public static void UpdateIconColors()
		{
			if(CurrentTheme.Name == MetroTheme.BaseLight.ToString())
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

		public static async Task RefreshWindowsAccent()
		{
			if(Config.Instance.AccentName == WindowAccentName && _currentWindowsAccent != SystemParameters.WindowGlassColor)
			{
				Config.Instance.AccentName = DefaultAccentName;
				await UpdateAccent();
				Config.Instance.AccentName = WindowAccentName;
				await UpdateAccent();
				_currentWindowsAccent = SystemParameters.WindowGlassColor;
			}
		}
	}
}
