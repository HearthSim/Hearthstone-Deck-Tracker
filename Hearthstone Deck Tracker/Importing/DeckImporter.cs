#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing.Websites;

#endregion

namespace Hearthstone_Deck_Tracker.Importing
{
	public static class DeckImporter
	{
		private static readonly Dictionary<string, Func<string, Task<Deck>>> Websites = new Dictionary<string, Func<string, Task<Deck>>>
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
			{"hearthbuilder", Hearthbuilder.Import}
		};

		public static async Task<Deck> Import(string url)
		{
			Logger.WriteLine("Importing deck from " + url, "DeckImporter");

			var website = Websites.FirstOrDefault(x => url.Contains(x.Key));
			if(website.Value != null)
				return await website.Value.Invoke(url);

			Logger.WriteLine("invalid url", "DeckImporter");
			return null;
		}
	}
}