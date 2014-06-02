using System;
using System.Net;
using System.Windows;

namespace Hearthstone_Deck_Tracker
{
    public class Helper
    {
        private static XmlManager<SerializableVersion> _xmlManager; 

        public static void CheckForUpdates()
        {
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
                return;
            }


            var versionXmlUrl =
                @"https://raw.githubusercontent.com/Epix37/Hearthstone-Deck-Tracker/master/Hearthstone%20Deck%20Tracker/Version.xml";

            var xml = new WebClient().DownloadString(versionXmlUrl);
            
            var currentVersion = new Version(version.ToString());
            var newVersion = new Version(_xmlManager.LoadFromString(xml).ToString());

            if (newVersion > currentVersion)
            {
                var releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
                if (
                    MessageBox.Show("New version available at: \n" + releaseDownloadUrl, "New version available!",
                                    MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    System.Diagnostics.Process.Start(releaseDownloadUrl);
                }
            }

        }

        public static bool IsNumeric(char c)
        {
            int output;
            return Int32.TryParse(c.ToString(), out output);
        }
    }
}
