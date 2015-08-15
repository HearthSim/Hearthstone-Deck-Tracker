#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Windows;
using MahApps.Metro.Controls.Dialogs;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public partial class MainWindow
	{
		private void BtnExport_Click(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault() ?? DeckList.Instance.ActiveDeck;
			if(deck == null)
				return;
			ExportDeck(deck.GetSelectedDeckVersion());
		}

		private async void ExportDeck(Deck deck)
		{
			var export = true;
			if(Config.Instance.ShowExportingDialog)
			{
				var message =
					string.Format(
					              "1) create a new, empty {0}-Deck {1}.\n\n2) leave the deck creation screen open.\n\n3)do not move your mouse or type after clicking \"export\"",
					              deck.Class, (Config.Instance.AutoClearDeck ? "(or open an existing one to be cleared automatically)" : ""));

				if(deck.GetSelectedDeckVersion().Cards.Any(c => c.Name == "Stalagg" || c.Name == "Feugen"))
				{
					message +=
						"\n\nIMPORTANT: If you own golden versions of Feugen or Stalagg please make sure to configure\nOptions > Other > Exporting";
				}

				var settings = new MetroDialogSettings {AffirmativeButtonText = "export"};
				var result =
					await
					this.ShowMessageAsync("Export " + deck.Name + " to Hearthstone", message, MessageDialogStyle.AffirmativeAndNegative, settings);
				export = result == MessageDialogResult.Affirmative;
			}
			if(export)
			{
				var controller = await this.ShowProgressAsync("Creating Deck", "Please do not move your mouse or type.");
				Topmost = false;
				await Task.Delay(500);
				await DeckExporter.Export(deck);
				await controller.CloseAsync();

				if(deck.MissingCards.Any())
					this.ShowMissingCardsMessage(deck);
			}
		}

		private async void BtnScreenhot_Click(object sender, RoutedEventArgs e)
		{
			var selectedDeck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(selectedDeck == null)
				return;
			Logger.WriteLine("Creating screenshot of " + selectedDeck.GetSelectedDeckVersion().GetDeckInfo(), "Screenshot");
			var screenShotWindow = new PlayerWindow(Config.Instance, selectedDeck.GetSelectedDeckVersion().Cards, true);
			screenShotWindow.Show();
			screenShotWindow.Top = 0;
			screenShotWindow.Left = 0;
			await Task.Delay(100);
			var source = PresentationSource.FromVisual(screenShotWindow);
			if(source == null)
				return;

			var dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
			var dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;

			var deck = selectedDeck.GetSelectedDeckVersion();
			var pngEncoder = Helper.ScreenshotDeck(screenShotWindow.ListViewPlayer, dpiX, dpiY, deck.Name);
			screenShotWindow.Shutdown();

			if(pngEncoder != null)
			{
				var saveOperation = await this.ShowScreenshotUploadSelectionDialog();
				var tmpFile = new FileInfo(Path.Combine(Config.Instance.DataDir, string.Format("tmp{0}.png", DateTime.Now.ToFileTime())));
				var fileName = saveOperation.SaveLocal
					               ? Helper.ShowSaveFileDialog(Helper.RemoveInvalidFileNameChars(deck.Name), "png") : tmpFile.FullName;
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
							imgurUrl = await Imgur.Upload(Config.Instance.ImgurClientId, ms, deck.Name);
							await controller.CloseAsync();
						}
					}

					if(imgurUrl != null)
					{
						await this.ShowSavedAndUploadedFileMessage(saveOperation.SaveLocal ? fileName : null, imgurUrl);
						Logger.WriteLine("Uploaded screenshot to " + imgurUrl, "Export");
					}
					else
						await this.ShowSavedFileMessage(fileName);
					Logger.WriteLine("Saved screenshot of " + deck.GetDeckInfo() + " to file: " + fileName, "Export");
				}
				if(tmpFile.Exists)
				{
					try
					{
						tmpFile.Delete();
					}
					catch(Exception ex)
					{
						Logger.WriteLine(ex.ToString(), "ExportScreenshot");
					}
				}
			}
		}

		private async void BtnSaveToFile_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(deck == null)
				return;

			var fileName = Helper.ShowSaveFileDialog(Helper.RemoveInvalidFileNameChars(deck.Name), "xml");

			if(fileName != null)
			{
				XmlManager<Deck>.Save(fileName, deck.GetSelectedDeckVersion());
				await this.ShowSavedFileMessage(fileName);
				Logger.WriteLine("Saved " + deck.GetSelectedDeckVersion().GetDeckInfo() + " to file: " + fileName, "Export");
			}
		}

		private void BtnClipboard_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault();
			if(deck == null)
				return;
			Clipboard.SetText(Helper.DeckToIdString(deck.GetSelectedDeckVersion()));
			this.ShowMessage("", "copied ids to clipboard");
			Logger.WriteLine("Copied " + deck.GetSelectedDeckVersion().GetDeckInfo() + " to clipboard", "Export");
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
					english =
						await
						this.ShowMessageAsync("Select language", "", MessageDialogStyle.AffirmativeAndNegative,
						                      new MetroDialogSettings
						                      {
							                      AffirmativeButtonText = Helper.LanguageDict.First(x => x.Value == "enUS").Key,
							                      NegativeButtonText = Helper.LanguageDict.First(x => x.Value == Config.Instance.SelectedLanguage).Key
						                      })
						== MessageDialogResult.Affirmative;
				}
				catch(Exception ex)
				{
					Logger.WriteLine(ex.ToString());
				}
			}
			try
			{
				var names =
					deck.GetSelectedDeckVersion()
						.Cards.Select(c => (english ? c.Name : c.LocalizedName) + (c.Count > 1 ? " x " + c.Count : ""))
						.Aggregate((c, n) => c + Environment.NewLine + n);
				Clipboard.SetText(names);
				this.ShowMessage("", "copied names to clipboard");
				Logger.WriteLine("Copied " + deck.GetDeckInfo() + " names to clipboard", "Export");
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error copying card names to clipboard: " + ex);
				this.ShowMessage("", "Error copying card names to clipboard.");
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
			Process.Start(deck.HearthStatsUrl);
		}
	}
}