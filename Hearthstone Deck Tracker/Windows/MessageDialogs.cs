#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.FlyoutControls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using static System.StringComparison;
using static MahApps.Metro.Controls.Dialogs.MessageDialogStyle;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public static class MessageDialogs
	{
		public static async Task<MessageDialogResult> ShowDeleteGameStatsMessage(this MetroWindow window, GameStats stats)
			=> await window.ShowMessageAsync("Delete Game", $"{stats.Result} vs {stats.OpponentHero}\nfrom {stats.StartTime}\n\nAre you sure?",
				AffirmativeAndNegative, new Settings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"});

		public static async Task<MessageDialogResult> ShowDeleteMultipleGameStatsMessage(this MetroWindow window, int count)
			=> await window.ShowMessageAsync("Delete Games", $"This will delete the selected games ({count}).\n\nAre you sure?",
				AffirmativeAndNegative, new Settings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"});

		public static async Task ShowUpdateNotesMessage(this MetroWindow window)
		{
			var result = await window.ShowMessageAsync("Update successful", "", AffirmativeAndNegative,
							new Settings {AffirmativeButtonText = "Show update notes", NegativeButtonText = "Close"});
			if(result == MessageDialogResult.Affirmative)
				Helper.TryOpenUrl(@"https://github.com/HearthSim/Hearthstone-Deck-Tracker/releases");
		}

		public static async void ShowRestartDialog()
		{
			var result =
				await Core.MainWindow.ShowMessageAsync("Restart required.", "HDT needs to be restarted for the changes to take effect.",
					MessageDialogStyle.AffirmativeAndNegative,
					new MessageDialogs.Settings() { AffirmativeButtonText = "Restart Now", NegativeButtonText = "Later" });
			if(result == MessageDialogResult.Affirmative)
				Core.MainWindow.Restart();
		}

		public static async Task ShowMessage(this MetroWindow window, string title, string message) => await window.ShowMessageAsync(title, message);

		public static async Task ShowSavedFileMessage(this MainWindow window, string fileName)
		{
			var result = await window.ShowMessageAsync("", $"Saved to\n\"{fileName}\"", AffirmativeAndNegative,
							new Settings {NegativeButtonText = "Open folder"});
			if(result == MessageDialogResult.Negative)
				Process.Start(Path.GetDirectoryName(fileName));
		}

		public static async Task ShowSavedAndUploadedFileMessage(this MainWindow window, string fileName, string url)
		{
			var sb = new StringBuilder();
			if(fileName != null)
				sb.AppendLine($"Saved to\n\"{fileName}\"");
			sb.AppendLine($"Uploaded to\n{url}");
			var result = await window.ShowMessageAsync("", sb.ToString(), AffirmativeAndNegativeAndSingleAuxiliary,
							new Settings {NegativeButtonText = "open in browser", FirstAuxiliaryButtonText = "copy url to clipboard"});
			if(result == MessageDialogResult.Negative)
				Helper.TryOpenUrl(url);
			else if(result == MessageDialogResult.FirstAuxiliary)
			{
				try
				{
					Clipboard.SetText(url);
				}
				catch(Exception ex)
				{
					Log.Error("Error copying url to clipboard: " + ex);
				}
			}
		}

		public static async Task<SaveScreenshotOperation> ShowScreenshotUploadSelectionDialog(this MainWindow window)
		{
			var result = await window.ShowMessageAsync("Select Operation", "\"upload\" will automatically upload the image to imgur.com",
							AffirmativeAndNegativeAndDoubleAuxiliary, new Settings
							{
								AffirmativeButtonText = "save",
								NegativeButtonText = "save & upload",
								FirstAuxiliaryButtonText = "upload",
								SecondAuxiliaryButtonText = "cancel"
							});
			return new SaveScreenshotOperation
			{
				Cancelled =  result == MessageDialogResult.SecondAuxiliary,
				SaveLocal = result != MessageDialogResult.FirstAuxiliary,
				Upload = result != MessageDialogResult.Affirmative
			};
		}

		public static async Task ShowLogConfigUpdateFailedMessage(this MetroWindow window)
		{
			var settings = new Settings {AffirmativeButtonText = "show instructions", NegativeButtonText = "close"};
			var result = await window.ShowMessageAsync("There was a problem updating the log.config",
										"New log.config settings are required for HDT to function correctly.\n\nTry starting HDT as administrator.\n\nIf that does not help, click \"show instructions\" to see how to update it manually.",
										AffirmativeAndNegative, settings);
			if(result == MessageDialogResult.Affirmative)
				Helper.TryOpenUrl("https://github.com/HearthSim/Hearthstone-Deck-Tracker/wiki/Setting-up-the-log.config");
		}

		public static async void ShowMissingCardsMessage(this MetroWindow window, Deck deck)
		{
			if(!deck.MissingCards.Any())
			{
				await window.ShowMessageAsync("No missing cards",
						"No cards were missing when you last exported this deck. (or you have not recently exported this deck)",
						Affirmative, new Settings {AffirmativeButtonText = "OK"});
				return;
			}
			var message = "The following cards were not found:\n";
			var totalDust = 0;
			var sets = new string[5];
			foreach(var card in deck.MissingCards)
			{
				message += "\n• " + card.LocalizedName;
				if(card.Count == 2)
					message += " x2";

				if(card.Set.Equals("CURSE OF NAXXRAMAS", CurrentCultureIgnoreCase))
					sets[0] = "and the Naxxramas DLC ";
				else if(card.Set.Equals("PROMOTION", CurrentCultureIgnoreCase))
					sets[1] = "and Promotion cards ";
				else if(card.Set.Equals("REWARD", CurrentCultureIgnoreCase))
					sets[2] = "and the Reward cards ";
				else if(card.Set.Equals("BLACKROCK MOUNTAIN", CurrentCultureIgnoreCase))
					sets[3] = "and the Blackrock Mountain DLC ";
				else if(card.Set.Equals("LEAGUE OF EXPLORERS", CurrentCultureIgnoreCase))
					sets[4] = "and the League of Explorers DLC ";
				else
					totalDust += card.DustCost * card.Count;
			}
			message += $"\n\nYou need {totalDust} dust {string.Join("", sets)}to craft the missing cards.";
			await window.ShowMessageAsync("Export incomplete", message, Affirmative, new Settings {AffirmativeButtonText = "OK"});
		}

		public static async Task<bool> ShowAddGameDialog(this MetroWindow window, Deck deck)
		{
			if(deck == null)
				return false;
			var dialog = new AddGameDialog(deck);
			await window.ShowMetroDialogAsync(dialog, new MetroDialogSettings {AffirmativeButtonText = "save", NegativeButtonText = "cancel"});
			var game = await dialog.WaitForButtonPressAsync();
			await window.HideMetroDialogAsync(dialog);
			if(game == null)
				return false;
			deck.DeckStats.AddGameResult(game);
			if(Config.Instance.HearthStatsAutoUploadNewGames)
			{
				if(game.GameMode == GameMode.Arena)
					HearthStatsManager.UploadArenaMatchAsync(game, deck, true, true).Forget();
				else
					HearthStatsManager.UploadMatchAsync(game, deck.GetSelectedDeckVersion(), true, true).Forget();
			}
			DeckStatsList.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks(forceUpdate: new[] {deck});
			return true;
		}

		public static async Task<DeckType?> ShowDeckTypeDialog(this MetroWindow window)
		{
			var dialog = new DeckTypeDialog();
			await window.ShowMetroDialogAsync(dialog);
			var type = await dialog.WaitForButtonPressAsync();
			await window.HideMetroDialogAsync(dialog);
			return type;
		}

		public static async Task<bool> ShowEditGameDialog(this MetroWindow window, GameStats game)
		{
			if(game == null)
				return false;
			var dialog = new AddGameDialog(game);
			await window.ShowMetroDialogAsync(dialog, new MetroDialogSettings {AffirmativeButtonText = "save", NegativeButtonText = "cancel"});
			var result = await dialog.WaitForButtonPressAsync();
			await window.HideMetroDialogAsync(dialog);
			if(result == null)
				return false;
			if(Config.Instance.HearthStatsAutoUploadNewGames && HearthStatsAPI.IsLoggedIn)
			{
				var deck = DeckList.Instance.Decks.FirstOrDefault(d => d.DeckId == game.DeckId);
				if(deck != null)
				{
					if(game.GameMode == GameMode.Arena)
						HearthStatsManager.UpdateArenaMatchAsync(game, deck, true, true);
					else
						HearthStatsManager.UpdateMatchAsync(game, deck.GetVersion(game.PlayerDeckVersion), true, true);
				}
			}
			DeckStatsList.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
			return true;
		}

		public static async Task<bool> ShowCheckHearthStatsMatchDeletionDialog(this MetroWindow window)
		{
			if(Config.Instance.HearthStatsAutoDeleteMatches.HasValue)
				return Config.Instance.HearthStatsAutoDeleteMatches.Value;
			var dialogResult =
				await
				window.ShowMessageAsync("Delete match(es) on HearthStats?", "You can change this setting at any time in the HearthStats menu.",
				                        AffirmativeAndNegative,
				                        new MetroDialogSettings {AffirmativeButtonText = "yes (always)", NegativeButtonText = "no (never)"});
			Config.Instance.HearthStatsAutoDeleteMatches = dialogResult == MessageDialogResult.Affirmative;
			Core.MainWindow.MenuItemCheckBoxAutoDeleteGames.IsChecked = Config.Instance.HearthStatsAutoDeleteMatches;
			Config.Save();
			return Config.Instance.HearthStatsAutoDeleteMatches != null && Config.Instance.HearthStatsAutoDeleteMatches.Value;
		}

		public static async Task<bool> ShowLanguageSelectionDialog(this MetroWindow window)
		{
			var english = await
				window.ShowMessageAsync("Select language", "", AffirmativeAndNegative,
										new Settings
										{
											AffirmativeButtonText = Helper.LanguageDict.First(x => x.Value == "enUS").Key,
											NegativeButtonText = Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key
										}) == MessageDialogResult.Affirmative;
			return english;
		}

		private static bool _awaitingMainWindowOpen;
		public static async void ShowNewArenaDeckMessageAsync(this MetroWindow window, HearthMirror.Objects.Deck deck)
		{
			if(_awaitingMainWindowOpen)
				return;
			_awaitingMainWindowOpen = true;

			if(window.WindowState == WindowState.Minimized)
				Core.TrayIcon.ShowMessage("New arena deck detected!");

			while(window.Visibility != Visibility.Visible || window.WindowState == WindowState.Minimized)
				await Task.Delay(100);

			var result = await window.ShowMessageAsync("New arena deck detected!",
												 "You can change this behaviour to \"auto save&import\" or \"manual\" in [options > tracker > importing]",
												 AffirmativeAndNegative, new Settings { AffirmativeButtonText = "Import", NegativeButtonText = "Cancel" });

			if(result == MessageDialogResult.Affirmative)
			{
				Log.Info("...saving new arena deck.");
				Core.MainWindow.ImportArenaDeck(deck);
			}
			else
				Log.Info("...discarded by user.");
			Core.Game.IgnoredArenaDecks.Add(deck.Id);
			_awaitingMainWindowOpen = false;
		}

		public class Settings : MetroDialogSettings
		{
			public Settings()
			{
				AnimateHide = AnimateShow = Config.Instance.UseAnimations;
			}
		}
	}

	public class SaveScreenshotOperation
	{
		public bool Cancelled { get; set; }
		public bool SaveLocal { get; set; }
		public bool Upload { get; set; }
	}
}
