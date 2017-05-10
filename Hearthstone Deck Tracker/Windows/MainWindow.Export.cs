#region

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using HearthMirror;
using Hearthstone_Deck_Tracker.Exporting;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;
using static MahApps.Metro.Controls.Dialogs.MessageDialogStyle;
using Clipboard = System.Windows.Clipboard;
using System.Collections.Generic;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class MainWindow
	{
		internal async void ExportDeck(Deck deck)
		{
			if(deck == null)
				return;
			if(Config.Instance.ShowExportingDialog)
			{
				var message = $"1) Create a new (or open an existing) {deck.Class} deck.\n\n2) Leave the deck creation screen open.\n\n3) Click 'Export' and do not move your mouse or type until done.";
				var result = await this.ShowMessageAsync("Export " + deck.Name + " to Hearthstone", message, AffirmativeAndNegative, new MessageDialogs.Settings { AffirmativeButtonText = "Export" });
				if(result == MessageDialogResult.Negative)
					return;
			}
			HearthMirror.Objects.Deck openDeck;
			var settings = new MessageDialogs.Settings() {AffirmativeButtonText = "Continue", NegativeButtonText = "Cancel"};
			while((openDeck = Reflection.GetEditedDeck()) == null)
			{
				var result = await this.ShowMessageAsync("No open deck found", "Please open a deck for editing in Hearthstone before continuing.", AffirmativeAndNegative, settings);
				if(result == MessageDialogResult.Negative)
					return;
			}
			var missingCards = ExportingHelper.GetMissingCards(deck).ToList();
			deck.MissingCards = missingCards;
			if(missingCards.Count > 0)
			{
				var result = await this.ShowMissingCardsMessage(deck, true);
				if(result == MessageDialogResult.Negative)
					return;
			}
			string selectedClass;
			while((selectedClass = Database.GetCardFromId(openDeck.Hero).PlayerClass) != deck.Class)
			{
				var result = await this.ShowMessageAsync("Incorrect class", $"Open deck is a {selectedClass} deck, but we are trying to import a {deck.Class} deck. Please create a deck with the correct class before continuing.", AffirmativeAndNegative, settings);
				if(result == MessageDialogResult.Negative)
					return;
				openDeck = Reflection.GetEditedDeck();
			}
			while(!deck.StandardViable && !openDeck.IsWild)
			{
				var result = await this.ShowMessageAsync("Not a wild deck", "Open deck is a standard deck, but we are importing a wild deck. Please switch the deck to wild before continuing.", AffirmativeAndNegative, settings);
				if(result == MessageDialogResult.Negative)
					return;
				openDeck = Reflection.GetEditedDeck();
			}
			var controller = await this.ShowProgressAsync("Creating Deck", "Please do not move your mouse or type.");
			Topmost = false;
			await Task.Delay(500);
			var success = await DeckExporter.Export(deck, async () =>
			{
				if(controller != null)
					await controller.CloseAsync();
				ActivateWindow();
				var result = await this.ShowMessageAsync("Importing interrupted", "Continue?", AffirmativeAndNegative,
					new MessageDialogs.Settings() { AffirmativeButtonText = "Continue", NegativeButtonText = "Cancel" });
				if(result == MessageDialogResult.Affirmative)
					controller = await this.ShowProgressAsync("Creating Deck", "Please do not move your mouse or type.");
				return result == MessageDialogResult.Affirmative;
			});
			if(controller.IsOpen)
				await controller.CloseAsync();
			if(success)
			{
				var hsDeck = Reflection.GetEditedDeck();
				if(hsDeck != null)
				{
					var existingHsId = DeckList.Instance.Decks.Where(x => x.DeckId != deck.DeckId).FirstOrDefault(x => x.HsId == hsDeck.Id);
					if(existingHsId != null)
						existingHsId.HsId = 0;
					deck.HsId = hsDeck.Id;
					DeckList.Save();
				}
			}
		}

		public void ShowScreenshotFlyout()
		{
			DeckScreenshotFlyout.Deck = DeckPickerList.SelectedDecks.FirstOrDefault() ?? DeckList.Instance.ActiveDeck;
			FlyoutDeckScreenshot.IsOpen = true;
		}

		public async Task SaveOrUploadScreenshot(PngBitmapEncoder pngEncoder, string proposedFileName)
		{
			if(pngEncoder != null)
			{
				var saveOperation = await this.ShowScreenshotUploadSelectionDialog();
				if(saveOperation.Cancelled)
					return;
				var tmpFile = new FileInfo(Path.Combine(Config.Instance.DataDir, $"tmp{DateTime.Now.ToFileTime()}.png"));
				var fileName = saveOperation.SaveLocal
					               ? Helper.ShowSaveFileDialog(Helper.RemoveInvalidFileNameChars(proposedFileName), "png") : tmpFile.FullName;
				if(fileName != null)
				{
					string imgurUrl = null;
					using(var ms = new MemoryStream())
					using(var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
					{
						pngEncoder.Save(ms);
						ms.WriteTo(fs);
						if(saveOperation.Upload)
						{
							var controller = await this.ShowProgressAsync("Uploading...", "");
							imgurUrl = await Imgur.Upload(Config.Instance.ImgurClientId, ms, proposedFileName);
							await controller.CloseAsync();
						}
					}

					if(imgurUrl != null)
					{
						await this.ShowSavedAndUploadedFileMessage(saveOperation.SaveLocal ? fileName : null, imgurUrl);
						Log.Info("Uploaded screenshot to " + imgurUrl);
					}
					else
						await this.ShowSavedFileMessage(fileName);
					Log.Info("Saved screenshot to: " + fileName);
				}
				if(tmpFile.Exists)
				{
					try
					{
						tmpFile.Delete();
					}
					catch(Exception ex)
					{
						Log.Error(ex);
					}
				}
			}
		}

		internal async void SaveDecksToDisk(IEnumerable<Deck> decks)
		{
			var selectedDecks = DeckPickerList.SelectedDecks;
			if (selectedDecks.Count > 1)
			{
				if(selectedDecks.Count > 10)
				{
					var result = await
						this.ShowMessageAsync("Exporting multiple decks!", $"You are about to export {selectedDecks.Count} decks. Are you sure?",
											  AffirmativeAndNegative);
					if(result != MessageDialogResult.Affirmative)
						return;
				}
				var dialog = new FolderBrowserDialog();
				if(dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
					return;
				foreach(var deck in DeckPickerList.SelectedDecks)
				{
					//Helper.GetValidFilePath avoids overwriting files and properly handles duplicate deck names
					var saveLocation = Path.Combine(dialog.SelectedPath, Helper.GetValidFilePath(dialog.SelectedPath, deck.Name, "xml"));
					XmlManager<Deck>.Save(saveLocation, deck.GetSelectedDeckVersion());
					Log.Info($"Saved {deck.GetSelectedDeckVersion().GetDeckInfo()} to file: {saveLocation}");
				}
				await this.ShowSavedFileMessage(dialog.SelectedPath);

			}
			else if(selectedDecks.Count > 0)
			{
				var deck = selectedDecks.First();
				var fileName = Helper.ShowSaveFileDialog(Helper.RemoveInvalidFileNameChars(deck.Name), "xml");
				if(fileName == null)
					return;
				XmlManager<Deck>.Save(fileName, deck.GetSelectedDeckVersion());
				await this.ShowSavedFileMessage(fileName);
				Log.Info($"Saved {deck.GetSelectedDeckVersion().GetDeckInfo()} to file: {fileName}");
			}
		}

		internal void ExportIdsToClipboard(Deck deck)
		{
			if(deck == null)
				return;
			Clipboard.SetText(Helper.DeckToIdString(deck.GetSelectedDeckVersion()));
			this.ShowMessage("", "copied ids to clipboard").Forget();
			Log.Info("Copied " + deck.GetSelectedDeckVersion().GetDeckInfo() + " to clipboard");
		}

		internal async void ExportCardNamesToClipboard(Deck deck)
		{
			if(deck == null || !deck.GetSelectedDeckVersion().Cards.Any())
				return;

			var english = true;
			var altLang = Config.Instance.AlternativeLanguages.FirstOrDefault(x => x != Config.Instance.SelectedLanguage);
			if(altLang != null || Config.Instance.SelectedLanguage != "enUS")
			{
				try
				{
					english = await this.ShowLanguageSelectionDialog(Config.Instance.SelectedLanguage == "enUS" ? altLang : Config.Instance.SelectedLanguage);
				}
				catch(Exception ex)
				{
					Log.Error(ex);
				}
			}
			try
			{
				var names =
					deck.GetSelectedDeckVersion()
					    .Cards.ToSortedCardList()
					    .Select(c => (english ? c.Name : c.LocalizedName) + (c.Count > 1 ? " x " + c.Count : ""))
					    .Aggregate((c, n) => c + Environment.NewLine + n);
				Clipboard.SetText(names);
				this.ShowMessage("", "copied names to clipboard").Forget();
				Log.Info("Copied " + deck.GetDeckInfo() + " names to clipboard");
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				this.ShowMessage("", "Error copying card names to clipboard.").Forget();
			}
		}

		internal async void ExportDeckFromWeb()
		{
			var result = await ImportDeckFromUrl();
			if(result.WasCancelled)
				return;
			if(result.Deck != null)
				ExportDeck(result.Deck);
			else
				await this.ShowMessageAsync("No deck found", "Could not find a deck on" + Environment.NewLine + result.Url);
		}
	}
}