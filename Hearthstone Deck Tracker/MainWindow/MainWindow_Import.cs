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
		private readonly Regex _cardLineRegexCountFirst = new Regex(@"(^(\s*)(?<count>\d)(\s*x)?\s+)(?<cardname>[\w\s'-]+)");
		private readonly Regex _cardLineRegexCountLast = new Regex(@"(?<cardname>[\w\s'-]+)(\s+(x\s*)(?<count>\d))(\s*)$");
		private readonly Regex _cardLineRegexCountLast2 = new Regex(@"(?<cardname>[\w\s'-]+)(\s+(?<count>\d))(\s*)$");

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

				if(!deck.Note.Contains(deck.Url))
					deck.Note = deck.Url + "\n" + deck.Note;

				SetNewDeck(deck, reimport);
				TagControlEdit.SetSelectedTags(deck.Tags);
				if(Config.Instance.AutoSaveOnImport)
					await SaveDeckWithOverwriteCheck();
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
				"elitedecks"
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
						Options.OptionsOtherImporting.CheckboxImportNetDeck.IsChecked = true;
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
					await SaveDeckWithOverwriteCheck();
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error importing deck from clipboard(id string): " + ex, "Import");
			}
		}


		private async void BtnClipboardText_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if(await CheckClipboardForNetDeckImport())
				{
					if(!Config.Instance.NetDeckClipboardCheck.HasValue)
					{
						Options.OptionsOtherImporting.CheckboxImportNetDeck.IsChecked = true;
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
							await SaveDeckWithOverwriteCheck();
					}
				}
			}
			catch(Exception ex)
			{
				Logger.WriteLine("Error importing deck from clipboard(text): " + ex, "Import");
			}
		}

		private Deck ParseCardString(string cards)
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

					var card = Game.GetCardFromName(cardName);
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

		private async void BtnFile_Click(object sender, RoutedEventArgs e)
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
						await SaveDeckWithOverwriteCheck();
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
			if(Config.Instance.ShowArenaImportMessage)
			{
				await
					this.ShowMessageAsync("How this works:",
					                      "Looks like this is your first time using this feature, so let me explain:\n\n1) Build your arena deck (or enter the arena screen if you're done already)\n\n2) Leave the arena screen (go back to the main menu)\n\n3) Press \"IMPORT > FROM GAME: ARENA\"\n\n4) Adjust the numbers\n\nWhy the last step? Because this is not perfect. It is only detectable which cards are in the deck but NOT how many of each. You can increase the count of a card by just right clicking it.");
				Config.Instance.ShowArenaImportMessage = false;
				Config.Save();
				MenuItemImportArena.IsEnabled = Game.PossibleArenaCards.Count > 0;
				return;
			}

			var deck = new Deck {Name = "Arena " + DateTime.Now.ToString("dd-MM HH:mm"), IsArenaDeck = true};
			foreach(var card in Game.PossibleArenaCards)
			{
				deck.Cards.Add(card);
				if(deck.Class == null && card.GetPlayerClass != "Neutral")
					deck.Class = card.GetPlayerClass;
			}
			deck.Tags.Add("Arena");
			SetNewDeck(deck);
		}
	}
}