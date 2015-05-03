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
			if(deck == null)
				return;
			string Current_Clipboard = "";

			try
			{
				if(Config.Instance.ExportPasteClipboard && Clipboard.ContainsText())
					Current_Clipboard = Clipboard.GetText();
				Logger.WriteLine(string.Format("Exporting " + deck.GetDeckInfo()), "DeckExporter");
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

				Logger.WriteLine("Waiting for " + Config.Instance.ExportStartDelay + " seconds before starting the export process", "DeckExporter");
				await Task.Delay(Config.Instance.ExportStartDelay * 1000);

				var hsRect = User32.GetHearthstoneRect(false);
				var ratio = (4.0 / 3.0) / ((double)hsRect.Width / hsRect.Height);

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
				deck.MissingCards.Clear();
				foreach(var card in deck.Cards)
				{
					var missingCardsCount =
						await AddCardToDeck(card, searchBoxPos, cardPosX, card2PosX, cardPosY, hsRect.Height, hsRect.Width, hsHandle);
					if(missingCardsCount < 0)
						return;
					if(missingCardsCount > 0)
					{
						var missingCard = (Card)card.Clone();
						missingCard.Count = missingCardsCount;
						deck.MissingCards.Add(missingCard);
					}
				}

				if(deck.MissingCards.Any())
					DeckList.Save();

				// Clear search field now all cards have been entered

				await ClickOnPoint(hsHandle, searchBoxPos);
				SendKeys.SendWait("{DELETE}");
				SendKeys.SendWait("{ENTER}");

				if(Config.Instance.ExportPasteClipboard)
					Clipboard.Clear();

				Logger.WriteLine("Done creating deck.", "DeckExporter");
			}
			catch(Exception e)
			{
				Logger.WriteLine("Error exporting deck: " + e, "DeckExporter");
			}
			finally
			{
				Helper.MainWindow.Overlay.ForceHidden = false;
				Helper.MainWindow.Overlay.UpdatePosition();
				if(Config.Instance.ExportPasteClipboard && Current_Clipboard != "")
					Clipboard.SetText(Current_Clipboard);
			}
		}

		private static async Task ClickAllCrystal(double ratio, int width, int height, IntPtr hsHandle)
		{
			Logger.WriteLine("Clicking \"all\" crystal...", "DeckExporter");
			await
				ClickOnPoint(hsHandle,
				             new Point((int)GetXPos(Config.Instance.ExportAllButtonX, width, ratio),
				                       (int)(Config.Instance.ExportAllButtonY * height)));
		}

		private static async Task SetDeckName(string name, double ratio, int width, int height, IntPtr hsHandle)
		{
			Logger.WriteLine("Setting deck name...", "DeckExporter");
			var nameDeckPos = new Point((int)GetXPos(Config.Instance.ExportNameDeckX, width, ratio),
			                            (int)(Config.Instance.ExportNameDeckY * height));
			await ClickOnPoint(hsHandle, nameDeckPos);
			//send enter and second click to make sure the current name gets selected
			SendKeys.SendWait("{ENTER}");
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
			return (width * ratio * left) + (width * (1 - ratio) / 2);
		}

		//returns the number of missing cards
		private static async Task<int> AddCardToDeck(Card card, Point searchBoxPos, double cardPosX, double card2PosX, double cardPosY,
		                                             int height, int width, IntPtr hsHandle)
		{
			if(!User32.IsHearthstoneInForeground())
			{
				Helper.MainWindow.ShowMessage("Exporting aborted", "Hearthstone window lost focus.");
				Logger.WriteLine("Exporting aborted, window lost focus", "DeckExporter");
				return -1;
			}

			await ClickOnPoint(hsHandle, searchBoxPos);

			var addArtist = new[] {"zhCN", "zhTW", "ruRU", "koKR"}.All(x => Config.Instance.SelectedLanguage != x);
			var fixedName = addArtist ? (card.LocalizedName + " " + card.Artist).ToLowerInvariant() : card.LocalizedName.ToLowerInvariant();
			if(Config.Instance.ExportPasteClipboard)
			{
				Clipboard.SetText(fixedName);
				SendKeys.SendWait("^v");
			}
			else
				SendKeys.SendWait(fixedName);
			SendKeys.SendWait("{ENTER}");

			Logger.WriteLine("try to export card: " + card.Name, "DeckExporter", 1);
			await Task.Delay(Config.Instance.DeckExportDelay * 2);

			if(await CheckForSpecialCases(card, cardPosX + 50, card2PosX + 50, cardPosY + 50, hsHandle))
				return 0;

			//Check if Card exist in collection
			if(CardExists(hsHandle, (int)cardPosX, (int)cardPosY))
			{
				//move mouse over card if card is new  TODO: currently does nothing
				/*var newCard = new Point((int)cardPosX, (int)cardPosY);
				User32.ClientToScreen(hsHandle, ref newCard);
				for(var i = 0; i < 3; i++)
					Cursor.Position = new Point(newCard.X + i + 50, newCard.Y - i + 50);*/

				//Check if a golden exist
				if(Config.Instance.PrioritizeGolden && CardExists(hsHandle, (int)card2PosX, (int)cardPosY))
				{
					await ClickOnPoint(hsHandle, new Point((int)card2PosX + 50, (int)cardPosY + 50));

					if(card.Count == 2)
					{
						await ClickOnPoint(hsHandle, new Point((int)card2PosX + 50, (int)cardPosY + 50));
						await ClickOnPoint(hsHandle, new Point((int)cardPosX + 50, (int)cardPosY + 50));
					}
				}
				else
				{
					await ClickOnPoint(hsHandle, new Point((int)cardPosX + 50, (int)cardPosY + 50));

					if(card.Count == 2)
					{
						//Check if two card are not available 
						await Task.Delay(100);
						if(CardHasLock(hsHandle, (int)(cardPosX + width * 0.048), (int)(cardPosY + height * 0.287)))
							return 1;

						await ClickOnPoint(hsHandle, new Point((int)cardPosX + 50, (int)cardPosY + 50));
					}
				}
			}
			else
				return card.Count;
			return 0;
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

		private static bool CardExists(IntPtr wndHandle, int posX, int posY)
		{
			const int width = 40;
			const int height = 40;
			const double minHue = 90;

			var capture = Helper.CaptureHearthstone(new Point(posX, posY), width, height, wndHandle);
			if(capture == null)
				return false;

			return GetAverageHueAndBrightness(capture).Hue > minHue;
		}

		private static bool CardHasLock(IntPtr wndHandle, int posX, int posY)
		{
			const int width = 55;
			const int height = 30;
			const double maxBrightness = 5.0 / 11.0;

			var capture = Helper.CaptureHearthstone(new Point(posX, posY), width, height, wndHandle);
			if(capture == null)
				return false;

			return GetAverageHueAndBrightness(capture).Brightness < maxBrightness;
		}

		private static HueAndBrightness GetAverageHueAndBrightness(Bitmap bmp, double saturationThreshold = 0.05)
		{
			var totalHue = 0.0f;
			var totalBrightness = 0.0f;
			var validPixels = 0;
			for(var i = 0; i < bmp.Width; i++)
			{
				for(var j = 0; j < bmp.Height; j++)
				{
					var pixel = bmp.GetPixel(i, j);

					//ignore sparkle
					if(pixel.GetSaturation() > saturationThreshold)
					{
						totalHue += pixel.GetHue();
						totalBrightness += pixel.GetBrightness();
						validPixels++;
					}
				}
			}

			return new HueAndBrightness(totalHue / validPixels, totalBrightness / validPixels);
		}

		private static async Task ClearDeck(int width, int height, IntPtr handle, double ratio)
		{
			var count = 0;
			Logger.WriteLine("Clearing deck...", "DeckExporter");
			while(!CheckForCardsInDeck(handle, width, height, ratio))
			{
				await
					ClickOnPoint(handle,
					             new Point((int)GetXPos(Config.Instance.ExportClearX, width, ratio), (int)(Config.Instance.ExportClearY * height)));
				if(count++ > 35)
					break;
			}
		}

		private static bool CheckForCardsInDeck(IntPtr wndHandle, int width, int height, double ratio)
		{
			var capture =
				Helper.CaptureHearthstone(
				                          new Point((int)GetXPos(Config.Instance.ExportClearX, width, ratio),
				                                    (int)(Config.Instance.ExportClearCheckYFixed * height)), 1, 1, wndHandle);
			return ColorDistance(capture.GetPixel(0, 0), Color.FromArgb(255, 56, 45, 69), 5);
		}

		private static bool ColorDistance(Color color, Color target, double distance)
		{
			return Math.Abs(color.R - target.R) < distance && Math.Abs(color.G - target.G) < distance && Math.Abs(color.B - target.B) < distance;
		}

		private class HueAndBrightness
		{
			public HueAndBrightness(double hue, double brightness)
			{
				Hue = hue;
				Brightness = brightness;
			}

			public double Hue { get; private set; }
			public double Brightness { get; private set; }
		}
	}
}