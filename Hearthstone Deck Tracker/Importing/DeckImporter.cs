#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing.Game;
using Hearthstone_Deck_Tracker.Importing.Websites;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Importing
{
	public static class DeckImporter
	{
		internal static readonly Dictionary<string, Func<string, Task<Deck>>> Websites = new Dictionary<string, Func<string, Task<Deck>>>
		{
			{"hearthstats", Hearthstats.Import},
			{"hss.io", Hearthstats.Import},
			{"hearthpwn", Hearthpwn.Import},
			{"hearthhead", Hearthhead.Import},
			{"hearthstoneplayers", Hearthstoneplayers.Import},
			{"tempostorm", Tempostorm.Import},
			{"hearthstonetopdecks", Hearthstonetopdecks.Import},
			{"hearthstonetopdeck.", Hearthstonetopdeck.Import},
			{"hearthnews.fr", HearthnewsFr.Import},
			{"arenavalue", Arenavalue.Import},
			{"hearthstone-decks", Hearthstonedecks.Import},
			{"heartharena", Heartharena.Import},
			{"hearthstoneheroes", Hearthstoneheroes.Import},
			{"elitedecks", Elitedecks.Import},
			{"icy-veins", Icyveins.Import},
			{"hearthbuilder", Hearthbuilder.Import},
			{"manacrystals", Manacrystals.Import}
		};

		public static async Task<Deck> Import(string url)
		{
			Log.Info("Importing deck from " + url);

			var website = Websites.FirstOrDefault(x => url.Contains(x.Key));
			if(website.Value != null)
			{
				var deck = await website.Value.Invoke(url);
				deck.Cards = new ObservableCollection<Card>(deck.Cards.Where(x => x.Id != Database.UnknownCardId));
				return deck;
			}

			Log.Error("invalid url");
			return null;
		}

		public static List<ImportedDeck> FromConstructed()
		{
			try
			{
				var decks = Reflection.GetDecks().Where(x => x.Cards.Sum(c => c.Count) == 30).ToList();
				Log.Info($"Found {decks.Count} new decks");
				var modifiedDecks = new List<ImportedDeck>();
				foreach(var deck in decks)
				{
					var existing = DeckList.Instance.Decks.Select(x =>
						new
						{
							IdMatch = x.HsId == deck.Id,
							CardMatch = deck.Cards.All(c => x.VersionsIncludingSelf.Select(x.GetVersion).Any(v => v.Cards.Any(c2 => c.Id == c2.Id && c.Count == c2.Count))),
							Deck = x
						}).Where(x => x.IdMatch || x.CardMatch).ToList();
					if(!existing.Any())
						modifiedDecks.Add(new ImportedDeck(deck, null));
					else if(existing.Any(x => x.IdMatch ^ x.CardMatch))
						modifiedDecks.Add(new ImportedDeck(deck, existing.Select(x => x.Deck).ToList()));
				}
				return modifiedDecks;
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			return new List<ImportedDeck>();
		}
	}
}