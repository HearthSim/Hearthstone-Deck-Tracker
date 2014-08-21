using System;
using System.IO;
using System.Linq;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for DeckImport.xaml
	/// </summary>
	public partial class DeckImport
	{
		//TODO: Convert this into a Flyout with a user control inside of it!!!

		//public MainWindow Window;


		public delegate void DeckOptionsButtonClickedEvent(DeckImport sender);

		public DeckImport()
		{
			InitializeComponent();
		}

		public event DeckOptionsButtonClickedEvent DeckOptionsButtonClicked;


		public void After_Click()
		{
			if(DeckOptionsButtonClicked != null)
				DeckOptionsButtonClicked(this);
		}

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
			var url = await Helper.MainWindow.ShowInputAsync("Import deck", "", settings);
			if(string.IsNullOrEmpty(url))
				return;

			var controller = await Helper.MainWindow.ShowProgressAsync("Loading Deck...", "please wait");

			//var deck = await Helper.MainWindow._deckImporter.Import(url);
			var deck = await DeckImporter.Import(url);

			await controller.CloseAsync();

			if(deck != null)
			{
				var reimport = Helper.MainWindow.EditingDeck && Helper.MainWindow.NewDeck != null &&
				               Helper.MainWindow.NewDeck.Url == url;

				deck.Url = url;

				if(reimport) //keep old notes
					deck.Note = Helper.MainWindow.NewDeck.Note;

				if(!deck.Note.Contains(url))
					deck.Note = url + "\n" + deck.Note;

				Helper.MainWindow.SetNewDeck(deck, reimport);
			}
			else
				await Helper.MainWindow.ShowMessageAsync("Error", "Could not load deck from specified url");

			After_Click();
		}

		private async void BtnIdString_Click(object sender, RoutedEventArgs e)
		{
			var settings = new MetroDialogSettings();
			var clipboard = Clipboard.GetText();
			if(clipboard.Count(c => c == ':') > 0 && clipboard.Count(c => c == ';') > 0)
				settings.DefaultText = clipboard;

			//import dialog
			var idString = await Helper.MainWindow.ShowInputAsync("Import deck", "", settings);
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
			Helper.MainWindow.SetNewDeck(deck);

			After_Click();
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
						Helper.MainWindow.TagControlNewDeck.SetSelectedTags(deck.Tags);
					}
					Helper.MainWindow.SetNewDeck(deck);
				}
				catch(Exception ex)
				{
					Logger.WriteLine("Error getting deck from file: \n" + ex.Message + "\n" + ex.StackTrace);
				}
			}

			After_Click();
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

			Helper.MainWindow.SetNewDeck(deck);
		}
	}
}