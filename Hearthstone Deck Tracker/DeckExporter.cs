#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker
{
	internal static class DeckExporter
	{
		//private readonly Config _config;

		//public DeckExporter(Config config)
		//{
		//	_config = config;
		//}

		public static async Task Export(Deck deck)
		{
			if (deck == null) return;

			var hsHandle = User32.FindWindow("UnityWndClass", "Hearthstone");

			if (!User32.IsForegroundWindow("Hearthstone"))
			{
				//restore window and bring to foreground
				User32.ShowWindow(hsHandle, User32.SwRestore);
				User32.SetForegroundWindow(hsHandle);
				//wait it to actually be in foreground, else the rect might be wrong
				await Task.Delay(500);
			}
			if (!User32.IsForegroundWindow("Hearthstone"))
			{
				MessageBox.Show("Can't find Heartstone window.");
				return;
			}

			var hsRect = User32.GetHearthstoneRect(false);
			var bounds = Screen.FromHandle(hsHandle).Bounds;

			if (Config.Instance.ExportSetDeckName)
				await SetDeckName(deck.Name, hsRect.Width, hsRect.Height, hsHandle);

			foreach (var card in deck.Cards)
			{
				await AddCardToDeck(card, hsRect.Width, hsRect.Height, hsHandle);
			}
		}

		private static async Task SetDeckName(string name, int width, int height, IntPtr hsHandle)
		{
			var nameDeckPos = new Point((int)(Config.Instance.NameDeckX * width), (int)(Config.Instance.NameDeckY * height));
			await ClickOnPoint(hsHandle, nameDeckPos);
			SendKeys.SendWait(name);
			SendKeys.SendWait("{ENTER}");
		}

		private static async Task AddCardToDeck(Card card, int width, int height, IntPtr hsHandle)
		{
			var ratio = (double)width / height;
			var cardPosX = ratio < 1.5 ? width * Config.Instance.CardPosX : width * Config.Instance.CardPosX * (ratio / 1.33);
			var searchBoxPos = new Point((int)(Config.Instance.SearchBoxX * width), (int)(Config.Instance.SearchBoxPosY * height));
			var cardPos = new Point((int)cardPosX, (int)(Config.Instance.CardPosY * height));

			await ClickOnPoint(hsHandle, searchBoxPos);
			SendKeys.SendWait(FixCardName(card.LocalizedName).ToLowerInvariant());
			SendKeys.SendWait("{ENTER}");

			await Task.Delay(Config.Instance.SearchDelay);

			var card2PosX = ratio < 1.5 ? width * Config.Instance.Card2PosX : width * Config.Instance.Card2PosX * (ratio / 1.33);
			var cardPosY = Config.Instance.CardPosY * height;
			for (int i = 0; i < card.Count; i++)
			{
				if (Config.Instance.PrioritizeGolden)
				{
					if (card.Count == 2)
						await ClickOnPoint(hsHandle, new Point((int)card2PosX, (int)cardPosY));
					else if (CheckForGolden(hsHandle, new Point((int)card2PosX, (int)(cardPosY + height * 0.05))))
						await ClickOnPoint(hsHandle, new Point((int)card2PosX, (int)cardPosY));
					else
						await ClickOnPoint(hsHandle, new Point((int)cardPosX, (int)cardPosY));
				}
				else
				{
					await ClickOnPoint(hsHandle, new Point((int)cardPosX, (int)cardPosY));
				}
			}

			if (card.Count == 2)
			{
				//click again to make sure we get 2 cards 
				if (Config.Instance.PrioritizeGolden)
				{
					await ClickOnPoint(hsHandle, new Point((int)cardPosX, (int)cardPosY));
					await ClickOnPoint(hsHandle, new Point((int)cardPosX, (int)cardPosY));
				}
				else
				{
					await ClickOnPoint(hsHandle, new Point((int)card2PosX, (int)cardPosY));
				}

			}
			
			// Clear search field now all cards have been entered
            await ClickOnPoint(hsHandle, searchBoxPos);
            SendKeys.SendWait("{DELETE}");
            SendKeys.SendWait("{ENTER}");
		}

		private static async Task ClickOnPoint(IntPtr wndHandle, Point clientPoint)
		{
			User32.ClientToScreen(wndHandle, ref clientPoint);

			Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

			//mouse down
			if (SystemInformation.MouseButtonsSwapped)
				User32.mouse_event((uint)User32.MouseEventFlags.RightDown, 0, 0, 0, UIntPtr.Zero);
			else
				User32.mouse_event((uint)User32.MouseEventFlags.LeftDown, 0, 0, 0, UIntPtr.Zero);

			await Task.Delay(Config.Instance.ClickDelay);

			//mouse up
			if (SystemInformation.MouseButtonsSwapped)
				User32.mouse_event((uint)User32.MouseEventFlags.RightUp, 0, 0, 0, UIntPtr.Zero);
			else
				User32.mouse_event((uint)User32.MouseEventFlags.LeftUp, 0, 0, 0, UIntPtr.Zero);

			await Task.Delay(Config.Instance.ClickDelay);
		}

		private static string FixCardName(string cardName)
		{
			switch (cardName)
			{
				//english
				case "Fireball":
				case "Windfury":
				case "Claw":
					return cardName + " Spell";
				case "Slam":
					return cardName + " Draw";
				case "Silence":
					return cardName + " minion.";


				//german
				case "Feuerball":
				case "Windzorn":
				case "Klaue":
					return cardName + " Zauber";
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

			if (capture == null)
				return false;

			var validPixels = 0;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
				{
					var pixel = capture.GetPixel(i, j);

					//ignore sparkle
					if (pixel.GetSaturation() > 0.05)
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