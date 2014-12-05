#region

using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Windows;

#endregion

namespace Hearthstone_Deck_Tracker
{
	internal static class DeckExporter
	{
		public static async Task Export(Deck deck)
		{
			if(deck == null) return;
			try
			{
				Logger.WriteLine(string.Format("Exporting " + deck.GetDeckInfo(), "DeckExporter"));
				var hsHandle = User32.GetHearthstoneWindow();

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
					MessageBox.Show("Can't find Heartstone window.");
					Logger.WriteLine("Can't find Hearthstone window.", "DeckExporter");
					return;
				}

				var hsRect = User32.GetHearthstoneRect(false);
				var ratio = (4.0 / 3.0) / ((double)hsRect.Width / hsRect.Height);

				string oldClipboardContent = null;
				try
				{
					oldClipboardContent = Clipboard.GetText();
				}
				catch
				{
				}

				var searchBoxPos = new Point((int)(GetXPos(Config.Instance.ExportSearchBoxX, hsRect.Width, ratio)),
				                             (int)(Config.Instance.ExportSearchBoxY * hsRect.Height));
				var cardPosX = GetXPos(Config.Instance.ExportCard1X, hsRect.Width, ratio);
				var card2PosX = GetXPos(Config.Instance.ExportCard2X, hsRect.Width, ratio);
				var cardPosY = Config.Instance.ExportCardsY * hsRect.Height;


				Helper.MainWindow.Overlay.ForceHidden = true;
				Helper.MainWindow.Overlay.UpdatePosition();

				if(Config.Instance.AutoClearDeck)
					await ClearDeck(hsRect.Width, hsRect.Height, hsHandle, ratio);

				if(Config.Instance.ExportSetDeckName)
					await SetDeckName(deck.Name, ratio, hsRect.Width, hsRect.Height, hsHandle);

				await ClickAllCrystal(ratio, hsRect.Width, hsRect.Height, hsHandle);

				Logger.WriteLine("Creating deck...", "DeckExporter");
				foreach(var card in deck.Cards)
					await AddCardToDeck(card, searchBoxPos, cardPosX, card2PosX, cardPosY, hsRect.Height, hsHandle);


				// Clear search field now all cards have been entered

				await ClickOnPoint(hsHandle, searchBoxPos);
				SendKeys.SendWait("{DELETE}");
				SendKeys.SendWait("{ENTER}");
				try
				{
					if(oldClipboardContent != null)
						Clipboard.SetText(oldClipboardContent);
				}
				catch
				{

				}
				Logger.WriteLine("Done creating deck.", "DeckExporter");
			}
			catch(Exception e)
			{
				Logger.WriteLine("Error exporting deck: " + e.Message, "DeckExporter");
			}
			finally
			{
				Helper.MainWindow.Overlay.ForceHidden = false;
				Helper.MainWindow.Overlay.UpdatePosition();
			}

		}

		private static async Task ClickAllCrystal(double ratio, int width, int height, IntPtr hsHandle)
		{
			Logger.WriteLine("Clicking \"all\" crystal...", "DeckExporter");
			await ClickOnPoint(hsHandle, new Point((int)GetXPos(Config.Instance.ExportAllButtonX, width, ratio), (int)(Config.Instance.ExportAllButtonY * height)));
		}

		private static async Task SetDeckName(string name, double ratio, int width, int height, IntPtr hsHandle)
		{
			Logger.WriteLine("Setting deck name...", "DeckExporter");
			var nameDeckPos = new Point((int)GetXPos(Config.Instance.ExportNameDeckX, width, ratio), (int)(Config.Instance.ExportNameDeckY * height));
			await ClickOnPoint(hsHandle, nameDeckPos);
			if(Config.Instance.ExportPasteClipboard)
			{
				Clipboard.SetText(name);
				SendKeys.SendWait("^v");
			}
			else
				SendKeys.SendWait(name);
			SendKeys.SendWait("{ENTER}");
		}

		private static double GetXPos(double left, int width, double ratio)
		{
			return (width * ratio * left) + ((width - width * ratio) / 2);
		}

		private static async Task AddCardToDeck(Card card, Point searchBoxPos, double cardPosX, double card2PosX, double cardPosY, int height, IntPtr hsHandle)
		{
			if(!User32.IsHearthstoneInForeground())
			{
				Helper.MainWindow.ShowMessage("Exporting aborted", "Hearthstone window lost focus.");
				Logger.WriteLine("Exporting aborted, window lost focus", "DeckExporter");
				return;
			}

			await ClickOnPoint(hsHandle, searchBoxPos);

			var addArtist = new[] {"zhCN", "zhTW", "ruRU", "koKR"}.All(x => Config.Instance.SelectedLanguage != x);
			var fixedName = addArtist
				                ? (card.LocalizedName + " " + card.Artist).ToLowerInvariant()
				                : card.LocalizedName.ToLowerInvariant();
			if(Config.Instance.ExportPasteClipboard)
			{
				Clipboard.SetText(fixedName);
				SendKeys.SendWait("^v");
			}
			else
				SendKeys.SendWait(fixedName);
			SendKeys.SendWait("{ENTER}");

			await Task.Delay(Config.Instance.DeckExportDelay * 2);

			if(await CheckForSpecialCases(card, cardPosX, card2PosX, cardPosY, hsHandle))
				return;

			var golden = CheckForGolden(hsHandle, new Point((int)card2PosX, (int)(cardPosY + height * 0.05)));
			for(var i = 0; i < card.Count; i++)
			{
				if(Config.Instance.PrioritizeGolden && golden)
					await ClickOnPoint(hsHandle, new Point((int)card2PosX, (int)cardPosY));
				else
					await ClickOnPoint(hsHandle, new Point((int)cardPosX, (int)cardPosY));
			}

			if(card.Count == 2 && golden)
			{
				//click again to make sure we get 2 cards 
				if(Config.Instance.PrioritizeGolden)
					await ClickOnPoint(hsHandle, new Point((int)cardPosX, (int)cardPosY));
				else
					await ClickOnPoint(hsHandle, new Point((int)card2PosX, (int)cardPosY));
			}
		}

		private static async Task<bool> CheckForSpecialCases(Card card, double cardPosX, double card2PosX, double cardPosY, IntPtr hsHandle)
		{
			if(card.Name == "Feugen")
			{
				if(Config.Instance.OwnsGoldenFeugen && Config.Instance.PrioritizeGolden)
					await ClickOnPoint(hsHandle, new Point((int)card2PosX, (int)cardPosY));
				else
					await ClickOnPoint(hsHandle, new Point((int)cardPosX, (int)cardPosY));
				return true;
			}
			if(card.Name == "Stalagg")
			{
				var posX3 = cardPosX + (card2PosX - cardPosX) * 2;
				var posX4 = cardPosX + (card2PosX - cardPosX) * 3;
				if(Config.Instance.OwnsGoldenFeugen)
				{
					if(Config.Instance.OwnsGoldenStalagg && Config.Instance.PrioritizeGolden)
						await ClickOnPoint(hsHandle, new Point((int)posX4, (int)cardPosY));
					else
						await ClickOnPoint(hsHandle, new Point((int)posX3, (int)cardPosY));
				}
				else
				{
					if(Config.Instance.OwnsGoldenStalagg && Config.Instance.PrioritizeGolden)
						await ClickOnPoint(hsHandle, new Point((int)posX3, (int)cardPosY));
					else
						await ClickOnPoint(hsHandle, new Point((int)card2PosX, (int)cardPosY));
				}
				return true;
			}
			return false;
		}

		private static async Task ClickOnPoint(IntPtr wndHandle, Point clientPoint)
		{
			User32.ClientToScreen(wndHandle, ref clientPoint);

			Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

			//mouse down
			if(SystemInformation.MouseButtonsSwapped)
				User32.mouse_event((uint)User32.MouseEventFlags.RightDown, 0, 0, 0, UIntPtr.Zero);
			else
				User32.mouse_event((uint)User32.MouseEventFlags.LeftDown, 0, 0, 0, UIntPtr.Zero);

			await Task.Delay(Config.Instance.DeckExportDelay);

			//mouse up
			if(SystemInformation.MouseButtonsSwapped)
				User32.mouse_event((uint)User32.MouseEventFlags.RightUp, 0, 0, 0, UIntPtr.Zero);
			else
				User32.mouse_event((uint)User32.MouseEventFlags.LeftUp, 0, 0, 0, UIntPtr.Zero);

			await Task.Delay(Config.Instance.DeckExportDelay);
		}

		private static bool CheckForGolden(IntPtr wndHandle, Point point)
		{
			const int width = 50, height = 50, targetHue = 43;
			const float targetSat = 0.38f;
			var avgHue = 0.0f;
			var avgSat = 0.0f;
			var capture = Helper.CaptureHearthstone(point, width, height, wndHandle);

			if(capture == null)
				return false;

			var validPixels = 0;
			for(var i = 0; i < width; i++)
			{
				for(var j = 0; j < height; j++)
				{
					var pixel = capture.GetPixel(i, j);

					//ignore sparkle
					if(pixel.GetSaturation() > 0.05)
					{
						avgHue += pixel.GetHue();
						avgSat += pixel.GetSaturation();
						validPixels++;
					}
				}
			}
			avgHue /= validPixels;
			avgSat /= validPixels;

			return avgHue <= targetHue && avgSat <= targetSat;
		}

		private static async Task ClearDeck(int width, int height, IntPtr handle, double ratio)
		{
			var count = 0;
			Logger.WriteLine("Clearing deck...", "DeckExporter");
			while(!CheckForCardsInDeck(handle, width, height, ratio))
			{
				await
					ClickOnPoint(handle,
					             new Point((int)GetXPos(Config.Instance.ExportClearX, width, ratio),
					                       (int)(Config.Instance.ExportClearY * height)));
				if(count++ > 35)
					break;
			}
		}

		private static bool CheckForCardsInDeck(IntPtr wndHandle, int width, int height, double ratio)
		{

			var capture = Helper.CaptureHearthstone(new Point((int)GetXPos(Config.Instance.ExportClearX, width, ratio), (int)(Config.Instance.ExportClearCheckY*height)), 1,
			                                        1, wndHandle);
			return ColorDistance(capture.GetPixel(0, 0), Color.FromArgb(255,56,45,69), 5);
		}

		private static bool ColorDistance(Color color, Color target, double distance)
		{
			return Math.Abs(color.R - target.R) < distance && Math.Abs(color.G - target.G) < distance &&
			       Math.Abs(color.B - target.B) < distance;
		}
	}
}