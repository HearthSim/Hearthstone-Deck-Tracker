#region

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;
using static MahApps.Metro.Controls.Dialogs.MessageDialogStyle;
using Clipboard = System.Windows.Clipboard;
using System.Collections.Generic;
using HearthDb;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class MainWindow
	{
		public void ShowExportFlyout(Deck deck)
		{
			DeckExportFlyout.Deck = deck.GetSelectedDeckVersion();
			FlyoutDeckExport.IsOpen = true;
		}

		public void ShowScreenshotFlyout()
		{
			DeckScreenshotFlyout.Deck = DeckPickerList.SelectedDecks.FirstOrDefault() ?? DeckList.Instance.ActiveDeck;
			FlyoutDeckScreenshot.IsOpen = true;
		}

		public void ShowDeckHistoryFlyout()
		{
			var deck = DeckPickerList.SelectedDecks.FirstOrDefault() ?? DeckList.Instance.ActiveDeck;
			if(!deck?.HasVersions ?? true)
				return;
			DeckHistoryFlyout.Deck = deck;
			FlyoutDeckHistory.IsOpen = true;
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
					string? imgurUrl = null;
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
		
		internal async void ExportCardNamesToClipboard(Deck deck)
		{
			if(deck == null || !deck.GetSelectedDeckVersion().Cards.Any())
			{
				this.ShowMessage("", LocUtil.Get("ShowMessage_CopyCardNames_NoCards")).Forget();
				return;
			}

			try
			{
				var selectedLanguage = await this.ShowSelectLanguageDialog();
				if(!selectedLanguage.IsCanceled)
				{
					Enum.TryParse(selectedLanguage.SelectedLanguage, out Locale myLang);
					var names = deck.GetSelectedDeckVersion().Cards.ToSortedCardList()
								.Select(c => (Cards.GetFromDbfId(c.DbfId).GetLocName(myLang)) + (c.Count > 1 ? " x " + c.Count : ""))
								.Aggregate((c, n) => c + Environment.NewLine + n);

					Clipboard.SetDataObject(names);
					Log.Info("Copied " + deck.GetDeckInfo() + " names to clipboard");
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				ErrorManager.AddError("Error copying card names", LocUtil.Get("ShowMessage_CopyCardNames_Error"));
			}
			HSReplayNetClientAnalytics.OnCopyDeck(CopyDeckAction.Action.CopyNames);
		}
	}
}
