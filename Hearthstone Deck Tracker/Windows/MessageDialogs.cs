#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

#endregion

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
				window.ShowMessageAsync("Delete Games", "This will delete the selected games (" + count + ").\n\nAre you sure?",
				                        MessageDialogStyle.AffirmativeAndNegative, settings);
		}

		public static async Task ShowUpdateNotesMessage(this MetroWindow window)
		{
			const string releaseDownloadUrl = @"https://github.com/Epix37/Hearthstone-Deck-Tracker/releases";
			var settings = new MetroDialogSettings {AffirmativeButtonText = "Show update notes", NegativeButtonText = "Close"};

			var result = await window.ShowMessageAsync("Update successful", "", MessageDialogStyle.AffirmativeAndNegative, settings);
			if(result == MessageDialogResult.Affirmative)
				Process.Start(releaseDownloadUrl);
		}

		public static async Task ShowMessage(this MainWindow window, string title, string message)
		{
			await window.ShowMessageAsync(title, message);
		}

		public static async Task ShowSavedFileMessage(this MainWindow window, string fileName)
		{
			var settings = new MetroDialogSettings {NegativeButtonText = "Open folder"};
			var result =
				await window.ShowMessageAsync("", "Saved to\n\"" + fileName + "\"", MessageDialogStyle.AffirmativeAndNegative, settings);
			if(result == MessageDialogResult.Negative)
				Process.Start(Path.GetDirectoryName(fileName));
		}

		public static async Task ShowSavedAndUploadedFileMessage(this MainWindow window, string fileName, string url)
		{
			var settings = new MetroDialogSettings {NegativeButtonText = "open in browser", FirstAuxiliaryButtonText = "copy url to clipboard"};
			var sb = new StringBuilder();
			if(fileName != null)
				sb.AppendLine("Saved to\n\"" + fileName + "\"");
			sb.AppendLine("Uploaded to\n" + url);
			var result = await window.ShowMessageAsync("", sb.ToString(), MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, settings);
			if(result == MessageDialogResult.Negative)
			{
				try
				{
					Process.Start(url);
				}
				catch(Exception ex)
				{
					Logger.WriteLine("Error starting browser: " + ex, "ScreenshotMessageDialog");
				}
			}
			else if(result == MessageDialogResult.FirstAuxiliary)
			{
				try
				{
					Clipboard.SetText(url);
				}
				catch(Exception ex)
				{
					Logger.WriteLine("Error copying url to clipboard: " + ex, "ScreenshotMessageDialog");
				}
			}
		}

		public static async Task<SaveScreenshotOperation> ShowScreenshotUploadSelectionDialog(this MainWindow window)
		{
			var settings = new MetroDialogSettings
			{
				AffirmativeButtonText = "save only",
				NegativeButtonText = "save & upload",
				FirstAuxiliaryButtonText = "upload only"
			};
			var result =
				await
				window.ShowMessageAsync("Select Operation", "\"upload\" will automatically upload the image to imgur.com",
				                        MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, settings);
			return new SaveScreenshotOperation
			{
				SaveLocal = result != MessageDialogResult.FirstAuxiliary,
				Upload = result != MessageDialogResult.Affirmative
			};
		}

		public static async Task ShowHsNotInstalledMessage(this MetroWindow window)
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
					Core.MainWindow.ShowMessage("Restart required.", "Please restart HDT for this to take effect.");
				}
			}
		}

		public static async void ShowMissingCardsMessage(this MetroWindow window, Deck deck)
		{
			if(!deck.MissingCards.Any())
			{
				await
					window.ShowMessageAsync("No missing cards",
					                        "No cards were missing when you last exported this deck. (or you have not recently exported this deck)",
					                        MessageDialogStyle.Affirmative, new MetroDialogSettings {AffirmativeButtonText = "OK"});
				return;
			}
			var message = "The following cards were not found:\n";
			var totalDust = 0;
			var promo = "";
			var nax = "";
			foreach(var card in deck.MissingCards)
			{
				message += "\n• " + card.LocalizedName;

				int dust;
				switch(card.Rarity)
				{
					case "Common":
						dust = 40;
						break;
					case "Rare":
						dust = 100;
						break;
					case "Epic":
						dust = 400;
						break;
					case "Legendary":
						dust = 1600;
						break;
					default:
						dust = 0;
						break;
				}

				if(card.Count == 2)
					message += " x2";

				if(card.Set.Equals("CURSE OF NAXXRAMAS", StringComparison.CurrentCultureIgnoreCase))
					nax = "and the Naxxramas DLC ";
				else if(card.Set.Equals("PROMOTION", StringComparison.CurrentCultureIgnoreCase))
					promo = "and Promotion cards ";
				else
					totalDust += dust * card.Count;
			}
			message += string.Format("\n\nYou need {0} dust {1}{2}to craft the missing cards.", totalDust, nax, promo);
			await
				window.ShowMessageAsync("Export incomplete", message, MessageDialogStyle.Affirmative,
				                        new MetroDialogSettings {AffirmativeButtonText = "OK"});
		}
	}

	public class SaveScreenshotOperation
	{
		public bool SaveLocal { get; set; }
		public bool Upload { get; set; }
	}
}