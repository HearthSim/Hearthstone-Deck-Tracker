#region

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

#endregion

namespace Hearthstone_Deck_Tracker
{
    internal class DeckExporter
    {
        private readonly Config _config;

        public DeckExporter(Config config)
        {
            _config = config;
        }
        
        public async Task Export(Deck deck)
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

            User32.Rect hsWindowRect = new User32.Rect();
            User32.GetWindowRect(hsHandle, ref hsWindowRect);

            var height = (hsWindowRect.bottom - hsWindowRect.top);
            var width = (hsWindowRect.right - hsWindowRect.left);

            var bounds = Screen.FromHandle(hsHandle).Bounds;
            bool isFullscreen = bounds.Width == width && bounds.Height == height;

            if(_config.ExportSetDeckName)
                await SetDeckName(deck.Name, width, height, hsHandle);

            foreach (var card in deck.Cards)
            {
                await AddCardToDeck(card, width, height, hsHandle, isFullscreen);
            }
        }

        private async Task SetDeckName(string name, int width, int height, IntPtr hsHandle)
        {
            var nameDeckPos = new Point((int)(_config.NameDeckX * width), (int)(_config.NameDeckY * height));
            await ClickOnPoint(hsHandle, nameDeckPos);
            SendKeys.SendWait(name);
            SendKeys.SendWait("{ENTER}");
        }

        private async Task AddCardToDeck(Card card, int width, int height, IntPtr hsHandle, bool isFullscreen)
        {
            var ratio = (double) width/height;

            var searchBoxY = (isFullscreen ? _config.SearchBoxYFullscreen : _config.SearchBoxY);
            var cardPosX = ratio < 1.5 ? width*_config.CardPosX : width*_config.CardPosX*(ratio/1.33);

            var searchBoxPos = new Point((int) (_config.SearchBoxX*width), (int) (searchBoxY*height));
            var cardPos = new Point((int) cardPosX, (int) (_config.CardPosY*height));

            await ClickOnPoint(hsHandle, searchBoxPos);
            SendKeys.SendWait("^(a)");
            SendKeys.SendWait(FixCardName(card.LocalizedName));
            SendKeys.SendWait("{ENTER}");

            await Task.Delay(_config.SearchDelay);

            var card2PosX = ratio < 1.5 ? width*_config.Card2PosX : width*_config.Card2PosX*(ratio/1.33);
            var cardPosY = _config.CardPosY*height;
            for (int i = 0; i < card.Count; i++)
            {
                if (_config.PrioritizeGolden)
                {
                    if (card.Count == 2)
                        await ClickOnPoint(hsHandle, new Point((int) card2PosX, (int) cardPosY));
                    else
                    {
                        if (CheckForGolden(hsHandle, new Point((int)card2PosX, (int)(cardPosY+height*0.05))))
                        {
                            await ClickOnPoint(hsHandle, new Point((int) card2PosX, (int) cardPosY));
                        }
                        else
                        {
                            await ClickOnPoint(hsHandle, new Point((int)cardPosX, (int)cardPosY));
                        }
                    }
                }
                else
                {
                    await ClickOnPoint(hsHandle, cardPos);
                }
                
            }
            if (card.Count == 2)
            {
                //click again to make sure we get 2 cards 
                if (_config.PrioritizeGolden)
                {
                    await ClickOnPoint(hsHandle, new Point((int)cardPosX, (int)cardPosY));
                    await ClickOnPoint(hsHandle, new Point((int)cardPosX, (int)cardPosY));
                }
                else
                {
                    await ClickOnPoint(hsHandle, new Point((int)card2PosX, (int)cardPosY));
                }

            }
        }

        private async Task ClickOnPoint(IntPtr wndHandle, Point clientPoint)
        {
            User32.ClientToScreen(wndHandle, ref clientPoint);

            Cursor.Position = new Point(clientPoint.X, clientPoint.Y);

            //mouse down
            if (SystemInformation.MouseButtonsSwapped)
                User32.mouse_event((uint)User32.MouseEventFlags.RightDown, 0, 0, 0, UIntPtr.Zero);
            else
                User32.mouse_event((uint)User32.MouseEventFlags.LeftDown, 0, 0, 0, UIntPtr.Zero);

            await Task.Delay(_config.ClickDelay);

            //mouse up
            if (SystemInformation.MouseButtonsSwapped)
                User32.mouse_event((uint)User32.MouseEventFlags.RightUp, 0, 0, 0, UIntPtr.Zero);
            else
                User32.mouse_event((uint)User32.MouseEventFlags.LeftUp, 0, 0, 0, UIntPtr.Zero);
            await Task.Delay(_config.ClickDelay);
        }

        private string FixCardName(string cardName)
        {
            switch (cardName)
            {
                //english
                case "Fireball":
                case "Windfury":
                    return cardName + " Spell";
                case "Slam":
                    return cardName + " Draw";

                //german
                case "Feuerball":
                case "Windzorn":
                    return cardName + " Zauber";
                default:
                    return cardName;
            }
        }

        private bool CheckForGolden(IntPtr wndHandle, Point point)
        {
            const int width = 50;
            const int height = 50;

            const int targetHue = 43;
            const float targetSat = 0.38f;
            
            var avgHue = 0.0f;
            var avgSat = 0.0f;
            var capture = CaptureHearthstone(wndHandle, point, width, height);
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

        public Bitmap CaptureHearthstone(IntPtr wndHandle, Point point, int width, int height)
        {
            User32.ClientToScreen(wndHandle, ref point);
            if (!User32.IsForegroundWindow("Hearthstone")) return null;

            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bmp);
            graphics.CopyFromScreen(point.X, point.Y, 0, 0, new Size(50, 50), CopyPixelOperation.SourceCopy);
            return bmp;
        }
    }
}