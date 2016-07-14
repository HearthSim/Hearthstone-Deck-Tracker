#region

using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using Point = System.Drawing.Point;
using HearthDb.Enums;
using HearthMirror;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Importing.Game;
using Deck = Hearthstone_Deck_Tracker.Hearthstone.Deck;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class MainWindow
	{
		private void BtnWeb_Click(object sender, RoutedEventArgs e) => ImportDeck();

		public async void ImportDeck(string url = null)
		{
			if(url == null)
				url = await InputDeckURL();
			if(url == null)
				return;
			var deck = await ImportDeckFromURL(url);
			if(deck != null)
			{
				var reimport = EditingDeck && _newDeck != null && _newDeck.Url == deck.Url;

				if(reimport) //keep old notes
					deck.Note = _newDeck.Note;

				SetNewDeck(deck, reimport);
				TagControlEdit.SetSelectedTags(deck.Tags);
				if(Config.Instance.AutoSaveOnImport)
					SaveDeckWithOverwriteCheck();
			}
			else
				await this.ShowMessageAsync("Error", "Could not load deck from specified url");
		}

		private async Task<string> InputDeckURL()
		{
			var settings = new MessageDialogs.Settings();
			var validUrls = DeckImporter.Websites.Keys.Select(x => x.Split('.')[0]).ToArray();
			try
			{
				var clipboard = Clipboard.ContainsText() ? new string(Clipboard.GetText().Take(1000).ToArray()) : "";
				if(validUrls.Any(clipboard.Contains))
					settings.DefaultText = clipboard;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}

			if(Config.Instance.DisplayNetDeckAd)
			{
				var result =
					await
					this.ShowMessageAsync("NetDeck",
					                      "For easier (one-click!) web importing check out the NetDeck Chrome Extension!\n\n(This message will not be displayed again, no worries.)",
					                      MessageDialogStyle.AffirmativeAndNegative,
					                      new MessageDialogs.Settings {AffirmativeButtonText = "Show me!", NegativeButtonText = "No thanks"});

				if(result == MessageDialogResult.Affirmative)
				{
					Helper.TryOpenUrl("https://chrome.google.com/webstore/detail/netdeck/lpdbiakcpmcppnpchohihcbdnojlgeel");
					var enableOptionResult =
						await
						this.ShowMessageAsync("Enable one-click importing?",
						                      "Would you like to enable one-click importing via NetDeck?\n(options > other > importing)",
						                      MessageDialogStyle.AffirmativeAndNegative,
						                      new MessageDialogs.Settings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"});
					if(enableOptionResult == MessageDialogResult.Affirmative)
					{
						Options.OptionsTrackerImporting.CheckboxImportNetDeck.IsChecked = true;
						Config.Instance.NetDeckClipboardCheck = true;
						Config.Save();
					}
				}

				Config.Instance.DisplayNetDeckAd = false;
				Config.Save();
			}


			//import dialog
			var url =
				await this.ShowInputAsync("Import deck", "Supported websites:\n" + validUrls.Aggregate((x, next) => x + ", " + next), settings);
			return url;
		}

		private async Task<Deck> ImportDeckFromURL(string url)
		{
			var controller = await this.ShowProgressAsync("Loading Deck...", "please wait");

			//var deck = await this._deckImporter.Import(url);
			var deck = await DeckImporter.Import(url);

			if(deck != null)
				deck.Url = url;

			await controller.CloseAsync();
			return deck;
		}

		private async void BtnIdString_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var settings = new MessageDialogs.Settings();
				var clipboard = Clipboard.ContainsText() ? Clipboard.GetText() : "";
				if(clipboard.Count(c => c == ':') > 0 && clipboard.Count(c => c == ';') > 0)
					settings.DefaultText = clipboard;

				//import dialog
				var idString =
					await
					this.ShowInputAsync("Import deck",
					                    "id:count;id2:count2;... (e.g. EX1_050:2;EX1_556:1;)\nObtained from: \nEXPORT > COPY IDS TO CLIPBOARD",
					                    settings);
				if(string.IsNullOrEmpty(idString))
					return;
				var deck = new Deck();
				foreach(var entry in idString.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries))
				{
					var splitEntry = entry.Split(':');
					if(splitEntry.Length != 2)
						continue;
					var card = Database.GetCardFromId(splitEntry[0]);
					if(card.Id == "UNKNOWN")
						continue;
					int count;
					int.TryParse(splitEntry[1], out count);
					card.Count = count;

					if(string.IsNullOrEmpty(deck.Class) && card.GetPlayerClass != "Neutral")
						deck.Class = card.GetPlayerClass;

					deck.Cards.Add(card);
				}
				SetNewDeck(deck);
				if(Config.Instance.AutoSaveOnImport)
					SaveDeckWithOverwriteCheck();
			}
			catch(Exception ex)
			{
				Log.Info("Error importing deck from clipboard(id string): " + ex);
			}
		}

		private async void BtnClipboardText_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if(NetDeck.CheckForClipboardImport())
				{
					if(!Config.Instance.NetDeckClipboardCheck.HasValue)
					{
						Options.OptionsTrackerImporting.CheckboxImportNetDeck.IsChecked = true;
						Config.Instance.NetDeckClipboardCheck = true;
						Config.Save();
					}
					return;
				}
				if(Clipboard.ContainsText())
				{
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
					var deck = Helper.ParseCardString(Clipboard.GetText(), !english);
					if(deck != null)
					{
						SetNewDeck(deck);
						if(Config.Instance.AutoSaveOnImport)
							SaveDeckWithOverwriteCheck();
					}
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}


		private void BtnFile_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog {Title = "Select Deck File", DefaultExt = "*.xml;*.txt", Filter = "Deck Files|*.txt;*.xml"};
			dialog.Multiselect = true;
			var dialogResult = dialog.ShowDialog();
			if(dialogResult == true)
			{
				foreach(String file in dialog.FileNames)
				{
					try
					{
						Deck deck = null;

						if(file.EndsWith(".txt"))
						{
							using(var sr = new StreamReader(file))
								deck = Helper.ParseCardString(sr.ReadToEnd());
						}
						else if(file.EndsWith(".xml"))
						{
							deck = XmlManager<Deck>.Load(file);
							//not all required information is saved in xml
							foreach(var card in deck.Cards)
								card.Load();
							TagControlEdit.SetSelectedTags(deck.Tags);
						}
						SetNewDeck(deck);
						if(Config.Instance.AutoSaveOnImport || dialog.FileNames.Length > 1)
							SaveDeckWithOverwriteCheck();
					}
					catch(Exception ex)
					{
						Log.Error(ex);
					}
				}
			}
		}

		private void BtnLastGame_Click(object sender, RoutedEventArgs e)
		{
			if(Core.Game.DrawnLastGame == null)
				return;
			var deck = new Deck();
			foreach(var card in Core.Game.DrawnLastGame)
			{
				if(card.IsCreated)
					continue;

				deck.Cards.Add(card);

				if(string.IsNullOrEmpty(deck.Class) && card.GetPlayerClass != "Neutral")
					deck.Class = card.PlayerClass;
			}

			SetNewDeck(deck);
		}

		private void BtnArena_Click(object sender, RoutedEventArgs e) => StartArenaImporting().Forget();

		public async Task StartArenaImporting()
		{
			ProgressDialogController controller = null;
			if(!Core.Game.IsRunning)
			{
				Log.Info("Waiting for game...");
				var result = await this.ShowMessageAsync("Importing arena deck", "Start Hearthstone and enter the 'Arena' screen.",
					MessageDialogStyle.AffirmativeAndNegative,
					new MessageDialogs.Settings() {AffirmativeButtonText = "Start Hearthstone", NegativeButtonText = "Cancel"});
				if(result == MessageDialogResult.Negative)
					return;
				Helper.StartHearthstoneAsync().Forget();
				controller = await this.ShowProgressAsync("Importing arena deck", "Waiting for Hearthstone...", true);
				while(!Core.Game.IsRunning)
				{
					if(controller.IsCanceled)
					{
						await controller.CloseAsync();
						return;
					}
					await Task.Delay(500);
				}
			}
			if(Core.Game.CurrentMode != Mode.DRAFT)
			{
				if(controller == null)
					controller = await this.ShowProgressAsync("Importing arena deck", "", true);
				controller.SetMessage("Enter the 'Arena' screen.");
				Log.Info("Waiting for DRAFT screen...");
				while(Core.Game.CurrentMode != Mode.DRAFT)
				{
					if(controller.IsCanceled)
					{
						await controller.CloseAsync();
						return;
					}
					await Task.Delay(500);
				}
			}
			var deck = DeckImporter.FromArena()?.Deck;
			while(deck == null || deck.Cards.Sum(x => x.Count) < 30)
			{
				if(controller == null)
					controller = await this.ShowProgressAsync("Importing arena deck", "", true);
				if(controller.IsCanceled)
				{
					await controller.CloseAsync();
					return;
				}
				controller.SetMessage($"Waiting for complete deck ({deck?.Cards.Sum(x => x.Count) ?? 0}/30 cards)...");
				await Task.Delay(1000);
				deck = DeckImporter.FromArena(false)?.Deck;
			}
			if(controller != null)
				await controller.CloseAsync();
			var recentArenaDecks = DeckList.Instance.Decks.Where(d => d.IsArenaDeck && d.Cards.Sum(x => x.Count) == 30).OrderByDescending(d => d.LastPlayedNewFirst).Take(15);
			var existing = recentArenaDecks.FirstOrDefault(d => d.Cards.All(c => deck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count)));
			if(existing != null)
			{
				var result = await this.ShowMessageAsync("Deck already exists", "You seem to already have this deck.",
					MessageDialogStyle.AffirmativeAndNegative,
					new MessageDialogs.Settings() { AffirmativeButtonText = "Use existing", NegativeButtonText = "Import anyway" });
				if(result == MessageDialogResult.Affirmative)
				{
					SelectDeck(existing, true);
					return;
				}
			}
			ImportArenaDeck(deck);
		}

		public void ImportArenaDeck(HearthMirror.Objects.Deck deck)
		{
			var arenaDeck = new Deck {
				Class = Database.GetCardFromId(deck.Hero).PlayerClass,
				HsId = deck.Id,
				Cards = new ObservableCollection<Card>(deck.Cards.Select(x =>
				{
					var card = Database.GetCardFromId(x.Id);
					card.Count = x.Count;
					return card;
				})),
				LastEdited = DateTime.Now,
				IsArenaDeck = true
			};
			arenaDeck.Name = Helper.ParseDeckNameTemplate(Config.Instance.ArenaDeckNameTemplate, arenaDeck);
			DeckList.Instance.Decks.Add(arenaDeck);
			DeckPickerList.UpdateDecks();
			SelectDeck(arenaDeck, true);
		}

		public async Task GetCardCounts(Deck deck)
		{
			var hsHandle = User32.GetHearthstoneWindow();
			if(!User32.IsHearthstoneInForeground())
			{
				//restore window and bring to foreground
				User32.ShowWindow(hsHandle, User32.SwRestore);
				User32.SetForegroundWindow(hsHandle);
				//wait it to actually be in foreground, else the rect might be wrong
				await Task.Delay(500);
			}
			if(!User32.IsHearthstoneInForeground())
			{
				Log.Error("Can't find Hearthstone window.");
				return;
			}
			await Task.Delay(1000);
			Core.Overlay.ForceHidden = true;
			Core.Overlay.UpdatePosition();
			const double xScale = 0.013;
			const double yScale = 0.017;
			const int targetHue = 53;
			const int hueMargin = 3;
			const int numVisibleCards = 21;
			var hsRect = User32.GetHearthstoneRect(false);
			var ratio = (4.0 / 3.0) / ((double)hsRect.Width / hsRect.Height);
			var posX = (int)Helper.GetScaledXPos(0.92, hsRect.Width, ratio);
			var startY = 71.0 / 768.0 * hsRect.Height;
			var strideY = 29.0 / 768.0 * hsRect.Height;
			var width = (int)Math.Round(hsRect.Width * xScale);
			var height = (int)Math.Round(hsRect.Height * yScale);

			for(var i = 0; i < Math.Min(numVisibleCards, deck.Cards.Count); i++)
			{
				var posY = (int)(startY + strideY * i);
				var capture = await ScreenCapture.CaptureHearthstoneAsync(new Point(posX, posY), width, height, hsHandle);
				if(capture == null)
					continue;
				var yellowPixels = 0;
				for(var x = 0; x < width; x++)
				{
					for(var y = 0; y < height; y++)
					{
						var pixel = capture.GetPixel(x, y);
						if(Math.Abs(pixel.GetHue() - targetHue) < hueMargin)
							yellowPixels++;
					}
				}
				//Console.WriteLine(yellowPixels + " of " + width * height + " - " + yellowPixels / (double)(width * height));
				//capture.Save("arenadeckimages/" + i + ".png");
				var yellowPixelRatio = yellowPixels / (double)(width * height);
				if(yellowPixelRatio > 0.25 && yellowPixelRatio < 50)
					deck.Cards[i].Count = 2;
			}

			if(deck.Cards.Count > numVisibleCards)
			{
				const int scrollClicksPerCard = 4;
				const int scrollDistance = 120;
				var clientPoint = new Point(posX, (int)startY);
				var previousPos = System.Windows.Forms.Cursor.Position;
				User32.ClientToScreen(hsHandle, ref clientPoint);
				System.Windows.Forms.Cursor.Position = new Point(clientPoint.X, clientPoint.Y);
				for(var j = 0; j < scrollClicksPerCard * (deck.Cards.Count - numVisibleCards); j++)
				{
					User32.mouse_event((uint)User32.MouseEventFlags.Wheel, 0, 0, -scrollDistance, UIntPtr.Zero);
					await Task.Delay(30);
				}
				System.Windows.Forms.Cursor.Position = previousPos;
				await Task.Delay(100);

				var remainingCards = deck.Cards.Count - numVisibleCards;
				startY = 76.0 / 768.0 * hsRect.Height + (numVisibleCards - remainingCards) * strideY;
				for(var i = 0; i < remainingCards; i++)
				{
					var posY = (int)(startY + strideY * i);
					var capture = await ScreenCapture.CaptureHearthstoneAsync(new Point(posX, posY), width, height, hsHandle);
					if(capture == null)
						continue;
					var yellowPixels = 0;
					for(var x = 0; x < width; x++)
					{
						for(var y = 0; y < height; y++)
						{
							var pixel = capture.GetPixel(x, y);
							if(Math.Abs(pixel.GetHue() - targetHue) < hueMargin)
								yellowPixels++;
						}
					}
					var yellowPixelRatio = yellowPixels / (double)(width * height);
					if(yellowPixelRatio > 0.25 && yellowPixelRatio < 50)
						deck.Cards[numVisibleCards + i].Count = 2;
				}

				System.Windows.Forms.Cursor.Position = new Point(clientPoint.X, clientPoint.Y);
				for(var j = 0; j < scrollClicksPerCard * (deck.Cards.Count - 21); j++)
				{
					User32.mouse_event((uint)User32.MouseEventFlags.Wheel, 0, 0, scrollDistance, UIntPtr.Zero);
					await Task.Delay(30);
				}
				System.Windows.Forms.Cursor.Position = previousPos;
			}

			Core.Overlay.ForceHidden = false;
			Core.Overlay.UpdatePosition();

			ActivateWindow();
		}

		private void BtnConstructed_Click(object sender, RoutedEventArgs e) => ShowImportDialog(false);

		private void BtnBrawl_Click(object sender, RoutedEventArgs e) => ShowImportDialog(true);

		internal async void ShowImportDialog(bool brawl)
		{
			DeckImportingFlyout.Reset(brawl);
			FlyoutDeckImporting.IsOpen = true;
			if(!Core.Game.IsRunning)
			{
				Log.Info("Waiting for game...");
				while(!Core.Game.IsRunning)
					await Task.Delay(500);
			}
			DeckImportingFlyout.StartedGame();
			var mode = brawl ? Mode.TAVERN_BRAWL : Mode.TOURNAMENT;
			if(Core.Game.CurrentMode != mode)
			{
				Log.Info($"Waiting for {mode} screen...");
				while(Core.Game.CurrentMode != mode)
					await Task.Delay(500);
			}
			var decks = brawl ? DeckImporter.FromBrawl() : DeckImporter.FromConstructed();
			DeckImportingFlyout.SetDecks(decks);
			Core.MainWindow.ActivateWindow();
		}
	}
}