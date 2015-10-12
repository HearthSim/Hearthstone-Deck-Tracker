using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker.Utility
{
    public static class Updater
    {
        private static DateTime _lastUpdateCheck;
        private static bool _showingUpdateMessage;
        private static bool TempUpdateCheckDisabled { get; set; }

        public static async void CheckForUpdates(bool force = false)
        {
            if (!force)
            {
                if (!Config.Instance.CheckForUpdates || TempUpdateCheckDisabled || Core.Game.IsRunning
                    || _showingUpdateMessage || (DateTime.Now - _lastUpdateCheck) < new TimeSpan(0, 10, 0))
                    return;
            }
            _lastUpdateCheck = DateTime.Now;
            var newVersion = await Helper.CheckForUpdates(false);
            if (newVersion != null)
                ShowNewUpdateMessage(newVersion, false);
            else if (Config.Instance.CheckForBetaUpdates)
            {
                newVersion = await Helper.CheckForUpdates(true);
                if (newVersion != null)
                    ShowNewUpdateMessage(newVersion, true);
            }
        }

        private static async void ShowNewUpdateMessage(Version newVersion, bool beta)
        {
            if (_showingUpdateMessage)
                return;
            _showingUpdateMessage = true;

            const string releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
            var settings = new MetroDialogSettings
            {
                AffirmativeButtonText = "Download",
                NegativeButtonText = "Not now"
            };
            if (newVersion == null)
            {
                _showingUpdateMessage = false;
                return;
            }
            try
            {
                await Task.Delay(10000);
                Core.MainWindow.ActivateWindow();
                while (Core.MainWindow.Visibility != Visibility.Visible
                       || Core.MainWindow.WindowState == WindowState.Minimized)
                    await Task.Delay(100);
                var newVersionString = string.Format("{0}.{1}.{2}", newVersion.Major, newVersion.Minor, newVersion.Build);
                var betaString = beta ? " BETA" : "";
                var result =
                    await
                        Core.MainWindow.ShowMessageAsync("New" + betaString + " Update available!",
                            "Press \"Download\" to automatically download.",
                            MessageDialogStyle.AffirmativeAndNegative, settings);

                if (result == MessageDialogResult.Affirmative)
                {
                    //recheck, in case there was no immediate response to the dialog
                    if ((DateTime.Now - _lastUpdateCheck) > new TimeSpan(0, 10, 0))
                    {
                        newVersion = await Helper.CheckForUpdates(beta);
                        if (newVersion != null)
                            newVersionString = string.Format("{0}.{1}.{2}", newVersion.Major, newVersion.Minor,
                                newVersion.Build);
                    }
                    try
                    {
                        Process.Start("HDTUpdate.exe",
                            string.Format("{0} {1}", Process.GetCurrentProcess().Id, newVersionString));
                        Core.MainWindow.Close();
                        Application.Current.Shutdown();
                    }
                    catch
                    {
                        Logger.WriteLine("Error starting updater");
                        Process.Start(releaseDownloadUrl);
                    }
                }
                else
                    TempUpdateCheckDisabled = true;

                _showingUpdateMessage = false;
            }
            catch (Exception e)
            {
                _showingUpdateMessage = false;
                Logger.WriteLine("Error showing new update message\n" + e.Message);
            }
        }

        public static void Cleanup()
        {
            try
            {
                if (File.Exists("HDTUpdate_new.exe"))
                {
                    if (File.Exists("HDTUpdate.exe"))
                        File.Delete("HDTUpdate.exe");
                    File.Move("HDTUpdate_new.exe", "HDTUpdate.exe");
                }
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error updating updater\n" + e);
            }
            try
            {
                //updater used pre v0.9.6
                if (File.Exists("Updater.exe"))
                    File.Delete("Updater.exe");
            }
            catch (Exception e)
            {
                Logger.WriteLine("Error deleting Updater.exe\n" + e);
            }
        }
    }
}