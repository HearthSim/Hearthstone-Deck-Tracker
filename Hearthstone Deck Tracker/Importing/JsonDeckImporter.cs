using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Importing
{
	public static class JsonDeckImporter
	{
		public static Deck Import(string json)
		{
			try
			{
				var jsonDeck = JsonConvert.DeserializeObject<JsonDeckObj>(json);
				if(jsonDeck?.CardIds == null)
					return null;
				var deck = new Deck
				{
					Name = jsonDeck.Name,
					Class = jsonDeck.Class,
					Url = jsonDeck.SourceUrl
				};
				foreach(var cardId in jsonDeck.CardIds.GroupBy(x => x))
				{
					var card = Database.GetCardFromId(cardId.Key);
					card.Count = cardId.Count();
					deck.Cards.Add(card);
				}
				return deck;
			}
			catch(JsonReaderException)
			{
				Log.Warn("String is not a valid json object");
				return null;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}

		public class JsonDeckObj
		{
			[JsonProperty("name")]
			public string Name { get; set; }
			[JsonProperty("class")]
			public string Class { get; set; }
			[JsonProperty("card_ids")]
			public string[] CardIds { get; set; }
			[JsonProperty("source_url")]
			public string SourceUrl { get; set; }
		}
	}
}
