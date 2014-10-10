﻿using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace Hearthstone_Deck_Tracker.Windows
{
    public static class MessageDialogs
    {
        public static async Task<MessageDialogResult> ShowDeleteGameStatsMessage(this MetroWindow window, GameStats stats)
        {
            var settings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};
            return
                await
                window.ShowMessageAsync("Delete Game",
                                        stats.Result + " vs " + stats.OpponentHero + "\nfrom " + stats.StartTime + "\n\nAre you sure?",
                                        MessageDialogStyle.AffirmativeAndNegative, settings);
        }

        public static async Task<MessageDialogResult> ShowDeleteMultipleGameStatsMessage(this MetroWindow window, int count)
        {
            var settings = new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"};
            return
                await
                window.ShowMessageAsync("Delete Games",
                                        "This will delete the selected games (" + count + ").\n\nAre you sure?",
                                        MessageDialogStyle.AffirmativeAndNegative, settings);
        }

        public static async void ShowUpdateNotesMessage(this MetroWindow window)
        {
            const string releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
            var settings = new MetroDialogSettings {AffirmativeButtonText = "Show update notes", NegativeButtonText = "Close"};

            var result = await window.ShowMessageAsync("Update successful", "", MessageDialogStyle.AffirmativeAndNegative, settings);
            if(result == MessageDialogResult.Affirmative)
                Process.Start(releaseDownloadUrl);
        }

        public static async void ShowMessage(this MainWindow window, string title, string message)
        {
            await window.ShowMessageAsync(title, message);
        }

        public static async void ShowHsNotInstalledMessage(this MetroWindow window)
        {
            var settings = new MetroDialogSettings {AffirmativeButtonText = "Ok", NegativeButtonText = "Select manually"};
            var result =
                await
                window.ShowMessageAsync("Hearthstone install directory not found",
                                        "Hearthstone Deck Tracker will not work properly if Hearthstone is not installed on your machine (obviously).",
                                        MessageDialogStyle.AffirmativeAndNegative, settings);
            if(result == MessageDialogResult.Negative)
            {
                var dialog = new OpenFileDialog
                    {
                        Title = "Select Hearthstone.exe",
                        DefaultExt = "Hearthstone.exe",
                        Filter = "Hearthstone.exe|Hearthstone.exe"
                    };
                var dialogResult = dialog.ShowDialog();

                if(dialogResult == true)
                {
                    Config.Instance.HearthstoneDirectory = Path.GetDirectoryName(dialog.FileName);
                    Config.Save();
                    await Helper.MainWindow.Restart();
                }
            }
        }
    }
}