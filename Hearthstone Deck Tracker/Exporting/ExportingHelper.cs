#region

using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Windows;

#endregion

namespace Hearthstone_Deck_Tracker.Exporting
{
	public class ExportingHelper
	{
		public static bool AddArtist
		{
			get { return new[] {"zhCN", "zhTW", "ruRU", "koKR"}.All(x => Config.Instance.SelectedLanguage != x); }
		}

		public static bool CardExists(IntPtr wndHandle, int posX, int posY, int width, int height)
		{
			const double scale = 0.037; // 40px @ height = 1080
			const double minHue = 90;

			int size = (int)Math.Round(height * scale);

			var capture = Helper.CaptureHearthstone(new Point(posX, posY), size, size, wndHandle);
			if(capture == null)
				return false;

			return HueAndBrightness.GetAverage(capture).Hue > minHue;
		}

		public static bool CardHasLock(IntPtr wndHandle, int posX, int posY, int width, int height)
		{
			// setting this as a "width" value relative to height, maybe not best solution?
			const double xScale = 0.051; // 55px @ height = 1080
			const double yScale = 0.0278; // 30px @ height = 1080
			const double maxBrightness = 5.0 / 11.0;

			int lockWidth = (int)Math.Round(height * xScale);
			int lockHeight = (int)Math.Round(height * yScale);

			var capture = Helper.CaptureHearthstone(new Point(posX, posY), lockWidth, lockHeight, wndHandle);
			if(capture == null)
				return false;

			return HueAndBrightness.GetAverage(capture).Brightness < maxBrightness;
		}

		public static bool IsDeckEmpty(IntPtr wndHandle, int width, int height, double ratio)
		{
			var capture =
				Helper.CaptureHearthstone(
				                          new Point((int)Helper.GetScaledXPos(Config.Instance.ExportClearX, width, ratio),
				                                    (int)(Config.Instance.ExportClearCheckYFixed * height)), 1, 1, wndHandle);
			return capture != null && ColorDistance(capture.GetPixel(0, 0), Color.FromArgb(255, 56, 45, 69), 5);
		}

		public static bool IsZeroCrystalSelected(IntPtr wndHandle, double ratio, int width, int height)
		{
			const double scale = 0.020; // 22px @ height = 1080
			const double minBrightness = 0.55;

			int size = (int)Math.Round(height * scale);

			int posX = (int)Helper.GetScaledXPos(Config.Instance.ExportZeroSquareX, width, ratio);
			int posY = (int)(Config.Instance.ExportZeroSquareY * height);

			var capture = Helper.CaptureHearthstone(new Point(posX, posY), size, size, wndHandle);

			if(capture == null)
				return false;

			return HueAndBrightness.GetAverage(capture).Brightness > minBrightness;
		}


		public static string GetSearchString(Card card)
		{
			var searchString = card.LocalizedName.ToLowerInvariant();
			if(AddArtist)
				searchString += " " + card.Artist.ToLowerInvariant();
			searchString += GetSpecialSearchCases(card.Name);
			return searchString;
		}

		public static string GetSpecialSearchCases(string cardName)
		{
			//Charge and Kor'kron Elite have the same artist, while Kor'kron Elite also has the effect "Charge". 
			//"2" seems to be the only consistent distinction across languages.
			if(cardName == "Charge")
				return " 2";
			return string.Empty;
		}

		public static async Task<bool> CheckForSpecialCases(Card card, double cardPosX, double card2PosX, double cardPosY, IntPtr hsHandle)
		{
			if(card.Name == "Feugen")
			{
				if(Config.Instance.OwnsGoldenFeugen && Config.Instance.PrioritizeGolden)
					await MouseActions.ClickOnPoint(hsHandle, new Point((int)card2PosX, (int)cardPosY));
				else
					await MouseActions.ClickOnPoint(hsHandle, new Point((int)cardPosX, (int)cardPosY));
				return true;
			}
			if(card.Name == "Stalagg")
			{
				var posX3 = cardPosX + (card2PosX - cardPosX) * 2;
				var posX4 = cardPosX + (card2PosX - cardPosX) * 3;
				if(Config.Instance.OwnsGoldenFeugen)
				{
					if(Config.Instance.OwnsGoldenStalagg && Config.Instance.PrioritizeGolden)
						await MouseActions.ClickOnPoint(hsHandle, new Point((int)posX4, (int)cardPosY));
					else
						await MouseActions.ClickOnPoint(hsHandle, new Point((int)posX3, (int)cardPosY));
				}
				else
				{
					if(Config.Instance.OwnsGoldenStalagg && Config.Instance.PrioritizeGolden)
						await MouseActions.ClickOnPoint(hsHandle, new Point((int)posX3, (int)cardPosY));
					else
						await MouseActions.ClickOnPoint(hsHandle, new Point((int)card2PosX, (int)cardPosY));
				}
				return true;
			}
			return false;
		}

		public static async Task<bool> EnsureHearthstoneInForeground(IntPtr hsHandle)
		{
			if(!User32.IsHearthstoneInForeground())
			{
				//restore window and bring to foreground
				User32.ShowWindow(hsHandle, User32.SwRestore);
				User32.SetForegroundWindow(hsHandle);
				//wait it to actually be in foreground, else the rect might be wrong
				await Task.Delay(500);
			}
			if(!User32.IsHearthstoneInForeground())
			{
				Core.MainWindow.ShowMessage("Exporting error", "Can't find Hearthstone window.");
				Logger.WriteLine("Can't find Hearthstone window.", "DeckExporter");
				return false;
			}
			return true;
		}

		public static bool ColorDistance(Color color, Color target, double distance)
		{
			return Math.Abs(color.R - target.R) < distance && Math.Abs(color.G - target.G) < distance && Math.Abs(color.B - target.B) < distance;
		}
	}
}