using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Hearthstone;
using Color = System.Drawing.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace Hearthstone_Deck_Tracker
{
	public static class Helper
	{
		public static double DpiScalingX = 1.0, DpiScalingY = 1.0;

		public static readonly Dictionary<string, string> LanguageDict = new Dictionary<string, string>
			{
				{"English", "enUS"},
				{"Chinese (China)", "zhCN"},
				{"Chinese (Taiwan)", "zhTW"},
				{"French", "frFR"},
				{"German", "deDE"},
				{"Italian", "itIT"},
				{"Korean", "koKR"},
				{"Polish", "plPL"},
				{"Portuguese", "ptBR"},
				{"Russian", "ruRU"},
				{"Spanish (Mexico)", "esMX"},
				{"Spanish (Spain)", "esES"}
			};

		public static MainWindow MainWindow { get; set; }


		public static Version CheckForUpdates(out Version newVersionOut)
		{
			Logger.WriteLine("Checking for updates...");
			newVersionOut = null;

			const string versionXmlUrl = @"https://raw.githubusercontent.com/Epix37/Hearthstone-Deck-Tracker/master/Hearthstone%20Deck%20Tracker/Version.xml";

			var currentVersion = GetCurrentVersion();

			if(currentVersion != null)
			{
				try
				{
					var xml = new WebClient().DownloadString(versionXmlUrl);

					var newVersion = new Version(XmlManager<SerializableVersion>.LoadFromString(xml).ToString());

					if(newVersion > currentVersion)
						newVersionOut = newVersion;
				}
				catch(Exception e)
				{
					MessageBox.Show("Error checking for new version.\n\n" + e.Message + "\n\n" + e.InnerException);
				}
			}

			return currentVersion;
		}

		// A bug in the SerializableVersion.ToString() method causes this to load Version.xml incorrectly.
		// The build and revision numbers are swapped (i.e. a Revision of 21 in Version.xml loads to Version.Build == 21).
		public static Version GetCurrentVersion()
		{
			try
			{
				return new Version(XmlManager<SerializableVersion>.Load("Version.xml").ToString());
			}
			catch(Exception e)
			{
				MessageBox.Show(
					e.Message + "\n\n" + e.InnerException +
					"\n\n If you don't know how to fix this, please overwrite Version.xml with the default file.",
					"Error loading Version.xml");

				return null;
			}
		}

		public static bool IsNumeric(char c)
		{
			int output;
			return Int32.TryParse(c.ToString(), out output);
		}

		public static bool IsHex(IEnumerable<char> chars)
		{
			return chars.All(c => ((c >= '0' && c <= '9')
			                       || (c >= 'a' && c <= 'f')
			                       || (c >= 'A' && c <= 'F')));
		}

		public static double DrawProbability(int copies, int deck, int draw)
		{
			return 1 - (BinomialCoefficient(deck - copies, draw) / BinomialCoefficient(deck, draw));
		}

		public static double BinomialCoefficient(int n, int k)
		{
			double result = 1;
			for(var i = 1; i <= k; i++)
			{
				result *= n - (k - i);
				result /= i;
			}
			return result;
		}

		public static string ScreenshotDeck(DeckListView dlv, double dpiX, double dpiY, string name)
		{
			try
			{
				var rtb = new RenderTargetBitmap((int)dlv.ActualWidth, (int)dlv.ActualHeight, dpiX, dpiY, PixelFormats.Pbgra32);
				rtb.Render(dlv);
				var encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(rtb));

				var path = GetValidFilePath("Screenshots", name, ".png");
				using(var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
					encoder.Save(stream);
				return path;
			}
			catch(Exception)
			{
				return null;
			}
		}

		public static string GetValidFilePath(string dir, string name, string extension)
		{
			var validDir = RemoveInvalidChars(dir);
			if(!Directory.Exists(validDir))
				Directory.CreateDirectory(validDir);

			if(!extension.StartsWith("."))
				extension = "." + extension;

			var path = validDir + "\\" + RemoveInvalidChars(name);
			if(File.Exists(path + extension))
			{
				var num = 1;
				while(File.Exists(path + "_" + num + extension))
					num++;
				path += "_" + num;
			}

			return path + extension;
		}

		public static string RemoveInvalidChars(string s)
		{
			var invalidChars = new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars());
			var regex = new Regex(string.Format("[{0}]", Regex.Escape(invalidChars)));
			return regex.Replace(s, "");
		}

		public static void SortCardCollection(IEnumerable collection, bool classFirst)
		{
			if(collection == null) return;
			var view1 = (CollectionView)CollectionViewSource.GetDefaultView(collection);
			view1.SortDescriptions.Clear();

			if(classFirst)
				view1.SortDescriptions.Add(new SortDescription("IsClassCard", ListSortDirection.Descending));

			view1.SortDescriptions.Add(new SortDescription("Cost", ListSortDirection.Ascending));
			view1.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Descending));
			view1.SortDescriptions.Add(new SortDescription("LocalizedName", ListSortDirection.Ascending));
		}

		public static string DeckToIdString(Deck deck)
		{
			return deck.Cards.Aggregate("", (current, card) => current + (card.Id + ":" + card.Count + ";"));
		}

		public static Bitmap CaptureHearthstone(Point point, int width, int height, IntPtr wndHandle = default(IntPtr))
		{
			if(wndHandle == default(IntPtr))
				wndHandle = User32.GetHearthstoneWindow();

			User32.ClientToScreen(wndHandle, ref point);
			if(!User32.IsHearthstoneInForeground()) return null;

			var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			var graphics = Graphics.FromImage(bmp);
			graphics.CopyFromScreen(point.X, point.Y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);
			return bmp;
		}

		public static async Task<bool> FriendsListOpen()
		{
			//wait for friendslist to open/close
			await Task.Delay(300);

			var rect = User32.GetHearthstoneRect(false);
			var capture = CaptureHearthstone(new Point(0, (int)(rect.Height * 0.85)), (int)(rect.Width * 0.1),
			                                 (int)(rect.Height * 0.15));
			if(capture == null) return false;

			for(var y = 0; y < capture.Height; y++)
			{
				for(var x = 0; x < capture.Width; x++)
				{
					if(IsYellowPixel(capture.GetPixel(x, y)))
					{
						var foundFriendsList = true;

						//check for a straight yellow line (left side of add button)
						for(var i = 0; i < 5; i++)
						{
							if(x + i >= capture.Width || !IsYellowPixel(capture.GetPixel(x + i, y)))
								foundFriendsList = false;
						}

						if(foundFriendsList)
						{
							Logger.WriteLine("Found Friendslist");
							return true;
						}
					}
				}
			}

			return false;
		}

		private static bool IsYellowPixel(Color pixel)
		{
			const int red = 216;
			const int green = 174;
			const int blue = 10;
			const int deviation = 10;
			return Math.Abs(pixel.R - red) <= deviation && Math.Abs(pixel.G - green) <= deviation &&
			       Math.Abs(pixel.B - blue) <= deviation;
		}

		public static void UpdateEverything()
		{
			//todo: move this somewhere else
			//reader done analyzing new stuff, update things
			if(MainWindow.Overlay.IsVisible)
				MainWindow.Overlay.Update(false);

			if(MainWindow.PlayerWindow.IsVisible)
				MainWindow.PlayerWindow.SetCardCount(Game.PlayerHandCount, 30 - Game.PlayerDrawn.Sum(card => card.Count));

			if(MainWindow.OpponentWindow.IsVisible)
				MainWindow.OpponentWindow.SetOpponentCardCount(Game.OpponentHandCount, Game.OpponentDeckCount, Game.OpponentHasCoin);


			if(MainWindow.NeedToIncorrectDeckMessage && !MainWindow.IsShowingIncorrectDeckMessage)
			{
				MainWindow.IsShowingIncorrectDeckMessage = true;
				MainWindow.ShowIncorrectDeckMessage();
			}
		}

		//http://stackoverflow.com/questions/23927702/move-a-folder-from-one-drive-to-another-in-c-sharp
		public static void CopyFolder(string sourceFolder, string destFolder)
		{
			if(!Directory.Exists(destFolder))
				Directory.CreateDirectory(destFolder);
			var files = Directory.GetFiles(sourceFolder);
			foreach(var file in files)
			{
				var name = Path.GetFileName(file);
				var dest = Path.Combine(destFolder, name);
				File.Copy(file, dest);
			}
			var folders = Directory.GetDirectories(sourceFolder);
			foreach(var folder in folders)
			{
				var name = Path.GetFileName(folder);
				var dest = Path.Combine(destFolder, name);
				CopyFolder(folder, dest);
			}
		}


		//http://stackoverflow.com/questions/3769457/how-can-i-remove-accents-on-a-string
		public static string RemoveDiacritics(string src, bool compatNorm)
		{
			var sb = new StringBuilder();
			foreach(var c in src.Normalize(compatNorm ? NormalizationForm.FormKD : NormalizationForm.FormD))
			{
				switch(CharUnicodeInfo.GetUnicodeCategory(c))
				{
					case UnicodeCategory.NonSpacingMark:
					case UnicodeCategory.SpacingCombiningMark:
					case UnicodeCategory.EnclosingMark:
						break;
					default:
						sb.Append(c);
						break;
				}
			}

			return sb.ToString();
		}
	}
}