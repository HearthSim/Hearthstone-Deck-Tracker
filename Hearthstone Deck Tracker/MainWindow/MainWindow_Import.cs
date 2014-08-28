using System;
using System.IO;
using System.Linq;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace Hearthstone_Deck_Tracker
{
	public partial class MainWindow
	{
		private async void BtnWeb_Click(object sender, RoutedEventArgs e)
		{
			var settings = new MetroDialogSettings();
			var clipboard = Clipboard.GetText();
			var validUrls = new[]
				{
					"hearthstats", "hss.io", "hearthpwn", "hearthhead", "hearthstoneplayers", "tempostorm",
					"hearthstonetopdeck", "hearthnews.fr", "arenavalue"
				};
			if(validUrls.Any(clipboard.Contains))
				settings.DefaultText = clipboard;

			//import dialog
			var url = await this.ShowInputAsync("Import deck", "Supported websites:\n" + validUrls.Aggregate((x, next) => x + ", " + next), settings);
			if(string.IsNullOrEmpty(url))
				return;

			var controller = await this.ShowProgressAsync("Loading Deck...", "please wait");

			//var deck = await this._deckImporter.Import(url);
			var deck = await DeckImporter.Import(url);

			await controller.CloseAsync();

			if(deck != null)
			{
				var reimport = EditingDeck && _newDeck != null &&
				               _newDeck.Url == url;

				deck.Url = url;

				if(reimport) //keep old notes
					deck.Note = _newDeck.Note;

				if(!deck.Note.Contains(url))
					deck.Note = url + "\n" + deck.Note;

				SetNewDeck(deck, reimport);
			}
			else
				await this.ShowMessageAsync("Error", "Could not load deck from specified url");
		}

		private async void BtnIdString_Click(object sender, RoutedEventArgs e)
		{
			var settings = new MetroDialogSettings();
			var clipboard = Clipboard.GetText();
			if(clipboard.Count(c => c == ':') > 0 && clipboard.Count(c => c == ';') > 0)
				settings.DefaultText = clipboard;

			//import dialog
			var idString = await this.ShowInputAsync("Import deck", "", settings);
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
		}

		private void BtnFile_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new OpenFileDialog
				{
					Title = "Select Deck File",
					DefaultExt = "*.xml;*.txt",
					Filter = "Deck Files|*.txt;*.xml"
				};
			var dialogResult = dialog.ShowDialog();
			if(dialogResult == true)
			{
				try
				{
					Deck deck = null;

					if(dialog.FileName.EndsWith(".txt"))
					{
						using(var sr = new StreamReader(dialog.FileName))
						{
							deck = new Deck();
							var lines = sr.ReadToEnd().Split('\n');
							foreach(var line in lines)
							{
								var card = Game.GetCardFromName(line.Trim());
								if(card.Name == "") continue;

								if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
									deck.Class = card.PlayerClass;

								if(deck.Cards.Contains(card))
								{
									var deckCard = deck.Cards.First(c => c.Equals(card));
									deck.Cards.Remove(deckCard);
									deckCard.Count++;
									deck.Cards.Add(deckCard);
								}
								else
									deck.Cards.Add(card);
							}
						}
					}
					else if(dialog.FileName.EndsWith(".xml"))
					{
						deck = XmlManager<Deck>.Load(dialog.FileName);
						//not all required information is saved in xml
						foreach(var card in deck.Cards)
							card.Load();
						TagControlNewDeck.SetSelectedTags(deck.Tags);
					}
					SetNewDeck(deck);
				}
				catch(Exception ex)
				{
					Logger.WriteLine("Error getting deck from file: \n" + ex.Message + "\n" + ex.StackTrace);
				}
			}
		}

		private void BtnLastGame_Click(object sender, RoutedEventArgs e)
		{
			if(Game.DrawnLastGame == null) return;
			var deck = new Deck();
			foreach(var card in Game.DrawnLastGame)
			{
				if(card.IsStolen) continue;

				deck.Cards.Add(card);

				if(string.IsNullOrEmpty(deck.Class) && card.GetPlayerClass != "Neutral")
					deck.Class = card.PlayerClass;
			}

			SetNewDeck(deck);
		}
	}
}