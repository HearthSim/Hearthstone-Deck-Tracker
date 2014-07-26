using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Hearthstone_Deck_Tracker.Hearthstone;
using MahApps.Metro.Controls.Dialogs;

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for DeckImport.xaml
	/// </summary>
	public partial class DeckImport : UserControl
	{
		//TODO: Convert this into a Flyout with a user control inside of it!!!

		public MainWindow Window;


		public event DeckOptionsButtonClickedEvent DeckOptionsButtonClicked;
		public delegate void DeckOptionsButtonClickedEvent(DeckImport sender);

		public DeckImport()
		{
			InitializeComponent();
		}


		public void After_Click()
		{
			if (DeckOptionsButtonClicked != null)
				DeckOptionsButtonClicked(this);
		}

		private async void BtnWeb_Click(object sender, RoutedEventArgs e)
		{
			var settings = new MetroDialogSettings();
			var clipboard = Clipboard.GetText();
			var validUrls = new[]
				{
					"hearthstats", "hss.io", "hearthpwn", "hearthhead", "hearthstoneplayers", "tempostorm",
					"hearthstonetopdeck"
				};
			if (validUrls.Any(clipboard.Contains))
			{
				settings.DefaultText = clipboard;
			}

			//import dialog
			var url = await Window.ShowInputAsync("Import deck", "", settings);
			if (string.IsNullOrEmpty(url))
				return;

			var controller = await Window.ShowProgressAsync("Loading Deck...", "please wait");

			var deck = await Window._deckImporter.Import(url);

			await controller.CloseAsync();

			if (deck != null)
			{
				var reimport = Window._editingDeck && Window._newDeck != null && Window._newDeck.Url == url;

				deck.Url = url;

				if (reimport) //keep old notes
					deck.Note = Window._newDeck.Note;

				if (!deck.Note.Contains(url))
					deck.Note = url + "\n" + deck.Note;

				Window.SetNewDeck(deck, reimport);
			}
			else
			{
				await Window.ShowMessageAsync("Error", "Could not load deck from specified url");
			}

			After_Click();
		}

		private void BtnArenavalue_Click(object sender, RoutedEventArgs e)
		{
			Deck deck = null;
			var clipboardLines = Clipboard.GetText().Split('\n');
			if (clipboardLines.Length >= 1 && clipboardLines.Length <= 100)
			{
				try
				{
					foreach (var line in clipboardLines)
					{
						var parts = line.Split(new[] { " x " }, StringSplitOptions.RemoveEmptyEntries);
						if (parts.Length == 0) continue;
						var name = parts[0].Trim();
						while (name.Length > 0 && Helper.IsNumeric(name[0]))
							name = name.Remove(0, 1);

						var card = Window._game.GetCardFromName(name);
						if (card.Id == "UNKNOWN")
							continue;

						var count = 1;
						if (parts.Length > 1)
							int.TryParse(parts[1], out count);

						card.Count = count;

						if (deck == null)
							deck = new Deck();

						if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
							deck.Class = card.PlayerClass;

						deck.Cards.Add(card);
					}

					Window.SetNewDeck(deck);
					if (deck == null)
						Window.ShowMessage("Error loading deck", "");
				}
				catch (Exception ex)
				{
					Logger.WriteLine("Error importing from arenavalue: " + ex.StackTrace);
					Window.ShowMessage("Error loading deck", "");
				}
			}

			After_Click();
		}

		private void BtnIdString_Click(object sender, RoutedEventArgs e)
		{
			After_Click();
		}

		private void BtnFile_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new Microsoft.Win32.OpenFileDialog();
			dialog.Title = "Select Deck File";
			dialog.DefaultExt = "*.xml;*.txt";
			dialog.Filter = "Deck Files|*.txt;*.xml";
			var dialogResult = dialog.ShowDialog();
			if (dialogResult == true)
			{
				try
				{
					Deck deck = null;

					if (dialog.FileName.EndsWith(".txt"))
					{
						using (var sr = new System.IO.StreamReader(dialog.FileName))
						{
							deck = new Deck();
							var lines = sr.ReadToEnd().Split('\n');
							foreach (var line in lines)
							{
								var card = Window._game.GetCardFromName(line.Trim());
								if (card.Name == "") continue;

								if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
								{
									deck.Class = card.PlayerClass;
								}

								if (deck.Cards.Contains(card))
								{
									var deckCard = Window.deck.Cards.First(c => c.Equals(card));
									deck.Cards.Remove(deckCard);
									deckCard.Count++;
									deck.Cards.Add(deckCard);
								}
								else
								{
									deck.Cards.Add(card);
								}
							}

						}
					}
					else if (dialog.FileName.EndsWith(".xml"))
					{
						deck = Window._xmlManagerDeck.Load(dialog.FileName);
						//not all required information is saved in xml
						foreach (var card in deck.Cards)
						{
							card.Load();
						}
						Window.TagControlNewDeck.SetSelectedTags(deck.Tags);

					}
					Window.SetNewDeck(deck);
				}
				catch (Exception ex)
				{
					Logger.WriteLine("Error getting deck from file: \n" + ex.Message + "\n" + ex.StackTrace);
				}
			}

			After_Click();
		}
	}
}
