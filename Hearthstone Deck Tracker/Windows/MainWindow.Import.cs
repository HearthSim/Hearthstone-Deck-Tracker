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
		public async void ImportDeck(string url = null)
		{
			var result = await ImportDeckFromUrl(url);
			if (result.WasCancelled)
				return;
			if (result.Deck != null)
				await ShowImportingChoice(result.Deck);
			else
				await this.ShowMessageAsync("No deck found", "Could not find a deck on" + Environment.NewLine + result.Url);
		}

		public class ImportingResult
		{
			public Deck Deck { get; set; }
			public string Url { get; set; }
			public bool WasCancelled { get; set; }
		}

		private async Task<ImportingResult> ImportDeckFromUrl(string url = null, bool checkClipboard = true)
		{
			var fromClipboard = false;
			if(url == null)
			{
				if(checkClipboard)
				{
					try
					{
						var clipboard = Clipboard.ContainsText() ? new string(Clipboard.GetText().Take(1000).ToArray()) : "";
						if(Helper.IsValidUrl(clipboard))
						{
							url = clipboard;
							fromClipboard = true;
						}
					}
					catch(Exception e)
					{
						Log.Error(e);
					}
				}
				if(url == null)
					url = await InputDeckUrl();
			}
			if(url == null)
				return new ImportingResult {WasCancelled = true};
			var controller = await this.ShowProgressAsync("Loading Deck", "Please wait...");
			var deck = await DeckImporter.Import(url);
			if(deck != null && string.IsNullOrEmpty(deck.Url))
				deck.Url = url;
			await controller.CloseAsync();
			if(deck == null && fromClipboard)
				return await ImportDeckFromUrl(checkClipboard: false);
			return new ImportingResult {Deck = deck, Url = url};
		}

		private async Task<string> InputDeckUrl()
		{
			try
			{
				return await this.ShowWebImportingDialog();
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		internal async void ImportFromIdString()
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
					int.TryParse(splitEntry[1], out var count);
					card.Count = count;

					if(string.IsNullOrEmpty(deck.Class) && card.GetPlayerClass != "Neutral")
						deck.Class = card.GetPlayerClass;

					deck.Cards.Add(card);
				}
				if(Config.Instance.AutoSaveOnImport)
					DeckManager.SaveDeck(deck);
				else
					ShowDeckEditorFlyout(deck, true);
			}
			catch(Exception ex)
			{
				Log.Info("Error importing deck from clipboard(id string): " + ex);
			}
		}

		internal void ImportFromFile()
		{
			var dialog = new OpenFileDialog {Title = "Select Deck File", DefaultExt = "*.xml;*.txt", Filter = "Deck Files|*.txt;*.xml"};
			dialog.Multiselect = true;
			var dialogResult = dialog.ShowDialog();
			if(dialogResult == true)
			{
				foreach(var file in dialog.FileNames)
				{
					try
					{
						Deck deck = null;

						if(file.EndsWith(".txt"))
						{
							using(var sr = new StreamReader(file))
								deck = StringImporter.Import(sr.ReadToEnd());
						}
						else if(file.EndsWith(".xml"))
						{
							deck = XmlManager<Deck>.Load(file);
							//not all required information is saved in xml
							foreach(var card in deck.Cards)
								card.Load();
							TagControlEdit.SetSelectedTags(deck.Tags);
						}
						if(Config.Instance.AutoSaveOnImport || dialog.FileNames.Length > 1)
							DeckManager.SaveDeck(deck);
						else
							ShowDeckEditorFlyout(deck, true);
					}
					catch(Exception ex)
					{
						Log.Error(ex);
					}
				}
			}
		}

		internal void ImportFromLastGame()
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

			ShowDeckEditorFlyout(deck, true);
		}

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
				HearthstoneRunner.StartHearthstone().Forget();
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

		private bool _clipboardImportingInProgress;
		internal async void ImportFromClipboard()
		{
			if(_clipboardImportingInProgress)
				return;
			_clipboardImportingInProgress = true;
			var deck = await ClipboardImporter.Import();
			if(deck == null)
			{
				const string dialogTitle = "MainWindow_Import_Dialog_NoDeckFound_Title";
				const string dialogText = "MainWindow_Import_Dialog_NoDeckFound_Text";
				this.ShowMessage(LocUtil.Get(dialogTitle), LocUtil.Get(dialogText)).Forget();
				_clipboardImportingInProgress = false;
				return;
			}
			await ShowImportingChoice(deck);
			_clipboardImportingInProgress = false;
		}

		private async Task ShowImportingChoice(Deck deck)
		{
			var choice = Config.Instance.PasteImportingChoice == ImportingChoice.Manual
				? await this.ShowImportingChoiceDialog() : Config.Instance.PasteImportingChoice;
			if(choice.HasValue)
			{
				if(choice.Value == ImportingChoice.SaveLocal)
					ShowDeckEditorFlyout(deck, true);
				else
					ShowExportFlyout(deck);
			}
		}
	}
}
