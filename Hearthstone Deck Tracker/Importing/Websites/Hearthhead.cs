using System;
using System.Threading.Tasks;
using System.Web;
using HearthDb.Deckstrings;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;
using Deck = Hearthstone_Deck_Tracker.Hearthstone.Deck;

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public class Hearthhead
	{
		public static async Task<Deck> Import(string url)
		{
			return await ImportHearthHead(url);
		}

		private static async Task<Deck> ImportHearthHead(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);

				var windowDataScript = HttpUtility.HtmlDecode(
					doc.DocumentNode.SelectSingleNode("//section[@id='main-content']/script").InnerText);

				var json = windowDataScript.Substring(windowDataScript.IndexOf("{", StringComparison.Ordinal)).TrimEnd().TrimEnd(';');
				var deckInfo = JsonConvert.DeserializeObject<dynamic>(json);
				var deckName = deckInfo.deck.name.Value;
				var deckString = deckInfo.deck.deck_string.Value;
				var hearthDbDeck = DeckSerializer.Deserialize(deckString);
				var deck = HearthDbConverter.FromHearthDbDeck(hearthDbDeck);
				deck.Name = deckName;
				deck.Url = url;
				return deck;
			}
			catch (Exception e)
			{
				Log.Error(e);
				return null;
			}
		}
	}
}
