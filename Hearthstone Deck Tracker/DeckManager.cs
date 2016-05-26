using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Importing.Game;
using Hearthstone_Deck_Tracker.Importing.Game.ImportOptions;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;

namespace Hearthstone_Deck_Tracker
{
	public class DeckManager
	{
		private static bool _waitingForClass;
		private static bool _waitingForUserInput;
		private static int _waitingForDraws;
		public static Guid IgnoredDeckId;
		public static List<Card> NotFoundCards { get; set; } = new List<Card>(); 

		public static async Task DetectCurrentDeck()
		{
			var deck = DeckList.Instance.ActiveDeck;
			if(deck == null || deck.DeckId == IgnoredDeckId || _waitingForClass || _waitingForUserInput)
				return;
			if(string.IsNullOrEmpty(Core.Game.Player.Class))
			{
				_waitingForClass = true;
				while(string.IsNullOrEmpty(Core.Game.Player.Class))
					await Task.Delay(100);
				_waitingForClass = false;
			}
			var cardEntites = Core.Game.Player.RevealedEntities.Where(x => (x.IsMinion || x.IsSpell || x.IsWeapon) && !x.Info.Created && !x.Info.Stolen).GroupBy(x => x.CardId).ToList();
			var notFound = cardEntites.Where(x => !deck.GetSelectedDeckVersion().Cards.Any(c => c.Id == x.Key && c.Count >= x.Count())).ToList();
			if(notFound.Any())
			{
				NotFoundCards = notFound.SelectMany(x => x).Select(x => x.Card).Distinct().ToList();
				Log.Warn("Cards not found in deck: " + string.Join(", ", NotFoundCards.Select(x => $"{x.Name} ({x.Id})")));
				if(Config.Instance.AutoDeckDetection)
					await AutoSelectDeck(deck, Core.Game.Player.Class, Core.Game.CurrentGameMode, Core.Game.CurrentFormat, cardEntites);
			}
			else
				NotFoundCards.Clear();
		}

		private static async Task AutoSelectDeck(Deck currentDeck, string heroClass, GameMode mode, Format? currentFormat, List<IGrouping<string, Entity>> cardEntites = null)
		{
			_waitingForDraws++;
			await Task.Delay(500);
			_waitingForDraws--;
			if(_waitingForDraws > 0)
				return;
			var validDecks = DeckList.Instance.Decks.Where(x => x.Class == heroClass && !x.Archived).ToList();
			if(currentDeck != null)
				validDecks.Remove(currentDeck);
			if(mode == GameMode.Arena)
				validDecks = validDecks.Where(x => x.IsArenaDeck && x.IsArenaRunCompleted != true).ToList();
			else if(mode != GameMode.None)
			{
				validDecks = validDecks.Where(x => !x.IsArenaDeck).ToList();
				if(currentFormat == Format.Standard)
					validDecks = validDecks.Where(x => x.StandardViable).ToList();

			}
			if(validDecks.Count > 1 && cardEntites != null)
				validDecks = validDecks.Where(x => cardEntites.All(ce => x.GetSelectedDeckVersion().Cards.Any(c => c.Id == ce.Key && c.Count >= ce.Count()))).ToList();
			if(validDecks.Count == 0)
			{
				Log.Info("Could not find matching deck.");
				ShowDeckSelectionDialog(validDecks);
				return;
			}
			if(validDecks.Count == 1)
			{
				var deck = validDecks.Single();
				Log.Info("Found one matching deck: " + deck);
				Core.MainWindow.SelectDeck(deck, true);
				return;
			}
			var lastUsed = DeckList.Instance.LastDeckClass.FirstOrDefault(x => x.Class == heroClass);
			if(lastUsed != null)
			{
				var deck = validDecks.FirstOrDefault(x => x.DeckId == lastUsed.Id);
				if(deck != null)
				{
					Log.Info($"Last used {heroClass} deck matches!");
					Core.MainWindow.SelectDeck(deck, true);
					return;
				}
			}
			ShowDeckSelectionDialog(validDecks);
		}

		private static void ShowDeckSelectionDialog(List<Deck> decks)
		{
			decks.Add(new Deck("Use no deck", "", new List<Card>(), new List<string>(), "", "", DateTime.Now, false, new List<Card>(),
								   SerializableVersion.Default, new List<Deck>(), false, "", Guid.Empty, ""));
			if(decks.Count == 1 && DeckList.Instance.ActiveDeck != null)
			{
				decks.Add(new Deck("No match - Keep using active deck", "", new List<Card>(), new List<string>(), "", "", DateTime.Now, false,
								   new List<Card>(), SerializableVersion.Default, new List<Deck>(), false, "", Guid.Empty, ""));
			}
			_waitingForUserInput = true;
			Log.Info("Waiting for user input...");
			var dsDialog = new DeckSelectionDialog(decks);
			dsDialog.ShowDialog();

			var selectedDeck = dsDialog.SelectedDeck;
			if(selectedDeck != null)
			{
				if(selectedDeck.Name == "Use no deck")
				{
					Log.Info("Auto deck detection disabled.");
					Core.MainWindow.SelectDeck(null, true);
					NotFoundCards.Clear();
				}
				else if(selectedDeck.Name == "No match - Keep using active deck")
				{
					IgnoredDeckId = DeckList.Instance.ActiveDeck?.DeckId ?? Guid.Empty;
					Log.Info($"Now ignoring {DeckList.Instance.ActiveDeck?.Name}");
					NotFoundCards.Clear();
				}
				else
				{
					Log.Info("Selected deck: " + selectedDeck.Name);
					Core.MainWindow.SelectDeck(selectedDeck, true);
				}
			}
			else
			{
				Log.Info("Auto deck detection disabled.");
				Core.MainWindow.ShowMessage("Auto deck selection disabled.", "This can be re-enabled by selecting \"AUTO\" in the bottom right of the deck picker.").Forget();
				Config.Instance.AutoDeckDetection = false;
				Config.Save();
				Core.MainWindow.DeckPickerList.UpdateAutoSelectToggleButton();
			}
			_waitingForUserInput = false;
		}

		public static void ResetIgnoredDeckId() => IgnoredDeckId = Guid.Empty;

		public static void ImportDecks(IEnumerable<ImportedDeck> decks, bool brawl, bool importNew = true, bool updateExisting = true, bool select = true)
		{
			Deck toSelect = null;
			foreach(var deck in decks)
			{
				if(deck.SelectedImportOption is NewDeck)
				{
					if(!importNew)
						continue;
					Log.Info($"Saving {deck.Deck.Name} as new deck.");
					var newDeck = new Deck {
						Class = deck.Class,
						Name = deck.Deck.Name,
						HsId = deck.Deck.Id,
						Cards = new ObservableCollection<Card>(deck.Deck.Cards.Select(x =>
						{
							var card = Database.GetCardFromId(x.Id);
							card.Count = x.Count;
							return card;
						})),
						IsArenaDeck = false
					};
					if(brawl)
						newDeck.Tags.Add("Brawl");
					DeckList.Instance.Decks.Add(newDeck);
					toSelect = newDeck;
				}
				else
				{
					if(!updateExisting)
						continue;
					var existing = deck.SelectedImportOption as ExistingDeck;
					if(existing == null)
						continue;
					var target = existing.Deck;
					target.HsId = deck.Deck.Id;
					if(brawl && !target.Tags.Any(x => x.ToUpper().Contains("BRAWL")))
						target.Tags.Add("Brawl");
					if(existing.NewVersion.Major == 0)
						Log.Info($"Assinging id to existing deck: {deck.Deck.Name}.");
					else
					{
						Log.Info($"Saving {deck.Deck.Name} as {existing.NewVersion.ShortVersionString} (prev={target.Version.ShortVersionString}).");
						DeckList.Instance.Decks.Remove(target);
						var oldDeck = (Deck)target.Clone();
						oldDeck.Versions = new List<Deck>();
						target.Name = deck.Deck.Name;
						target.LastEdited = DateTime.Now;
						target.Versions.Add(oldDeck);
						target.Version = existing.NewVersion;
						target.SelectedVersion = existing.NewVersion;
						target.HearthStatsDeckVersionId = "";
						target.Cards.Clear();
						var cards = deck.Deck.Cards.Select(x =>
						{
							var card = Database.GetCardFromId(x.Id);
							card.Count = x.Count;
							return card;
						});
						foreach(var card in cards)
							target.Cards.Add(card);
						var clone = (Deck)target.Clone();
						DeckList.Instance.Decks.Add(clone);
						toSelect = clone;
					}
				}
			}
			DeckList.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
			Core.MainWindow.UpdateIntroLabelVisibility();
			if(select && toSelect != null)
				Core.MainWindow.SelectDeck(toSelect, true);
			Core.UpdatePlayerCards(true);
		}
	}
}
