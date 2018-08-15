#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing.Game;
using Hearthstone_Deck_Tracker.Importing.Websites;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Card = Hearthstone_Deck_Tracker.Hearthstone.Card;
using CardIds = HearthDb.CardIds;
using Deck = Hearthstone_Deck_Tracker.Hearthstone.Deck;

#endregion

namespace Hearthstone_Deck_Tracker.Importing
{
	public static class DeckImporter
	{
		internal static readonly Dictionary<string, Func<string, Task<Deck>>> Websites = new Dictionary<string, Func<string, Task<Deck>>>
		{
			{"hearthpwn", Hearthpwn.Import},
			{"marduktv", Marduktv.Import},
			{"hearthstoneplayers", Hearthstoneplayers.Import},
			{"tempostorm", Tempostorm.Import},
			{"hearthstonetopdecks", Hearthstonetopdecks.Import},
			{"hearthstonetopdeck.", Hearthstonetopdeck.Import},
			{"arenavalue", Arenavalue.Import},
			{"hearthstone-decks", Hearthstonedecks.Import},
			{"heartharena", Heartharena.Import},
			{"hearthstoneheroes", Hearthstoneheroes.Import},
			{"icy-veins", Icyveins.Import},
			{"hearthbuilder", Hearthbuilder.Import},
			{"manacrystals", Manacrystals.Import},
			{"powned", Powned.Import}
		};

		private const int BrawlDeckType = 6;
		private static List<HearthMirror.Objects.Deck> _constructedDecksCache;
		private static List<HearthMirror.Objects.Deck> _brawlDecksCache;
		private static ArenaInfo _arenaInfoCache;

		public static List<HearthMirror.Objects.Deck> ConstructedDecksCache
		{
			get { return _constructedDecksCache ?? (_constructedDecksCache = GetConstructedDecks()); }
			set { _constructedDecksCache = value; }
		}

		public static ArenaInfo ArenaInfoCache
		{
			get { return _arenaInfoCache ?? (_arenaInfoCache = Reflection.GetArenaDeck()); }
			set { _arenaInfoCache = value; }
		}

		public static List<HearthMirror.Objects.Deck> BrawlDecksCache
		{
			get { return _brawlDecksCache ?? (_brawlDecksCache = GetBrawlDecks()); }
			set { _brawlDecksCache = value; }
		}

		public static async Task<Deck> Import(string url)
		{
			Log.Info("Importing deck from " + url);
			var website = Websites.FirstOrDefault(x => url.Contains(x.Key));
			if(website.Value != null)
			{
				Log.Info("Using custom importer...");
				var deck = await website.Value.Invoke(url);
				if(deck == null)
				{
					Log.Info("Custom importer failed. Checking for meta tags...");
					return await MetaTagImporter.TryFindDeck(url);
				}
				deck.Cards = new ObservableCollection<Card>(deck.Cards.Where(x => x.Id != Database.UnknownCardId));
				return deck;
			}
			Log.Info("Using meta tags importer...");
			return await MetaTagImporter.TryFindDeck(url);
		}

		public static List<ImportedDeck> FromConstructed(bool refreshCache = true)
		{
			try
			{
				if(refreshCache)
					ConstructedDecksCache = GetConstructedDecks();
				var newDecks = GetImportedDecks(ConstructedDecksCache, DeckList.Instance.Decks);
				Log.Info($"Found {ConstructedDecksCache.Count} decks, {newDecks.Count} new");
				foreach(var deck in newDecks)
				{
					var match = Regex.Match(deck.Deck.Name, @"(.*)(v\d+\.\d+)$");
					if(match.Success)
						deck.Deck.Name = match.Groups[1].Value;
				}
				return newDecks;
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			return new List<ImportedDeck>();
		}

		private static List<HearthMirror.Objects.Deck> GetConstructedDecks()
			=> Reflection.GetDecks()?.Where(IsValidDeck).ToList() ?? new List<HearthMirror.Objects.Deck>();

		private static bool IsValidDeck(HearthMirror.Objects.Deck deck)
		{
			if(deck.Type == BrawlDeckType)
				return false;
			try
			{
				var count = deck.Cards.Sum(c => c.Count);
				return count == 30 || count == 1
					&& deck.Cards.First().Id == CardIds.Collectible.Neutral.WhizbangTheWonderful;
			}
			catch(OverflowException e)
			{
				// Probably bad data from memory
				Log.Error(e);
				return false;
			}
		}

		public static List<ImportedDeck> FromBrawl()
		{
			try
			{
				BrawlDecksCache = GetBrawlDecks();
				Log.Info($"Found {BrawlDecksCache.Count} decks");
				return GetImportedDecks(BrawlDecksCache, DeckList.Instance.Decks);
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			return new List<ImportedDeck>();
		}

		private static List<HearthMirror.Objects.Deck> GetBrawlDecks()
			=> Reflection.GetDecks()?.Where(x => x.Type == BrawlDeckType).ToList() ?? new List<HearthMirror.Objects.Deck>();

		public static List<ImportedDeck> GetImportedDecks(IEnumerable<HearthMirror.Objects.Deck> decks, IList<Deck> localDecks)
		{
			var importedDecks = new List<ImportedDeck>();
			var hsDecks = decks.ToList();
			foreach (var deck in hsDecks)
			{
				if(deck.Cards.Count == 1 && deck.Cards.Single().Id == CardIds.Collectible.Neutral.WhizbangTheWonderful)
				{
					var templateDecks = Reflection.GetTemplateDecks()?.Where(x => x.SortOrder < 2).Select(x =>
					{
						if(!Hearthstone.CardIds.CardClassHero.TryGetValue((CardClass)x.Class, out var hero))
							return null;
						return new HearthMirror.Objects.Deck
						{
							Id = x.DeckId,
							Name = x.Title,
							Cards = x.Cards,
							Hero = hero
						};
					}).Where(x => x != null);
					if(templateDecks != null)
					{
						importedDecks.AddRange(GetImportedDecks(templateDecks, localDecks));
						continue;
					}
				}

				var otherDecks = hsDecks.Except(new[] {deck});
				var existing = localDecks.Where(x => otherDecks.All(d => d.Id != x.HsId)).Select(x =>
					new
					{
						Deck = x,
						IdMatch = x.HsId == deck.Id,
						CardMatch = x.VersionsIncludingSelf.Select(x.GetVersion).Where(v => v.Cards.Sum(c => c.Count) == 30)
														 .Any(v => deck.Cards.All(c => v.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count)))
					}).Where(x => x.IdMatch || x.CardMatch).ToList();
				if(!existing.Any())
				{
					var iDeck = new ImportedDeck(deck, null, localDecks);
					if(!string.IsNullOrEmpty(iDeck.Class))
						importedDecks.Add(iDeck);
				}
				else if(!existing.Any(x => x.IdMatch && x.CardMatch) && existing.Any(x => x.IdMatch ^ x.CardMatch))
				{
					var iDeck = new ImportedDeck(deck, existing.Select(x => x.Deck).ToList(), localDecks);
					if(!string.IsNullOrEmpty(iDeck.Class))
						importedDecks.Add(iDeck);
				}
			}
			return importedDecks;
		}

		public static ArenaInfo FromArena(bool log = true)
		{
			try
			{
				ArenaInfoCache = Reflection.GetArenaDeck();
				if(ArenaInfoCache != null && log)
					Log.Info($"Found new {ArenaInfoCache.Wins}-{ArenaInfoCache.Losses} arena deck: hero={ArenaInfoCache.Deck.Hero}, cards={ArenaInfoCache.Deck.Cards.Sum(x => x.Count)}");
				else if(log)
					Log.Info("Found no arena deck");
				return ArenaInfoCache;
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			return null;
		}
	}
}
