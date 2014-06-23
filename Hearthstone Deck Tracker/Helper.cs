using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace Hearthstone_Deck_Tracker
{
    public class Helper
    {
        private static XmlManager<SerializableVersion> _xmlManager;

        public static Version CheckForUpdates(out Version newVersionOut)
        {
            newVersionOut = null;

            SerializableVersion version;
            _xmlManager = new XmlManager<SerializableVersion>() { Type = typeof(SerializableVersion) };

            try
            {
                version = _xmlManager.Load("Version.xml");
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

                var newVersion = new Version(_xmlManager.LoadFromString(xml).ToString());

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
            var hsHandle = User32.FindWindow(null, windowName);

            User32.Rect hsWindowRect = new User32.Rect();
            User32.GetWindowRect(hsHandle, ref hsWindowRect);

            var height = (hsWindowRect.bottom - hsWindowRect.top);
            var width = (hsWindowRect.right - hsWindowRect.left);

            var bounds = Screen.FromHandle(hsHandle).Bounds;

            return bounds.Width == width && bounds.Height == height;
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
    }
}
