using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Hearthstone;
using MessageBox = System.Windows.MessageBox;

namespace Hearthstone_Deck_Tracker
{
	public class Helper
	{
		//private static XmlManager<SerializableVersion> _xmlManager;

		public static double DpiScalingX = 1.0;

		public static double DpiScalingY = 1.0;

		public static Version CheckForUpdates(out Version newVersionOut)
		{
			newVersionOut = null;

			SerializableVersion version;
			//_xmlManager = new XmlManager<SerializableVersion>() { Type = typeof(SerializableVersion) };

			try
			{
				// version = _xmlManager.Load("Version.xml");
				version = XmlManager<SerializableVersion>.Load("Version.xml");
			}
			catch (Exception e)
			{
				MessageBox.Show(
					e.Message + "\n\n" + e.InnerException + "\n\n If you don't know how to fix this, please verwrite Version.xml with the default file.", "Error loading Version.xml");

				return null;
			}


			var versionXmlUrl =
				@"https://raw.githubusercontent.com/Epix37/Hearthstone-Deck-Tracker/master/Hearthstone%20Deck%20Tracker/Version.xml";

			var currentVersion = new Version(version.ToString());
			try
			{
				var xml = new WebClient().DownloadString(versionXmlUrl);

				var newVersion = new Version(XmlManager<SerializableVersion>.LoadFromString(xml).ToString());

				if (newVersion > currentVersion)
				{
					newVersionOut = newVersion;
				}
			}
			catch (Exception e)
			{
				MessageBox.Show("Error checking for new version.\n\n" + e.Message + "\n\n" + e.InnerException);
			}
			return currentVersion;
		}

		public static bool IsNumeric(char c)
		{
			int output;
			return Int32.TryParse(c.ToString(), out output);
		}

		public static bool IsFullscreen(string windowName)
		{
			var hsHandle = User32.FindWindow("UnityWndClass", windowName);

			var hsRect = User32.GetHearthstoneRect(false);

			var bounds = Screen.FromHandle(hsHandle).Bounds;

			return bounds.Width == hsRect.Width && bounds.Height == hsRect.Height;
		}

		public static bool IsHex(IEnumerable<char> chars)
		{
			bool isHex;
			foreach (var c in chars)
			{
				isHex = ((c >= '0' && c <= '9') ||
						 (c >= 'a' && c <= 'f') ||
						 (c >= 'A' && c <= 'F'));

				if (!isHex)
					return false;
			}
			return true;
		}

		public static double DrawProbability(int copies, int deck, int draw)
		{
			return 1 - (BinomialCoefficient(deck - copies, draw) / BinomialCoefficient(deck, draw));
		}

		public static double BinomialCoefficient(int n, int k)
		{
			double result = 1;
			for (int i = 1; i <= k; i++)
			{
				result *= n - (k - i);
				result /= i;
			}
			return result;
		}

		public static Dictionary<string, string> LanguageDict = new Dictionary<string, string>()
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


		public static string ScreenshotDeck(DeckListView dlv, double dpiX, double dpiY, string name)
		{
			try
			{
				var rtb = new RenderTargetBitmap((int)dlv.ActualWidth, (int)dlv.ActualHeight, dpiX, dpiY, PixelFormats.Pbgra32);
				rtb.Render(dlv);
				var encoder = new PngBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(rtb));

				var path = GetValidFilePath("Screenshots", name, ".png");
				using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
				{
					encoder.Save(stream);
				}
				return path;
			}
			catch (Exception)
			{
				return null;
			}

		}

		public static string GetValidFilePath(string dir, string name, string extension)
		{
			var validDir = RemoveInvalidChars(dir);
			if (!Directory.Exists(validDir))
				Directory.CreateDirectory(validDir);

			if (!extension.StartsWith("."))
				extension = "." + extension;

			var path = validDir + "\\" + RemoveInvalidChars(name);
			if (File.Exists(path + extension))
			{
				int num = 1;
				while (File.Exists(path + "_" + num + extension))
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
			if (collection == null) return;
			var view1 = (CollectionView)CollectionViewSource.GetDefaultView(collection);
			view1.SortDescriptions.Clear();

			if (classFirst)
				view1.SortDescriptions.Add(new SortDescription("IsClassCard", ListSortDirection.Descending));

			view1.SortDescriptions.Add(new SortDescription("Cost", ListSortDirection.Ascending));
			view1.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Descending));
			view1.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
		}

		public static string DeckToIdString(Deck deck)
		{
			return deck.Cards.Aggregate("", (current, card) => current + (card.Id + ":" + card.Count + ";"));
		}
	}
}
