#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.FlyoutControls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using static MahApps.Metro.Controls.Dialogs.MessageDialogStyle;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public static class MessageDialogs
	{
		private static string LocDeleteGameStatsTitle = "MessageDialogs_DeleteGameStats_Title";
		private static string LocDeleteGameStatsMultiTitle = "MessageDialogs_DeleteGameStats_Multi_Title";
		private static string LocDeleteGameStatsMultiText = "MessageDialogs_DeleteGameStats_Multi_Text";
		private static string LocDeleteGameStatsSure = "MessageDialogs_DeleteGameStats_Label_Sure";
		private static string LocDeleteGameStatsButtonDelete = "MessageDialogs_DeleteGameStats_Button_Delete";
		private static string LocDeleteGameStatsButtonCancel = "MessageDialogs_DeleteGameStats_Button_Cancel";

		private static string LocRestartTitle = "MessageDialogs_Restart_Title";
		private static string LocRestartText = "MessageDialogs_Restart_Text";
		private static string LocRestartButtonRestart = "MessageDialogs_Restart_Button_Restart";
		private static string LocRestartButtonLater = "MessageDialogs_Restart_Button_Later";

		private const string LocSavedFileText = "MessageDialogs_SavedFile_Title";
		private const string LocSavedFileButtonOk = "MessageDialogs_SavedFile_Button_Ok";
		private const string LocSavedFileButtonOpen = "MessageDialogs_SavedFile_Button_OpenFolder";
		
		private const string LocSaveUploadSaved = "MessageDialogs_SaveUpload_Text_Saved";
		private const string LocSaveUploadUploaded = "MessageDialogs_SaveUpload_Text_Uploaded";
		private const string LocSaveUploadButtonOk = "MessageDialogs_SaveUpload_Button_Ok";
		private const string LocSaveUploadButtonBrowser = "MessageDialogs_SaveUpload_Button_Browser";
		private const string LocSaveUploadButtonClipboard = "MessageDialogs_SaveUpload_Button_Clipboard";

		private const string LocScreenshotActionTitle = "MessageDialogs_ScrenshotAction_Title";
		private const string LocScreenshotActionDescription = "MessageDialogs_ScrenshotAction_Description";
		private const string LocScreenshotActionButtonSave = "MessageDialogs_ScrenshotAction_Button_Save";
		private const string LocScreenshotActionButtonSaveUpload = "MessageDialogs_ScrenshotAction_Button_SaveUpload";
		private const string LocScreenshotActionButtonUpload = "MessageDialogs_ScrenshotAction_Button_Upload";
		private const string LocScreenshotActionButtonCancel = "MessageDialogs_ScrenshotAction_Button_Cancel";

		private const string LocLogConfigTitle = "MessageDialogs_LogConfig_Title";
		private const string LocLogConfigDescription1 = "MessageDialogs_LogConfig_Description1";
		private const string LocLogConfigDescription2 = "MessageDialogs_LogConfig_Description2";
		private const string LocLogConfigDescription3 = "MessageDialogs_LogConfig_Description3";
		private const string LocLogConfigButtonInstructions = "MessageDialogs_LogConfig_Button_Instructions";
		private const string LocLogConfigButtonClose = "MessageDialogs_LogConfig_Button_Close";

		public static async Task<MessageDialogResult> ShowDeleteGameStatsMessage(this MetroWindow window, GameStats stats)
			=> await window.ShowMessageAsync(LocUtil.Get(LocDeleteGameStatsTitle),
				stats + Environment.NewLine + Environment.NewLine + LocUtil.Get(LocDeleteGameStatsSure),
				AffirmativeAndNegative,
				new Settings
				{
					AffirmativeButtonText = LocUtil.Get(LocDeleteGameStatsButtonDelete),
					NegativeButtonText = LocUtil.Get(LocDeleteGameStatsButtonCancel)
				});

		public static async Task<MessageDialogResult> ShowDeleteMultipleGameStatsMessage(this MetroWindow window, int count)
			=> await window.ShowMessageAsync(LocUtil.Get(LocDeleteGameStatsMultiTitle),
				$"{LocUtil.Get(LocDeleteGameStatsMultiText)} ({count})." + Environment.NewLine
				+ Environment.NewLine + LocUtil.Get(LocDeleteGameStatsSure),
				AffirmativeAndNegative,
				new Settings
				{
					AffirmativeButtonText = LocUtil.Get(LocDeleteGameStatsButtonDelete),
					NegativeButtonText = LocUtil.Get(LocDeleteGameStatsButtonCancel)
				});

		public static async void ShowRestartDialog()
		{
			var result = await Core.MainWindow.ShowMessageAsync(LocUtil.Get(LocRestartTitle), LocUtil.Get(LocRestartText),
				AffirmativeAndNegative,
				new Settings()
				{
					AffirmativeButtonText = LocUtil.Get(LocRestartButtonRestart),
					NegativeButtonText = LocUtil.Get(LocRestartButtonLater)
				});
			if(result == MessageDialogResult.Affirmative)
				Core.MainWindow.Restart();
		}

		public static async Task ShowMessage(this MetroWindow window, string title, string message) => await window.ShowMessageAsync(title, message);

		public static async Task ShowSavedFileMessage(this MainWindow window, string fileName)
		{
			var result = await window.ShowMessageAsync("", 
						LocUtil.Get(LocSavedFileText) + Environment.NewLine + Environment.NewLine + fileName,
						AffirmativeAndNegative,
						new Settings
						{
							AffirmativeButtonText = LocUtil.Get(LocSavedFileButtonOk),
							NegativeButtonText = LocUtil.Get(LocSavedFileButtonOpen)
						});
			if(result == MessageDialogResult.Negative)
				Process.Start(Path.GetDirectoryName(fileName));
		}

		public static async Task ShowSavedAndUploadedFileMessage(this MainWindow window, string fileName, string url)
		{
			var sb = new StringBuilder();
			if(fileName != null)
			{
				sb.AppendLine(LocUtil.Get(LocSaveUploadSaved));
				sb.AppendLine(fileName);
				sb.AppendLine();
			}
			sb.AppendLine(LocUtil.Get(LocSaveUploadUploaded));
			sb.AppendLine(url);
			var result = await window.ShowMessageAsync("", sb.ToString(), AffirmativeAndNegativeAndSingleAuxiliary,
				new Settings
				{
					AffirmativeButtonText = LocUtil.Get(LocSaveUploadButtonOk),
					NegativeButtonText = LocUtil.Get(LocSaveUploadButtonBrowser),
					FirstAuxiliaryButtonText = LocUtil.Get(LocSaveUploadButtonClipboard)
				});
			if(result == MessageDialogResult.Negative)
				Helper.TryOpenUrl(url);
			else if(result == MessageDialogResult.FirstAuxiliary)
			{
				try
				{
					Clipboard.SetDataObject(url);
				}
				catch(Exception ex)
				{
					Log.Error("Error copying url to clipboard: " + ex);
				}
			}
		}

		public static async Task<SaveScreenshotOperation> ShowScreenshotUploadSelectionDialog(this MainWindow window)
		{
			var result = await window.ShowMessageAsync(LocUtil.Get(LocScreenshotActionTitle), LocUtil.Get(LocScreenshotActionDescription),
							AffirmativeAndNegativeAndDoubleAuxiliary, new Settings
							{
								AffirmativeButtonText = LocUtil.Get(LocScreenshotActionButtonSave),
								NegativeButtonText = LocUtil.Get(LocScreenshotActionButtonSaveUpload),
								FirstAuxiliaryButtonText = LocUtil.Get(LocScreenshotActionButtonUpload),
								SecondAuxiliaryButtonText = LocUtil.Get(LocScreenshotActionButtonCancel)
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
			var settings = new Settings
			{
				AffirmativeButtonText = LocUtil.Get(LocLogConfigButtonInstructions),
				NegativeButtonText = LocUtil.Get(LocLogConfigButtonClose)
			};
			var result = await window.ShowMessageAsync(LocUtil.Get(LocLogConfigTitle),
										LocUtil.Get(LocLogConfigDescription1) + Environment.NewLine + Environment.NewLine
										+ LocUtil.Get(LocLogConfigDescription2) + Environment.NewLine + Environment.NewLine
										+ LocUtil.Get(LocLogConfigDescription3),
										AffirmativeAndNegative, settings);
			if(result == MessageDialogResult.Affirmative)
				Helper.TryOpenUrl("https://github.com/HearthSim/Hearthstone-Deck-Tracker/wiki/Setting-up-the-log.config");
		}

		public static async Task<MessageDialogResult> ShowMissingCardsMessage(this MetroWindow window, Deck deck, bool exportDialog)
		{
			if(!deck.MissingCards.Any())
			{
				return await window.ShowMessageAsync("No missing cards",
						"No cards were missing when you last exported this deck. (or you have not recently exported this deck)",
						Affirmative, new Settings {AffirmativeButtonText = "OK"});
			}
			var message = "You are missing the following cards:\n";
			var totalDust = 0;
			var sets = new List<string>();
			foreach(var card in deck.MissingCards)
			{
				message += "\n• " + card.LocalizedName;
				if(card.Count == 2)
					message += " x2";

				if(card.Set == HearthDbConverter.SetConverter(CardSet.NAXX))
					sets.Add("and the Naxxramas DLC ");
				else if(card.Set == HearthDbConverter.SetConverter(CardSet.PROMO))
					sets.Add("and Promotion cards ");
				else if(card.Set == HearthDbConverter.SetConverter(CardSet.HOF))
					sets.Add("and the Hall of Fame cards ");
				else if(card.Set == HearthDbConverter.SetConverter(CardSet.BRM))
					sets.Add("and the Blackrock Mountain DLC ");
				else if(card.Set == HearthDbConverter.SetConverter(CardSet.LOE))
					sets.Add("and the League of Explorers DLC ");
				else if(card.Set == HearthDbConverter.SetConverter(CardSet.KARA))
					sets.Add("and the One Night in Karazhan DLC ");
				else
					totalDust += card.DustCost * card.Count;
			}
			message += $"\n\nYou need {totalDust} dust {string.Join("", sets.Distinct())}to craft the missing cards.";
			var style = exportDialog ? AffirmativeAndNegative : Affirmative;
			var settings = new Settings {AffirmativeButtonText = "OK"};
			if(exportDialog)
			{
				settings.AffirmativeButtonText = "Export";
				settings.NegativeButtonText = "Cancel";
				message += "\n\nExport anyway? (this will not craft the cards)";
			}
			return await window.ShowMessageAsync("Missing cards", message, style, settings);
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

		public static async Task<string> ShowWebImportingDialog(this MetroWindow window)
		{
			var dialog = new WebImportingDialog();
			await window.ShowMetroDialogAsync(dialog);
			var type = await dialog.WaitForButtonPressAsync();
			await window.HideMetroDialogAsync(dialog);
			return type;
		}

		public static async Task<ImportingChoice?> ShowImportingChoiceDialog(this MetroWindow window)
		{
			var dialog = new ImportingChoiceDialog();
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
			DeckStatsList.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
			return true;
		}

		public static async Task<SelectLanguageOperation> ShowSelectLanguageDialog(this MetroWindow window)
		{
			var dialog = new SelectLanguageDialog();
			await window.ShowMetroDialogAsync(dialog);
			var result = await dialog.WaitForButtonPressAsync();
			await window.HideMetroDialogAsync(dialog);
			return result;
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

		internal static async void ShowDevUpdatesMessage(this MetroWindow window)
		{
			while(window.Visibility != Visibility.Visible || window.WindowState == WindowState.Minimized)
				await Task.Delay(1000);
			var result = await window.ShowMessageAsync("Development updates",
				"You just updated to a stable release but still have development updates enabled.\n"
				+ "Keeping these enabled will automatically update HDT to the next development build once it becomes available.\n\n"
				+ "Note: Development builds might be unstable. When in doubt click disable.",
				AffirmativeAndNegative,
				new Settings
				{
					AffirmativeButtonText = "Keep enabled",
					NegativeButtonText = "Disable",
				});
			var allowDevUpdates = result == MessageDialogResult.Affirmative;
			Config.Instance.AllowDevUpdates = allowDevUpdates;
			Config.Instance.CheckForDevUpdates = allowDevUpdates;
			Config.Save();
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

	public class SelectLanguageOperation
	{
		public string SelectedLanguage { get; set; }
		public bool IsCanceled { get; set; }
	}
}
