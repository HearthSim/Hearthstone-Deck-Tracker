#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing.Game;
using Hearthstone_Deck_Tracker.Utility.Logging;
using CardIds = HearthDb.CardIds;
using Deck = Hearthstone_Deck_Tracker.Hearthstone.Deck;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using HearthDb.Enums;

#endregion

namespace Hearthstone_Deck_Tracker.Importing
{
	public static class DeckImporter
	{

		private const int BrawlDeckType = 6;
		private static List<HearthMirror.Objects.Deck>? _constructedDecksCache;
		private static List<HearthMirror.Objects.Deck>? _brawlDecksCache;
		private static ArenaInfo? _arenaInfoCache;

		public static List<HearthMirror.Objects.Deck> ConstructedDecksCache
		{
			get { return _constructedDecksCache ??= GetConstructedDecks(); }
			set { _constructedDecksCache = value; }
		}

		public static ArenaInfo ArenaInfoCache
		{
			get { return _arenaInfoCache ??= Reflection.Client.GetArenaDeck(); }
			set { _arenaInfoCache = value; }
		}

		public static List<HearthMirror.Objects.Deck> BrawlDecksCache
		{
			get { return _brawlDecksCache ??= GetBrawlDecks(); }
			set { _brawlDecksCache = value; }
		}

		public static async Task<Deck?> Import(string url)
		{
			Log.Info("Importing deck from " + url + " using meta tags importer");
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
			=> Reflection.Client.GetDecks()?.Where(IsValidDeck).ToList() ?? new List<HearthMirror.Objects.Deck>();

		private static bool IsValidDeck(HearthMirror.Objects.Deck deck)
		{
			if(deck.Type == BrawlDeckType)
				return false;
			try
			{
				var count = deck.Cards.Sum(c => c.Count);
				return IsExactlyWhizbang(deck) || count == 30 || count == 40;
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
			=> Reflection.Client.GetDecks()?.Where(x => x.Type == BrawlDeckType).ToList() ?? new List<HearthMirror.Objects.Deck>();

		public static List<ImportedDeck> GetImportedDecks(IEnumerable<HearthMirror.Objects.Deck> decks, IList<Deck> localDecks)
		{
			var importedDecks = new List<ImportedDeck>();
			var hsDecks = decks.ToList();
			foreach (var deck in hsDecks)
			{
				if(IsExactlyWhizbang(deck))
					continue; // Cannot be imported here (will need to happen in the context of a game with a deck id)

				var otherDecks = hsDecks.Except(new[] {deck});
				var existing = localDecks.Where(x => otherDecks.All(d => d.Id != x.HsId)).Select(x =>
				{
					var mainDeckMatch = x.VersionsIncludingSelf.Select(x.GetVersion)
						.Where(v => v.Cards.Sum(c => c.Count) == 30)
						.Any(v => deck.Cards.All(c => v.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count)));
					var sideboardMatch = x.VersionsIncludingSelf.Select(x.GetVersion).Where(v =>
							v.Sideboards.Select(s => s.OwnerCardId).SequenceEqual(deck.Sideboards.Keys))
						.Any(v => deck.Sideboards.SelectMany(s => s.Value).All(c =>
							v.Sideboards.SelectMany(s => s.Cards).Any(c2 => c.Id == c2.Id && c.Count == c2.Count)));
					return new
					{
						Deck = x,
						IdMatch = x.HsId == deck.Id,
						CardMatch = mainDeckMatch && sideboardMatch
					};
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

		public static ArenaInfo? FromArena(bool log = true)
		{
			try
			{
				ArenaInfoCache = Reflection.Client.GetArenaDeck();
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

		public static ImportedDeck? FromDeckTemplateId(int deckTemplateId)
		{
			var deck = GetTemplateDeck(deckTemplateId);
			if(deck is null)
				return null;
			return new ImportedDeck(deck, null, DeckList.Instance.Decks);
		}

		private static HearthMirror.Objects.Deck? GetTemplateDeck(int deckTemplateId)
		{
			try
			{
				var deck = Reflection.Client.GetTemplateDeckByDeckTemplateId(deckTemplateId);
				if(deck is null)
					return null;

				if(!Hearthstone.CardIds.CardClassHero.TryGetValue((CardClass)deck.Class, out var hero))
					return null;

				return new HearthMirror.Objects.Deck
				{
					Id = deck.DeckId,
					Name = deck.Title,
					Cards = deck.Cards.GroupBy(dbfId => dbfId, (dbfId, dbfIds) =>
					{
						var card = Database.GetCardFromDbfId(dbfId);
						if(card == null)
							return null;
						return new HearthMirror.Objects.Card(card.Id, dbfIds.Count(), 0);
					}).WhereNotNull().ToList(),
					Hero = hero,
				};
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			return null;
		}

		public static bool IsExactlyWhizbang(long deckId, bool refreshCache = true)
		{
			if(refreshCache)
				ConstructedDecksCache = GetConstructedDecks();
			ConstructedDecksCache = GetConstructedDecks();
			var deck = ConstructedDecksCache.FirstOrDefault(d => d.Id == deckId);
			if(deck == null)
				return false;
			return IsExactlyWhizbang(deck);
		}

		public static bool IsExactlyWhizbang(HearthMirror.Objects.Deck deck)
		{
			return deck.Cards.Count == 1 && (
				deck.Cards.Single().Id == CardIds.Collectible.Neutral.WhizbangTheWonderful ||
				deck.Cards.Single().Id == CardIds.Collectible.Neutral.SplendiferousWhizbang
			);
		}
	}
}
