#region

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Tempostorm
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				// check url looks correct
				var pattern = "/decks/([^/]+)$";
				var match = Regex.Match(url, pattern);
				// get deck name from url, and post the json request
				if(match.Success && match.Groups.Count == 2)
				{
					var slug = match.Groups[1].ToString();
					var data = await ImportingHelper.PostJson("https://tempostorm.com/deck", "{\"slug\": \"" + slug + "\"}");
					// parse json
					var jsonObject = JsonConvert.DeserializeObject<dynamic>(data);
					if(jsonObject.success.ToObject<bool>())
					{
						var deck = new Deck();

						deck.Name = jsonObject.deck.name.ToString();
						//deck.Class = jsonObject.deck.playerClass.ToString();
						var cards = jsonObject.deck.cards;

						foreach(var item in cards)
						{
							var card = Database.GetCardFromName(item.card.name.ToString());
							card.Count = item.qty.ToString().Equals("2") ? 2 : 1;
							deck.Cards.Add(card);
							if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
								deck.Class = card.PlayerClass;
						}

						return deck;
					}
					throw new Exception("JSON request failed for '" + slug + "'.");
				}
				throw new Exception("The url (" + url + ") is not a vaild TempoStorm deck.");
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}
	}
}