using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Importing;
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
		private static int _autoSelectCount;
		public static Guid IgnoredDeckId;
		public static List<Card> NotFoundCards { get; set; } = new List<Card>(); 

		internal static void ResetAutoSelectCount() => _autoSelectCount = 0;

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
				var activeVersion = deck.Version;
				AutoImport(false);
				if(activeVersion != deck.Version && cardEntites.All(ce => deck.GetSelectedDeckVersion().Cards.Any(c => c.Id == ce.Key && c.Count >= ce.Count())))
				{
					//if autoimport finds a new version of the selected deck, the new version will be set as selected, but not active.
					//We are still using the old one for these checks AND exclude the selected version from possible targets to switch to,
					//so if the newly imported version matches all existing entites, use that one.
					Core.MainWindow.SelectDeck(deck, true);
					return;
				}	
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
			validDecks = validDecks.FilterByMode(mode, currentFormat);
			if(cardEntites != null)
				validDecks = validDecks.Where(x => cardEntites.All(ce => x.GetSelectedDeckVersion().Cards.Any(c => c.Id == ce.Key && c.Count >= ce.Count()))).ToList();
			if(_autoSelectCount > 1)
			{
				Log.Info("Too many auto selects. Showing dialog.");
				ShowDeckSelectionDialog(validDecks);
				return;
			}
			if(validDecks.Count == 0)
			{
				Log.Info("Could not find matching deck.");
				if(cardEntites == null || !AutoSelectDeckVersion(heroClass, mode, currentFormat, cardEntites))
					ShowDeckSelectionDialog(validDecks);
				return;
			}
			if(validDecks.Count == 1)
			{
				var deck = validDecks.Single();
				Log.Info("Found one matching deck: " + deck);
				Core.MainWindow.SelectDeck(deck, true);
				_autoSelectCount++;
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
					_autoSelectCount++;
					return;
				}
			}
			if(cardEntites == null || !AutoSelectDeckVersion(heroClass, mode, currentFormat, cardEntites))
				ShowDeckSelectionDialog(validDecks);
		}

		private static bool AutoSelectDeckVersion(string heroClass, GameMode mode, Format? format, List<IGrouping<string, Entity>> cardEntites)
		{
			var validDecks = DeckList.Instance.Decks.Where(x => x.Class == heroClass && !x.Archived).ToList();
			validDecks = validDecks.FilterByMode(mode, format);
			foreach(var deck in validDecks)
			{
				foreach(var version in deck.VersionsIncludingSelf.Where(x => x != deck.SelectedVersion).Select(deck.GetVersion))
				{
					if(!cardEntites.All(ce => version.Cards.Any(c => c.Id == ce.Key && c.Count >= ce.Count())))
						continue;
					Log.Info($"Found matching version on {deck.Name}: {version.Version.ShortVersionString}.");
					deck.SelectVersion(version);
					Core.MainWindow.SelectDeck(deck, true);
					_autoSelectCount++;
					return true;
				}
			}
			Log.Info("Found no matching version.");
			return false;
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
						LastEdited = DateTime.Now,
						IsArenaDeck = false
					};
					if(brawl)
					{
						newDeck.Tags.Add("Brawl");
						newDeck.Name = Helper.ParseDeckNameTemplate(Config.Instance.BrawlDeckNameTemplate, newDeck);
					}
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

		private static DateTime _lastAutoImport;
		public static bool AutoImport(bool select)
		{
			if((DateTime.UtcNow - _lastAutoImport).TotalSeconds < 5)
				return false;
			_lastAutoImport = DateTime.UtcNow;
			switch(Core.Game.CurrentGameMode)
			{
				case GameMode.Ranked:
				case GameMode.Casual:
				case GameMode.Friendly:
				case GameMode.Practice:
					return AutoImportConstructed(select);
				case GameMode.Arena:
					AutoImportArena(ArenaImportingBehaviour.AutoImportSave);
					break;
			}
			return false;
		}

		public static bool AutoImportConstructed(bool select)
		{
			var decks = DeckImporter.FromConstructed();
			if(decks.Any() && (Config.Instance.ConstructedAutoImportNew || Config.Instance.ConstructedAutoUpdate))
			{
				ImportDecks(decks, false, Config.Instance.ConstructedAutoImportNew, Config.Instance.ConstructedAutoUpdate, select);
				return true;
			}
			return false;
		}

		public static bool AutoImportArena(ArenaImportingBehaviour behaviour)
		{
			var deck = DeckImporter.FromArena();
			if(deck?.Deck.Cards.Sum(x => x.Count) != 30)
				return false;
			Log.Info($"Found new complete {deck.Deck.Hero} arena deck!");
			var recentArenaDecks =
				DeckList.Instance.Decks.Where(d => d.IsArenaDeck && d.Cards.Sum(x => x.Count) == 30).OrderByDescending(
					d => d.LastPlayedNewFirst).Take(15);
			if(recentArenaDecks.Any(d => d.Cards.All(c => deck.Deck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count))))
				Log.Info("...but we already have that one. Discarding.");
			else if(Core.Game.IgnoredArenaDecks.Contains(deck.Deck.Id))
				Log.Info("...but it was already discarded by the user. No automatic action taken.");
			else if(behaviour == ArenaImportingBehaviour.AutoAsk)
			{
				Core.MainWindow.ShowNewArenaDeckMessageAsync(deck.Deck);
				return true;
			}
			else if(behaviour == ArenaImportingBehaviour.AutoImportSave)
			{
				Log.Info("...auto saving new arena deck.");
				Core.MainWindow.ImportArenaDeck(deck.Deck);
				return true;
			}
			return false;
		}
	}

	public static class DeckListExtensions
	{
		public static List<Deck> FilterByMode(this List<Deck> decks, GameMode mode, Format? format)
		{
			var filtered = new List<Deck>(decks);
			if(mode == GameMode.Arena)
				filtered = filtered.Where(x => x.IsArenaDeck && x.IsArenaRunCompleted != true).ToList();
			else if(mode != GameMode.None)
			{
				filtered = filtered.Where(x => !x.IsArenaDeck).ToList();
				if(format == Format.Standard)
					filtered = filtered.Where(x => x.StandardViable).ToList();
			}
			return filtered;
		}
	}
}
