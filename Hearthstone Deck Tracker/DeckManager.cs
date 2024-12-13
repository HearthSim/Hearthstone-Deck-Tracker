﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Importing.Game;
using Hearthstone_Deck_Tracker.Importing.Game.ImportOptions;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Windows;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using CardIds = HearthDb.CardIds;
using Deck = Hearthstone_Deck_Tracker.Hearthstone.Deck;
using Sideboard = Hearthstone_Deck_Tracker.Hearthstone.Sideboard;

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
			if(string.IsNullOrEmpty(Core.Game.Player.OriginalClass))
			{
				_waitingForClass = true;
				while(string.IsNullOrEmpty(Core.Game.Player.OriginalClass))
					await Task.Delay(100);
				_waitingForClass = false;
			}
			var cardEntites = RevealedEntites;
			var notFound = GetMissingCards(cardEntites, deck);
			if(notFound.Any())
			{
				var activeVersion = deck.Version;
				AutoImport(false);
				if(activeVersion != deck.Version && cardEntites.All(ce => deck.GetSelectedDeckVersion().Cards.Any(c => c.Id == ce.Key && c.Count >= ce.Count())))
				{
					//if autoimport finds a new version of the selected deck, the new version will be set as selected, but not active.
					//We are still using the old one for these checks AND exclude the selected version from possible targets to switch to,
					//so if the newly imported version matches all existing entities, use that one.
					Core.MainWindow.SelectDeck(deck, true);
					return;
				}
				NotFoundCards = notFound.SelectMany(x => x).Select(x => x.Card).Distinct().ToList();
				Log.Warn("Cards not found in deck: " + string.Join(", ", NotFoundCards.Select(x => $"{x.Name} ({x.Id})")));
				if(Config.Instance.AutoDeckDetection)
					await AutoSelectDeck(deck, Core.Game.Player.OriginalClass!, Core.Game.CurrentGameMode, Core.Game.CurrentFormat, cardEntites);
			}
			else
				NotFoundCards.Clear();
		}

		private static List<IGrouping<string, Entity>> RevealedEntites => Core.Game.Player.RevealedEntities
			.Where(x => x.IsPlayableCard && !x.Info.Created && !x.Info.Stolen && x.Card.Collectible).GroupBy(x => x.CardId!)
			.ToList();

		private static List<IGrouping<string, Entity>> GetMissingCards(List<IGrouping<string, Entity>> revealed, Deck deck) =>
			revealed.Where(x => !deck.GetSelectedDeckVersion().Cards.Any(c => c.Id == x.Key && c.Count >= x.Count())).ToList();

		private static async Task AutoSelectDeck(Deck currentDeck, string heroClass, GameMode mode, Format? currentFormat, List<IGrouping<string, Entity>>? cardEntites = null)
		{
			if(mode == GameMode.Battlegrounds)
			{
				Log.Info("Switching to no-deck mode for battlegrounds");
				Core.MainWindow.SelectDeck(null, true);
				return;
			}
			if(mode == GameMode.Mercenaries)
			{
				Log.Info("Switching to no-deck mode for mercenaries");
				Core.MainWindow.SelectDeck(null, true);
				return;
			}
			_waitingForDraws++;
			await Task.Delay(500);
			_waitingForDraws--;
			if(_waitingForDraws > 0)
				return;
			var validDecks = DeckList.Instance.Decks.Where(x => x.Class == heroClass && !x.Archived && !x.IsDungeonDeck && !x.IsDuelsDeck).ToList();
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
				if(cardEntites == null || !AutoSelectDeckVersion(heroClass, mode, currentFormat, cardEntites))
				{
					Log.Info("No matching deck found, using no-deck mode");
					Core.MainWindow.SelectDeck(null, true);
				}
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
			var validDecks = DeckList.Instance.Decks.Where(x => x.Class == heroClass && !x.Archived && !x.IsDungeonDeck).ToList();
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
			decks.Add(new Deck("Use no deck", "", new List<Card>(), new List<Sideboard>(), new List<string>(), "", "", DateTime.Now, false, new List<Card>(),
								   SerializableVersion.Default, new List<Deck>(), Guid.Empty));
			if(decks.Count == 1 && DeckList.Instance.ActiveDeck != null)
			{
				decks.Add(new Deck("No match - Keep using active deck", "", new List<Card>(), new List<Sideboard>(), new List<string>(), "", "", DateTime.Now, false,
								   new List<Card>(), SerializableVersion.Default, new List<Deck>(), Guid.Empty));
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
				Core.MainWindow.ShowMessage("Auto deck selection disabled.", "This can be re-enabled under 'options (advanced) > tracker > general'.").Forget();
				Config.Instance.AutoDeckDetection = false;
				Config.Save();
				Core.MainWindow.AutoDeckDetection(false);
			}
			_waitingForUserInput = false;
		}

		public static void ResetIgnoredDeckId() => IgnoredDeckId = Guid.Empty;

		public static void ImportDecks(IEnumerable<ImportedDeck> decks, bool brawl, bool importNew = true, bool updateExisting = true, bool select = true)
		{
			var imported = ImportDecksTo(DeckList.Instance.Decks, decks, brawl, importNew, updateExisting);
			if(!imported.Any())
				return;
			DeckList.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
			Core.MainWindow.UpdateIntroLabelVisibility();
			if(select)
				Core.MainWindow.SelectDeck(imported.First(), true);
			Core.UpdatePlayerCards(true);
		}

		public static List<Deck> ImportDecksTo(ICollection<Deck> targetList, IEnumerable<ImportedDeck> decks, bool brawl, bool importNew, bool updateExisting)
		{
			var importedDecks = new List<Deck>();
			foreach(var deck in decks)
			{
				if(deck.SelectedImportOption is NewDeck)
				{
					if(!importNew)
						continue;
					Log.Info($"Saving {deck.Deck.Name} as new deck.");
					var newDeck = new Deck
					{
						Class = deck.Class,
						Name = deck.Deck.Name,
						HsId = deck.Deck.Id,
						Cards = new ObservableCollection<Card>(deck.Deck.Cards.Select(x =>
						{
							var card = Database.GetCardFromId(x.Id);
							if(card == null)
								return null;
							card.Count = x.Count;
							return card;
						}).WhereNotNull()),
						Sideboards = deck.Deck.Sideboards?.Select(x =>
							new Sideboard(x.Key, x.Value.Select(c =>
							{
								var card = Database.GetCardFromId(c.Id);
								if(card == null)
									return null;
								card.Count = c.Count;
								return card;
							}).WhereNotNull().ToList()
						)).WhereNotNull().ToList() ?? new List<Sideboard>(),
						LastEdited = DateTime.Now,
						IsArenaDeck = false
					};
					if(brawl)
					{
						newDeck.Tags.Add("Brawl");
						newDeck.Name = Helper.ParseDeckNameTemplate(Config.Instance.BrawlDeckNameTemplate, newDeck);
					}

					var existingWithId = targetList.FirstOrDefault(d => d.HsId == deck.Deck.Id);
					if(existingWithId != null)
						existingWithId.HsId = 0;

					targetList.Add(newDeck);
					importedDecks.Add(newDeck);
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
					if(brawl && !target.Tags.Any(x => x.ToUpper().Contains("BRAWL"))
							&& (target.DeckStats.Games.Count == 0
								|| target.DeckStats.Games.All(g => g.GameMode == GameMode.Brawl)))
						target.Tags.Add("Brawl");
					if(target.Archived)
					{
						target.Archived = false;
						Log.Info($"Unarchiving deck: {deck.Deck.Name}.");
					}
					if(existing.NewVersion.Major == 0)
						Log.Info($"Assigning id to existing deck: {deck.Deck.Name}.");
					else
					{
						Log.Info(
							$"Saving {deck.Deck.Name} as {existing.NewVersion.ShortVersionString} (prev={target.Version.ShortVersionString}).");
						targetList.Remove(target);
						var oldDeck = (Deck) target.Clone();
						oldDeck.Versions = new List<Deck>();
						if(!brawl)
							target.Name = deck.Deck.Name;
						target.LastEdited = DateTime.Now;
						target.Versions.Add(oldDeck);
						target.Version = existing.NewVersion;
						target.SelectedVersion = existing.NewVersion;
						target.Cards.Clear();
						var cards = deck.Deck.Cards.Select(x =>
						{
							var card = Database.GetCardFromId(x.Id);
							if(card == null)
								return null;
							card.Count = x.Count;
							return card;
						}).WhereNotNull();
						foreach(var card in cards)
							target.Cards.Add(card);
						target.Sideboards.Clear();
						if (deck.Deck.Sideboards != null)
							foreach(var sideboard in deck.Deck.Sideboards)
							{
								var sideboardCards = sideboard.Value.Select(x =>
								{
									var card = Database.GetCardFromId(x.Id);
									if(card == null)
										return null;
									card.Count = x.Count;
									return card;
								}).WhereNotNull().ToList();

								target.Sideboards.Add(new Sideboard(sideboard.Key, sideboardCards));
							}
						var clone = (Deck) target.Clone();
						targetList.Add(clone);
						importedDecks.Add(clone);
					}
				}
			}
			return importedDecks;
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
					AutoImportArena(Config.Instance.SelectedArenaImportingBehaviour ?? ArenaImportingBehaviour.AutoImportSave);
					break;
			}
			return false;
		}

		public static bool AutoImportConstructed(bool select, bool brawl = false)
		{
			var decks = brawl ? DeckImporter.FromBrawl() : DeckImporter.FromConstructed();
			if(decks.Any() && (Config.Instance.ConstructedAutoImportNew || Config.Instance.ConstructedAutoUpdate))
			{
				ImportDecks(decks, brawl, Config.Instance.ConstructedAutoImportNew, Config.Instance.ConstructedAutoUpdate, select);
				return true;
			}
			return false;
		}

		public static void AutoImportArena(ArenaImportingBehaviour behaviour, ArenaInfo? info = null)
		{
			var deck = info ?? DeckImporter.FromArena();
			if(deck?.Deck.Cards.Sum(x => x.Count) != 30)
				return;

			Log.Info($"Found new complete {deck.Deck.Hero} arena deck!");

			var matchingHsId =
				DeckList.Instance.Decks.FirstOrDefault(d => d.IsArenaDeck && d.HsId != 0 && d.HsId == deck.Deck.Id);
			if(matchingHsId != null)
			{
				// update NOOOOOO! cards after expansion release
				Log.Info("...but we already know that id. Checking for changes...");

				if(matchingHsId.Cards.All(c => deck.Deck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count)))
				{
					Log.Info("No changes found.");
					return;
				}

				Log.Info("Updating deck with new cards...");
				matchingHsId.Cards.Clear();
				var cards = deck.Deck.Cards.Select(x =>
				{
					var card = Database.GetCardFromId(x.Id);
					if(card == null)
						return null;
					card.Count = x.Count;
					return card;
				}).WhereNotNull();
				foreach(var card in cards)
					matchingHsId.Cards.Add(card);
				Core.MainWindow.DeckPickerList.UpdateDecks();
				Core.MainWindow.SelectDeck(matchingHsId, true);
				return;
			}

			var recentArenaDecks = DeckList.Instance.Decks.Where(d => d.IsArenaDeck && d.Cards.Sum(x => x.Count) == 30).OrderByDescending(
					d => d.LastPlayedNewFirst).Take(15);
			if(recentArenaDecks.Any(d => d.Cards.All(c => deck.Deck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count))))
			{
				Log.Info("...but we already have that one. Discarding.");
				return;
			}

			if(Core.Game.IgnoredArenaDecks.Contains(deck.Deck.Id))
			{
				Log.Info("...but it was already discarded by the user. No automatic action taken.");
				return;
			}

			switch(behaviour)
			{
				case ArenaImportingBehaviour.AutoImportSave:
					Log.Info("...auto saving new arena deck.");
					Core.MainWindow.ImportArenaDeck(deck.Deck);
					break;
				case ArenaImportingBehaviour.AutoAsk:
					Core.MainWindow.ShowNewArenaDeckMessageAsync(deck.Deck);
					break;
			}
		}

		public static void AutoSelectTemplateDeckByDeckTemplateId(IGame game, int deckTemplateId)
		{
			var selectedDeck = DeckImporter.FromDeckTemplateId(deckTemplateId);
			if(selectedDeck == null)
				return;
			var deckId = selectedDeck.Deck.Id;
			ImportDecks(new List<ImportedDeck>() { selectedDeck }, false);
			AutoSelectDeckById(game, deckId);
		}

		public static void AutoSelectTemplateDeckByDeckId(IGame game, int deckId)
		{
			var maybeDeckTemplateId = Reflection.Client.FindDeckTemplateIdForDeckId(deckId);
			if(maybeDeckTemplateId is int deckTemplateId)
			{
				AutoSelectTemplateDeckByDeckTemplateId(game, deckTemplateId);
			}
		}

		internal static async void AutoSelectDeckById(IGame game, long id)
		{
			Log.Info($"Trying to select deck for id={id}");
			if(id <= 0)
			{
				if(game.CurrentMode == Mode.ADVENTURE)
				{
					while(game.IsDungeonMatch == null)
						await Task.Delay(500);
					if(game.IsDungeonMatch.Value)
						return;
				}
				Log.Info("No selected deck found, using no-deck mode");
				Core.MainWindow.SelectDeck(null, true);
				return;
			}

			AutoImportConstructed(false, game.CurrentMode == Mode.TAVERN_BRAWL);
			if(DeckImporter.IsExactlyWhizbang(id, refreshCache: false))
			{
				Log.Info("Selected a Whizbang deck, using no-deck mode");
				Core.MainWindow.SelectDeck(null, true);
				return;
			}

			var selectedDeck = DeckList.Instance.Decks.FirstOrDefault(x => x.HsId == id);
			if(selectedDeck == null)
			{
				Log.Warn($"No deck with id={id} found");
				return;
			}
			Log.Info("Found selected deck: " + selectedDeck.Name);
			var hsDeck = DeckImporter.FromConstructed(false).FirstOrDefault(x => x.Deck.Id == id)?.Deck;
			var selectedVersion = selectedDeck.GetSelectedDeckVersion();
			if(hsDeck != null && !selectedVersion.Cards.All(c => hsDeck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count)))
			{
				var nonSelectedVersions = selectedDeck.VersionsIncludingSelf.Where(v => v != selectedVersion.Version).Select(selectedDeck.GetVersion);
				var version = nonSelectedVersions.FirstOrDefault(v => v.Cards.All(c => hsDeck.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count)));
				if(version != null)
				{
					selectedDeck.SelectVersion(version);
					Log.Info("Switching to version: " + version.Version.ShortVersionString);
				}
			}
			else if(Equals(selectedDeck, DeckList.Instance.ActiveDeck))
			{
				Log.Info("Already using the correct deck");
				return;
			}
			Core.MainWindow.SelectDeck(selectedDeck, true);
		}

		public static void SaveDeck(Deck deck, bool invokeApi = true)
		{
			deck.Edited();
			DeckList.Instance.Decks.Add(deck);
			DeckList.Save();
			Core.MainWindow.DeckPickerList.SelectDeckAndAppropriateView(deck, forceUpdate: true);
			Core.MainWindow.SelectDeck(deck, true);
			if(invokeApi)
				DeckManagerEvents.OnDeckCreated.Execute(deck);
		}

		public static void SaveDeck(Deck baseDeck, Deck newVersion, bool overwriteCurrent = false)
		{
			DeckList.Instance.Decks.Remove(baseDeck);
			baseDeck.Versions?.Clear();
			if(!overwriteCurrent)
				newVersion.Versions.Add(baseDeck);
			newVersion.SelectedVersion = newVersion.Version;
			newVersion.Archived = false;
			SaveDeck(newVersion, false);
			DeckManagerEvents.OnDeckUpdated.Execute(newVersion);
		}

		public static void DungeonRunMatchStarted(bool newRun, CardSet set, bool isPVPDR, bool recursive = false)
		{
			if(!Config.Instance.DungeonAutoImport)
				return;
			Log.Info($"Dungeon run detected! New={newRun}, Recursive={recursive}");
			var boardHero = Core.Game.Player.Hero?.Card;
			if(boardHero == null)
				return;
			var playerClass = set == CardSet.ULDUM
				? DungeonRun.GetUldumHeroPlayerClass(boardHero.PlayerClass)
				: boardHero.PlayerClass;
			if(playerClass == null)
				return;
			var revealed = RevealedEntites;
			var existingDeck = DeckList.Instance.Decks
				.Where(x => (isPVPDR && x.IsDuelsDeck || !isPVPDR && x.IsDungeonDeck) && x.Class == playerClass
							&& !((x.IsDungeonRunCompleted ?? false) || (x.IsDuelsRunCompleted ?? false))
							&& (!newRun || x.Cards.Count == 10 || x.Cards.Count == 11)
							&& GetMissingCards(revealed, x).Count == 0)
				.OrderByDescending(x => x.LastEdited).FirstOrDefault();
			if(existingDeck == null)
			{
				if(newRun)
				{
					var hero = Core.Game.Opponent.PlayerEntities.FirstOrDefault(x => x.IsHero)?.CardId;
					if (set == CardSet.DALARAN || set == CardSet.ULDUM)
					{
						Watchers.DungeonRunWatcher.UpdateDungeonInfo();
						if(!recursive)
						{
							DungeonRunMatchStarted(newRun, set, isPVPDR, true);
							return;
						}
					}
					else
						CreateDungeonDeck(playerClass, set, isPVPDR);
				}
				else
				{
					Log.Info("We don't have an existing deck for this run, but it's not a new run");
					if(DeckList.Instance.ActiveDeck != null)
					{
						Log.Info("Switching to no deck mode");
						Core.MainWindow.SelectDeck(null, true);
					}
				}
			}
			else if(!existingDeck.Equals(DeckList.Instance.ActiveDeck))
			{
				Log.Info($"Selecting existing deck: {existingDeck.Name}");
				Core.MainWindow.SelectDeck(existingDeck, true);
			}
		}

		private static bool TryGetHeroClass(int dbfId, out string? playerClass)
		{
			playerClass = null;
			var heroCard = Database.GetCardFromDbfId(dbfId, false);
			if(heroCard != null && heroCard != Database.UnknownCard && heroCard.PlayerClass != null)
				playerClass = heroCard.PlayerClass;
			return playerClass != null;
		}

		public static void UpdateDungeonRunDeck(DungeonInfo info, bool isPVPDR)
		{
			if(!Config.Instance.DungeonAutoImport)
				return;

			var isNewPVPDR = isPVPDR && !info.RunActive && info.SelectedLoadoutTreasureDbId > 0;
			Log.Info($"Found dungeon run deck Set={(CardSet)info.CardSet}, PVPDR={isPVPDR} (new={isNewPVPDR})");

			var allCards = info.DbfIds?.ToList() ?? new List<int>();

			// New PVPDR runs have all non-loadout cards in the DbfIds. We still add the picked loadout below.
			// So we don't want to replace allCards with baseDeck, as backDeck is empty, and we don't want to add
			// any loot or treasure, as these will be the ones from a previous run, if they exist.
			if (!isNewPVPDR)
			{
				var baseDeck = info.SelectedDeck ?? new List<int>();
				if (baseDeck.All(x => allCards.Contains(x)))
				{
					if(info.PlayerChosenLoot > 0)
					{
						var loot = new[] { info.LootA, info.LootB, info.LootC };
						var chosen = loot[info.PlayerChosenLoot - 1];
						for(var i = 1; i < chosen.Count; i++)
							allCards.Add(chosen[i]);
					}
					if(info.PlayerChosenTreasure > 0)
						allCards.Add(info.Treasure[info.PlayerChosenTreasure - 1]);
				}
				else
					allCards = baseDeck;
			}

			var cards = allCards.GroupBy(x => x).Select(x =>
			{
				var card = Database.GetCardFromDbfId(x.Key, false);
				if(card == null)
					return null;
				card.Count = x.Count();
				return card;
			}).WhereNotNull().ToList();

			var loadoutCardId = info.LoadoutCardId;
			var loadout = loadoutCardId != null ? Database.GetCardFromId(loadoutCardId) : null;
			if (loadout != null && !allCards.Contains(loadout.DbfId)) {
				cards.Add(loadout);
			}

			if(!Config.Instance.DungeonRunIncludePassiveCards)
				cards.RemoveAll(c => !c.Collectible && c.HideStats);

			var cardSet = (CardSet)info.CardSet;

			string? playerClass = null;
			if(cardSet == CardSet.ULDUM && loadout != null)
				playerClass = DungeonRun.GetUldumHeroPlayerClass(loadout.PlayerClass);
			else if(isPVPDR)
			{
				if(info.HeroClass != 0)
					playerClass = HearthDbConverter.ConvertClass((CardClass)info.HeroClass);
				else if(info.HeroCardDbId != 0 && TryGetHeroClass(info.HeroCardDbId, out var heroCardClass))
					playerClass = heroCardClass;
				else if(info.PlayerSelectedHeroCardDbId != 0 && TryGetHeroClass(info.PlayerSelectedHeroCardDbId, out var selectedHeroCardClass))
					playerClass = selectedHeroCardClass;
				else if(info.HeroCardClass != 0)
					playerClass = HearthDbConverter.ConvertClass((CardClass)info.HeroCardClass);
			}
			else
			{
				if(allCards.Count == 10)
					playerClass = allCards.Select(x => Database.GetCardFromDbfId(x)?.PlayerClass).FirstOrDefault(x => x != null)?.ToUpperInvariant();
				if(playerClass == null)
					playerClass = ((CardClass)info.HeroCardClass).ToString().ToUpperInvariant();
			}

			if(playerClass == null)
			{
				Log.Warn($"Could not find playerClass");
				return;
			}

			var deck = DeckList.Instance.Decks.FirstOrDefault(x => (!isPVPDR && x.IsDungeonDeck || isPVPDR && x.IsDuelsDeck) && x.Class?.ToUpperInvariant() == playerClass.ToUpperInvariant()
																		&& x.Cards.All(e => cards.Any(c => c.Id == e.Id && c.Count >= e.Count))
																		&& !(x.IsDungeonRunCompleted ?? false)
																		&& !(x.IsDuelsRunCompleted ?? false));
			var baseDbfids = isPVPDR ? info.DbfIds : info.SelectedDeck;
			if(deck == null && (deck = CreateDungeonDeck(playerClass, cardSet, isPVPDR, baseDbfids, loadout)) == null)
			{
				Log.Info($"No existing deck - can't find default deck for {playerClass}");
				return;
			}
			if(!info.RunActive && (cardSet == CardSet.ULDUM || cardSet == CardSet.DALARAN)) {
				Log.Info($"Inactive run for Set={cardSet.ToString()} - this is a new run");
				return;
			}
			if(cards.All(c => deck.Cards.Any(e => c.Id == e.Id && c.Count == e.Count)))
			{
				Log.Info("No new cards");
				return;
			}
			deck.Cards.Clear();
			Helper.SortCardCollection(cards, false);
			foreach(var card in cards)
				deck.Cards.Add(card);
			deck.LastEdited = DateTime.Now;
			DeckList.Save();
			Core.UpdatePlayerCards(true);
			Log.Info("Updated dungeon run deck");
		}

		private static Deck? CreateDungeonDeck(string playerClass, CardSet set, bool isPVPDR, List<int>? selectedDeck = null, Card? loadout = null)
		{
			var shrine = Core.Game.Player.Board.FirstOrDefault(x => x.HasTag(GameTag.SHRINE))?.CardId ?? "";
			Log.Info($"Creating new {playerClass} dungeon run deck (CardSet={set}, Shrine={shrine}, SelectedDeck={selectedDeck != null})");
			var deck = selectedDeck == null
				? DungeonRun.GetDefaultDeck(playerClass, set, shrine)
				: DungeonRun.GetDeckFromDbfIds(playerClass, set, isPVPDR, selectedDeck);
			if(deck == null)
			{
				Log.Info($"Could not find default deck for {playerClass} in card set {set} with Shrine={shrine}");
				return null;
			}
			if (loadout != null && selectedDeck != null && !selectedDeck.Contains(loadout.DbfId))
				deck.Cards.Add(loadout);
			DeckList.Instance.Decks.Add(deck);
			DeckList.Save();
			Core.MainWindow.DeckPickerList.UpdateDecks();
			Core.MainWindow.SelectDeck(deck, true);
			return deck;
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
