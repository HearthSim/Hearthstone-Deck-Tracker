#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
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
	}
}