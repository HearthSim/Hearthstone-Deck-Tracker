#region

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using MahApps.Metro.Controls.Dialogs;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Deck = Hearthstone_Deck_Tracker.Hearthstone.Deck;

#endregion

namespace Hearthstone_Deck_Tracker.Windows
{
	public partial class MainWindow
	{
		public async void ImportDeck(string? url = null)
		{
			var result = await ImportDeckFromUrl(url);
			if(result.WasCancelled)
				return;
			if(result.Deck != null)
				await ShowImportingChoice(result.Deck);
			else
				await this.ShowMessageAsync("No deck found", "Could not find a deck on" + Environment.NewLine + result.Url);
		}

		public class ImportingResult
		{
			public Deck? Deck { get; set; }
			public string? Url { get; set; }
			public bool WasCancelled { get; set; }
		}

		private async Task<ImportingResult> ImportDeckFromUrl(string? url = null, bool checkClipboard = true)
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
				return new ImportingResult { WasCancelled = true };
			var controller = await this.ShowProgressAsync("Loading Deck", "Please wait...");
			var deck = await DeckImporter.Import(url);
			if(deck != null && string.IsNullOrEmpty(deck.Url))
				deck.Url = url;
			await controller.CloseAsync();
			if(deck == null && fromClipboard)
				return await ImportDeckFromUrl(checkClipboard: false);
			return new ImportingResult { Deck = deck, Url = url };
		}

		private async Task<string?> InputDeckUrl()
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
			ActivateWindow();
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
