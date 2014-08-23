#region

using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	internal static class DeckExporter
	{
		public static async Task Export(Deck deck)
		{
			if(deck == null) return;

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

			var searchBoxPos = new Point((int)(GetXPos(Config.Instance.ExportSearchBoxX, hsRect.Width, ratio)), (int)(Config.Instance.ExportSearchBoxY * hsRect.Height));
			var cardPosX = GetXPos(Config.Instance.ExportCard1X, hsRect.Width, ratio);
			var card2PosX = GetXPos(Config.Instance.ExportCard2X, hsRect.Width, ratio);
			var cardPosY = Config.Instance.ExportCardsY * hsRect.Height;

			if(Config.Instance.ExportSetDeckName)
				await SetDeckName(deck.Name, ratio, hsRect.Width, hsRect.Height, hsHandle);

			await ClickAllCrystal(ratio, hsRect.Width, hsRect.Height, hsHandle);

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
		}

		private static async Task ClickAllCrystal(double ratio, int width, int height, IntPtr hsHandle)
		{
			await ClickOnPoint(hsHandle, new Point((int)GetXPos(Config.Instance.ExportAllButtonX, width, ratio), (int)(Config.Instance.ExportAllButtonY * height)));
		}

		private static async Task SetDeckName(string name, double ratio, int width, int height, IntPtr hsHandle)
		{
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
				return;
			}

			await ClickOnPoint(hsHandle, searchBoxPos);

			var fixedName = FixCardName(card.LocalizedName).ToLowerInvariant();
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

		private static string FixCardName(string cardName)
		{
			switch(cardName)
			{
					//multilanguage
				case "Silence":
					switch(Config.Instance.SelectedLanguage)
					{
						case "enUS":
							return cardName + " common";
						case "frFR":
							return cardName + " commune";
						default:
							return cardName;
					}
				case "Blizzard":
					return cardName + " 2";
				case "Feuerball":
				case "Fireball":
					return cardName + " 6";

					//english
				case "Windfury":
				case "Claw":
					return cardName + " Spell";
				case "Slam":
					return cardName + " Draw";

					//german
				case "Windzorn":
				case "Klaue":
					return cardName + " Zauber";

					//french
				case "Éclair":
					return cardName + " 3";

				default:
					return cardName;
			}
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
	}
}