#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Controls;
using Hearthstone_Deck_Tracker.Exporting;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;
using Clipboard = System.Windows.Clipboard;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class MainWindow
	{
		private void BtnExport_Click(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault() ?? DeckList.Instance.ActiveDeck;
			if(deck == null)
				return;
			ExportDeck(deck);
		}

		private async void ExportDeck(Deck deck)
		{
			var export = true;
			if(Config.Instance.ShowExportingDialog)
			{
				var message = $"1) Create a new (or open an existing) {deck.Class} deck.\n\n2) Leave the deck creation screen open.\n\n3) Click 'Export' and do not move your mouse or type until done.";
				var settings = new MessageDialogs.Settings {AffirmativeButtonText = "Export"};
				var result = await this.ShowMessageAsync("Export " + deck.Name + " to Hearthstone", message, MessageDialogStyle.AffirmativeAndNegative, settings);
				export = result == MessageDialogResult.Affirmative;
			}
			if(export)
			{
				var controller = await this.ShowProgressAsync("Creating Deck", "Please do not move your mouse or type.");
				Topmost = false;
				await Task.Delay(500);
				var success = await DeckExporter.Export(deck);
				await controller.CloseAsync();

				if(success)
				{
					var hsDeck = HearthMirror.Reflection.GetEditedDeck();
					if(hsDeck != null)
					{
						var existingHsId = DeckList.Instance.Decks.Where(x => x.DeckId != deck.DeckId).FirstOrDefault(x => x.HsId == hsDeck.Id);
						if(existingHsId != null)
							existingHsId.HsId = 0;
						deck.HsId = hsDeck.Id;
						DeckList.Save();
					}
				}

				if(deck.MissingCards.Any())
					this.ShowMissingCardsMessage(deck);
			}
		}

		private void BtnScreenhot_Click(object sender, RoutedEventArgs e) => CaptureScreenshot(true);

		private void BtnScreenhotWithInfo_Click(object sender, RoutedEventArgs e) => CaptureScreenshot(false);

		private async void CaptureScreenshot(bool deckOnly)
		{
			var selectedDeck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(selectedDeck == null)
				return;
			Log.Info("Creating screenshot of " + selectedDeck.GetSelectedDeckVersion().GetDeckInfo());

			var deck = selectedDeck.GetSelectedDeckVersion();
			var cards = 35 * deck.Cards.Count;
			var height = (deckOnly ? 0 : 124) + cards;
			var width = 219;

			DeckView control = new DeckView(deck, deckOnly);
			control.Measure(new Size(width, height));
			control.Arrange(new Rect(new Size(width, height)));
			control.UpdateLayout();
			Log.Debug($"Screenshot: {control.ActualWidth} x {control.ActualHeight}");

			RenderTargetBitmap bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
			bmp.Render(control);
			var encoder = new PngBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(bmp));

			await SaveOrUploadScreenshot(encoder, deck.Name);
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

		private async void BtnSaveToFile_OnClick(object sender, RoutedEventArgs e)
		{
			var selectedDecks = DeckPickerList.SelectedDecks;
			if (selectedDecks.Count > 1)
			{
				if(selectedDecks.Count > 10)
				{
					var result = await
						this.ShowMessageAsync("Exporting multiple decks!", $"You are about to export {selectedDecks.Count} decks. Are you sure?",
											  MessageDialogStyle.AffirmativeAndNegative);
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

		private void BtnClipboard_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(deck == null)
				return;
			Clipboard.SetText(Helper.DeckToIdString(deck.GetSelectedDeckVersion()));
			this.ShowMessage("", "copied ids to clipboard").Forget();
			Log.Info("Copied " + deck.GetSelectedDeckVersion().GetDeckInfo() + " to clipboard");
		}

		private async void BtnClipboardNames_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(deck == null || !deck.GetSelectedDeckVersion().Cards.Any())
				return;

			var english = true;
			if(Config.Instance.SelectedLanguage != "enUS")
			{
				try
				{
					english = await this.ShowLanguageSelectionDialog();
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

		private async void BtnExportFromWeb_Click(object sender, RoutedEventArgs e)
		{
			var url = await InputDeckURL();
			if(url == null)
				return;
			var deck = await ImportDeckFromURL(url);
			if(deck != null)
				ExportDeck(deck);
			else
				await this.ShowMessageAsync("Error", "Could not load deck from specified url");
		}

		internal void MenuItemMissingDust_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(deck == null)
				return;
			this.ShowMissingCardsMessage(deck);
		}

		public void BtnOpenHearthStats_Click(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(deck == null || !deck.HasHearthStatsId)
				return;
			Helper.TryOpenUrl(deck.HearthStatsUrl);
		}
	}
}