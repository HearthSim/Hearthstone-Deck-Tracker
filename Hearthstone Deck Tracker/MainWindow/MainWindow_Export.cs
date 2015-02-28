#region

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
			var deck = DeckList.Instance.ActiveDeck;
			if(deck == null)
				return;
			ExportDeck(deck.GetSelectedDeckVersion());
		}

		private async void ExportDeck(Deck deck)
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

			if(result == MessageDialogResult.Affirmative)
			{
				var controller = await this.ShowProgressAsync("Creating Deck", "Please do not move your mouse or type.");
				Topmost = false;
				await Task.Delay(500);
				await DeckExporter.Export(deck);
				await controller.CloseAsync();

				if(deck.MissingCards.Any())
				{
					string missingcards = MissingCardsMessage(deck);
					var hsRect = User32.GetHearthstoneRect(false);
					if(((float)hsRect.Width / (float)hsRect.Height) > 1.5)
						Helper.MainWindow.Overlay.ShowMissingCards(missingcards);
					else
						this.ShowMissingCardsMessage(missingcards);
				}
			}
		}

		public string MissingCardsMessage (Deck deck)
		{
			if(!deck.MissingCards.Any())
			{
				return (string)"";
			}
			var message = "The following cards were \nnot found:\n";
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

				if(card.Set.Equals("CURSE OF NAXXRAMAS", System.StringComparison.CurrentCultureIgnoreCase))
					nax = "\nand the Naxxramas DLC ";
				else if(card.Set.Equals("PROMOTION", System.StringComparison.CurrentCultureIgnoreCase))
					promo = "\nand Promotion cards ";
				else
					totalDust += dust * card.Count;
			}
			message += string.Format("\n\nYou need {0} dust {1}{2}\nto craft the missing cards.", totalDust, nax, promo);
			return message;
		}

		private async void BtnScreenhot_Click(object sender, RoutedEventArgs e)
		{
			if(DeckList.Instance.ActiveDeck == null)
				return;
			Logger.WriteLine("Creating screenshot of " + DeckList.Instance.ActiveDeckVersion.GetDeckInfo(), "Screenshot");
			var screenShotWindow = new PlayerWindow(Config.Instance, DeckList.Instance.ActiveDeckVersion.Cards, true);
			screenShotWindow.Show();
			screenShotWindow.Top = 0;
			screenShotWindow.Left = 0;
			await Task.Delay(100);
			var source = PresentationSource.FromVisual(screenShotWindow);
			if(source == null)
				return;

			var dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
			var dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;

			var deck = DeckList.Instance.ActiveDeckVersion;
			var pngEncoder = Helper.ScreenshotDeck(screenShotWindow.ListViewPlayer, dpiX, dpiY, deck.Name);
			screenShotWindow.Shutdown();

			if(pngEncoder != null)
			{
				var fileName = Helper.ShowSaveFileDialog(deck.Name, "png");

				if(fileName != null)
				{
					using(var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
						pngEncoder.Save(stream);

					await this.ShowSavedFileMessage(fileName);
					Logger.WriteLine("Saved screenshot of " + deck.GetDeckInfo() + " to file: " + fileName, "Export");
				}
			}
		}

		private async void BtnSaveToFile_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = DeckList.Instance.ActiveDeckVersion;
			if(deck == null)
				return;

			var fileName = Helper.ShowSaveFileDialog(deck.Name, "xml");

			if(fileName != null)
			{
				XmlManager<Deck>.Save(fileName, deck);
				await this.ShowSavedFileMessage(fileName);
				Logger.WriteLine("Saved " + deck.GetDeckInfo() + " to file: " + fileName, "Export");
			}
		}

		private void BtnClipboard_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = DeckList.Instance.ActiveDeckVersion;
			if(deck == null)
				return;
			Clipboard.SetText(Helper.DeckToIdString(deck));
			this.ShowMessage("", "copied to clipboard");
			Logger.WriteLine("Copied " + deck.GetDeckInfo() + " to clipboard", "Export");
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

		private void MenuItemMissingDust_OnClick(object sender, RoutedEventArgs e)
		{
			var deck = DeckList.Instance.ActiveDeckVersion;
			if(deck == null)
				return;
			string missingcards = MissingCardsMessage(deck);
			this.ShowMissingCardsMessage(missingcards);
		}
	}
}