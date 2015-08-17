#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public partial class MainWindow
	{
		private readonly Regex _cardLineRegexCountFirst = new Regex(@"(^(\s*)(?<count>\d)(\s*x)?\s+)(?<cardname>[\w\s'\.:!-]+)");
		private readonly Regex _cardLineRegexCountLast = new Regex(@"(?<cardname>[\w\s'\.:!-]+)(\s+(x\s*)(?<count>\d))(\s*)$");
		private readonly Regex _cardLineRegexCountLast2 = new Regex(@"(?<cardname>[\w\s'\.:!-]+)(\s+(?<count>\d))(\s*)$");

		private async void BtnWeb_Click(object sender, RoutedEventArgs e)
		{
			var url = await InputDeckURL();
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
			var settings = new MetroDialogSettings();
			var clipboard = Clipboard.ContainsText() ? Clipboard.GetText() : "";
			var validUrls = new[]
			{
				"hearthstats",
				"hss.io",
				"hearthpwn",
				"hearthhead",
				"hearthstoneplayers",
				"tempostorm",
				"hearthstonetopdeck",
				"hearthnews.fr",
				"arenavalue",
				"hearthstone-decks",
				"heartharena",
				"hearthstoneheroes",
				"elitedecks",
				"icy-veins",
				"hearthbuilder"
			};
			if(validUrls.Any(clipboard.Contains))
				settings.DefaultText = clipboard;

			if(Config.Instance.DisplayNetDeckAd)
			{
				var result =
					await
					this.ShowMessageAsync("NetDeck",
					                      "For easier (one-click!) web importing check out the NetDeck Chrome Extension!\n\n(This message will not be displayed again, no worries.)",
					                      MessageDialogStyle.AffirmativeAndNegative,
					                      new MetroDialogSettings {AffirmativeButtonText = "Show me!", NegativeButtonText = "No thanks"});

				if(result == MessageDialogResult.Affirmative)
				{
					Process.Start("https://chrome.google.com/webstore/detail/netdeck/lpdbiakcpmcppnpchohihcbdnojlgeel");
					var enableOptionResult =
						await
						this.ShowMessageAsync("Enable one-click importing?",
						                      "Would you like to enable one-click importing via NetDeck?\n(options > other > importing)",
						                      MessageDialogStyle.AffirmativeAndNegative,
						                      new MetroDialogSettings {AffirmativeButtonText = "Yes", NegativeButtonText = "No"});
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
				var settings = new MetroDialogSettings();
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
					var card = Game.GetCardFromId(splitEntry[0]);
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
				Logger.WriteLine("Error importing deck from clipboard(id string): " + ex, "Import");
			}
		}

		private void BtnClipboardText_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if(CheckClipboardForNetDeckImport())
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
					var deck = ParseCardString(Clipboard.GetText());
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
				Logger.WriteLine("Error importing deck from clipboard(text): " + ex, "Import");
			}
		}

		private Deck ParseCardString(string cards, bool localizedNames = false)
		{
			try
			{
				var deck = new Deck();
				var lines = cards.Split('\n');
				foreach(var line in lines)
				{
					var count = 1;
					var cardName = line.Trim();
					Match match = null;
					if(_cardLineRegexCountFirst.IsMatch(cardName))
						match = _cardLineRegexCountFirst.Match(cardName);
					else if(_cardLineRegexCountLast.IsMatch(cardName))
						match = _cardLineRegexCountLast.Match(cardName);
					else if(_cardLineRegexCountLast2.IsMatch(cardName))
						match = _cardLineRegexCountLast2.Match(cardName);
					if(match != null)
					{
						var tmpCount = match.Groups["count"];
						if(tmpCount.Success)
							count = int.Parse(tmpCount.Value);
						cardName = match.Groups["cardname"].Value.Trim();
					}

					var card = Game.GetCardFromName(cardName, localizedNames);
					if(card == null || string.IsNullOrEmpty(card.Name))
						continue;
					card.Count = count;

					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;

					if(deck.Cards.Contains(card))
					{
						var deckCard = deck.Cards.First(c => c.Equals(card));
						deck.Cards.Remove(deckCard);
						deckCard.Count += count;
						deck.Cards.Add(deckCard);
					}
					else
						deck.Cards.Add(card);
				}
				return deck;
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error parsing card string: " + ex, "Import");
				return null;
			}
		}

		private void BtnFile_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog {Title = "Select Deck File", DefaultExt = "*.xml;*.txt", Filter = "Deck Files|*.txt;*.xml"};
			var dialogResult = dialog.ShowDialog();
			if(dialogResult == true)
			{
				try
				{
					Deck deck = null;

					if(dialog.FileName.EndsWith(".txt"))
					{
						using(var sr = new StreamReader(dialog.FileName))
							deck = ParseCardString(sr.ReadToEnd());
					}
					else if(dialog.FileName.EndsWith(".xml"))
					{
						deck = XmlManager<Deck>.Load(dialog.FileName);
						//not all required information is saved in xml
						foreach(var card in deck.Cards)
							card.Load();
						TagControlEdit.SetSelectedTags(deck.Tags);
					}
					SetNewDeck(deck);
					if(Config.Instance.AutoSaveOnImport)
						SaveDeckWithOverwriteCheck();
				}
				catch(Exception ex)
				{
					Logger.WriteLine("Error getting deck from file: \n" + ex, "Import");
				}
			}
		}

		private void BtnLastGame_Click(object sender, RoutedEventArgs e)
		{
			if(Game.DrawnLastGame == null)
				return;
			var deck = new Deck();
			foreach(var card in Game.DrawnLastGame)
			{
				if(card.IsStolen)
					continue;

				deck.Cards.Add(card);

				if(string.IsNullOrEmpty(deck.Class) && card.GetPlayerClass != "Neutral")
					deck.Class = card.PlayerClass;
			}

			SetNewDeck(deck);
		}

		private async void BtnArena_Click(object sender, RoutedEventArgs e)
		{
			if(Config.Instance.ShowArenaImportMessage || Game.PossibleArenaCards.Count < 10)
			{
				await
					this.ShowMessageAsync("How this works:",
					                      "1) Build your arena deck (or enter the arena screen if you're done already)\n\n2) Leave the arena screen (go back to the main menu)\n\n3) Press \"IMPORT > FROM GAME: ARENA\"\n\n4) Adjust the numbers\n\nWhy the last step? Because this is not perfect. It is only detectable which cards are in the deck but NOT how many of each. You can increase the count of a card by just right clicking it.\n\nYou can see this information again in 'options > tracker > importing'");

				if(Config.Instance.ShowArenaImportMessage)
				{
					Config.Instance.ShowArenaImportMessage = false;
					Config.Save();
				}
				if(Game.PossibleArenaCards.Count < 10)
					return;
			}

			var deck = new Deck {Name = Helper.ParseDeckNameTemplate(Config.Instance.ArenaDeckNameTemplate), IsArenaDeck = true};
			foreach(var card in Game.PossibleArenaCards.OrderBy(x => x.Cost).ThenBy(x => x.Type).ThenBy(x => x.LocalizedName))
			{
				deck.Cards.Add(card);
				if(deck.Class == null && card.GetPlayerClass != "Neutral")
					deck.Class = card.GetPlayerClass;
			}
			if(Config.Instance.DeckImportAutoDetectCardCount)
			{
				await this.ShowMessageAsync("Arena cards found!", "[WORK IN PROGRESS] Please enter the arena screen, then click ok. Wait until HDT has loaded the deck.\n\nPlease don't move your mouse.\n\nNote: For right now, this can currently only detect if a card has 1 or more than 1 copy (sets count to 2). Cards with more than 2 copies still have to be manually adjusted.");
				var controller = await this.ShowProgressAsync("Please wait...", "Detecting card counts...");
				await GetCardCounts(deck);
				await controller.CloseAsync();
			}
			SetNewDeck(deck);
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
				MessageBox.Show("Can't find Hearthstone window.");
				Logger.WriteLine("Can't find Hearthstone window.", "ArenaImport");
				return;
			}
			await Task.Delay(1000);
			Overlay.ForceHidden = true;
			Overlay.UpdatePosition();
			const double xScale = 0.013; 
			const double yScale = 0.017;
			const int targetHue = 53;
			const int hueMargin = 3;
			const int numVisibleCards = 21;
			var hsRect = User32.GetHearthstoneRect(false);
			var ratio = (4.0 / 3.0) / ((double)hsRect.Width / hsRect.Height);
			var posX = (int)DeckExporter.GetXPos(0.92, hsRect.Width, ratio);
			var startY = 71.0/768.0 * hsRect.Height;
			var strideY = 29.0/768.0 * hsRect.Height;
			int width = (int)Math.Round(hsRect.Width * xScale);
			int height = (int)Math.Round(hsRect.Height * yScale);

			for(var i = 0; i < Math.Min(numVisibleCards, deck.Cards.Count); i++)
			{
				var posY = (int)(startY + strideY * i);
				var capture = Helper.CaptureHearthstone(new System.Drawing.Point(posX, posY), width, height, hsHandle);
				if(capture != null)
				{
					var yellowPixels = 0;
					for(int x = 0; x < width; x++)
					{
						for(int y = 0; y < height; y++)
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
			}

			if(deck.Cards.Count > numVisibleCards)
			{
				const int scrollClicksPerCard = 4;
				const int scrollDistance = 120;
				var clientPoint = new System.Drawing.Point(posX, (int)startY);
				var previousPos = System.Windows.Forms.Cursor.Position;
				User32.ClientToScreen(hsHandle, ref clientPoint);
				System.Windows.Forms.Cursor.Position = new System.Drawing.Point(clientPoint.X, clientPoint.Y);
				for(int j = 0; j < scrollClicksPerCard * (deck.Cards.Count - numVisibleCards); j++)
				{
					User32.mouse_event((uint)User32.MouseEventFlags.Wheel, 0, 0, -scrollDistance, UIntPtr.Zero);
					await Task.Delay(30);
				}
				System.Windows.Forms.Cursor.Position = previousPos;
				await Task.Delay(100);

				var remainingCards = deck.Cards.Count - numVisibleCards;
				startY = 76.0 / 768.0 * hsRect.Height + (numVisibleCards - remainingCards) * strideY;
                for(int i = 0; i < remainingCards ; i++)
				{
					var posY = (int)(startY + strideY * i);
					var capture = Helper.CaptureHearthstone(new System.Drawing.Point(posX, posY), width, height, hsHandle);
					if(capture != null)
					{
						var yellowPixels = 0;
						for(int x = 0; x < width; x++)
						{
							for(int y = 0; y < height; y++)
							{
								var pixel = capture.GetPixel(x, y);
								if(Math.Abs(pixel.GetHue() - targetHue) < hueMargin)
									yellowPixels++;
							}
						}
						//Console.WriteLine(yellowPixels + " of " + width * height + " - " + yellowPixels / (double)(width * height));
						//capture.Save("arenadeckimages/" + i + 21 + ".png");
						var yellowPixelRatio = yellowPixels / (double)(width * height);
                        if(yellowPixelRatio > 0.25 && yellowPixelRatio < 50)
							deck.Cards[numVisibleCards + i].Count = 2;
					}
				}

				System.Windows.Forms.Cursor.Position = new System.Drawing.Point(clientPoint.X, clientPoint.Y);
				for(int j = 0; j < scrollClicksPerCard * (deck.Cards.Count - 21); j++)
				{
					User32.mouse_event((uint)User32.MouseEventFlags.Wheel, 0, 0, scrollDistance, UIntPtr.Zero);
					await Task.Delay(30);
				}
				System.Windows.Forms.Cursor.Position = previousPos;
			}

			Overlay.ForceHidden = false;
			Overlay.UpdatePosition();

			ActivateWindow();
		}

		private async void BtnConstructed_Click(object sender, RoutedEventArgs e)
		{
			if(Config.Instance.ShowConstructedImportMessage || Game.PossibleConstructedCards.Count < 10)
			{
				if(Config.Instance.ShowConstructedImportMessage)
				{
					var result =
						await
						this.ShowMessageAsync("Setting up",
						                      "This functionality requires a quick semi-automatic setup. HDT needs to know whichs cards on the first page for each class exist as golden and normal.\n\nYou may have to run the setup again if those cards change: 'options > tracker > importing'",
						                      MessageDialogStyle.AffirmativeAndNegative,
						                      new MetroDialogSettings {AffirmativeButtonText = "start", NegativeButtonText = "cancel"});
					if(result != MessageDialogResult.Affirmative)
						return;
					await Helper.SetupConstructedImporting();
					Config.Instance.ShowConstructedImportMessage = false;
					Config.Save();
				}
				await
					this.ShowMessageAsync("How this works:",
					                      "0) Build your deck\n\n1) Go to the main menu (always start from here)\n\n2) Open \"My Collection\" and open the deck you want to import (do not edit the deck at this point)\n\n3) Go straight back to the main menu\n\n4) Press \"IMPORT > FROM GAME: CONSTRUCTED\"\n\n5) Adjust the numbers\n\nWhy the last step? Because this is not perfect. It is only detectable which cards are in the deck but NOT how many of each. Depening on what requires less clicks, non-legendary cards will default to 1 or 2. There may issues importing druid cards that exist as normal and golden on your first page.\n\nYou can see this information again in 'options > tracker > importing'");
				if(Game.PossibleConstructedCards.Count(c => c.PlayerClass == "Druid" || c.PlayerClass == null) < 10
				   && Game.PossibleConstructedCards.Count(c => c.PlayerClass != "Druid") < 10)
					return;
			}


			var deck = new Deck();
			deck.Class = Game.PossibleConstructedCards.Last(c => !string.IsNullOrEmpty(c.PlayerClass)).PlayerClass;

			var legendary = Game.PossibleConstructedCards.Where(c => c.Rarity == "Legendary").ToList();
			var remaining =
				Game.PossibleConstructedCards.Where(
				                                    c =>
				                                    c.Rarity != "Legendary" && (string.IsNullOrEmpty(c.PlayerClass) || c.PlayerClass == deck.Class))
				    .ToList();
			var count = Math.Abs(30 - (2 * remaining.Count + legendary.Count)) < Math.Abs(30 - (remaining.Count + legendary.Count)) ? 2 : 1;
			foreach(var card in Game.PossibleConstructedCards)
			{
				if(!string.IsNullOrEmpty(card.PlayerClass) && card.PlayerClass != deck.Class)
					continue;
				card.Count = card.Rarity == "Legendary" ? 1 : count;
				deck.Cards.Add(card);
				if(deck.Class == null && card.GetPlayerClass != "Neutral")
					deck.Class = card.GetPlayerClass;
			}
			SetNewDeck(deck);
			HsLogReader.Instance.ClearLog();
		}
	}
}