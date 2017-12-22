#region

using System;
using System.Net;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Newtonsoft.Json;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Icyveins
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				string json;
				using(var wc = new WebClient())
					json = await wc.DownloadStringTaskAsync(url + ".json");
				var wrapper = JsonConvert.DeserializeObject<IcyVeinsWrapper>(json);
				var deck = new Deck {Name = wrapper.deck_name};
				foreach(var cardObj in wrapper.deck_cards)
				{
					var cardName = cardObj.name;
					if(cardName.EndsWith(" Naxx"))
						cardName = cardName.Replace(" Naxx", "");
					if(cardName.EndsWith(" GvG"))
						cardName = cardName.Replace(" GvG", "");
					if(cardName.EndsWith(" BrM"))
						cardName = cardName.Replace(" BrM", "");
					var card = Database.GetCardFromName(cardName);
					card.Count = cardObj.quantity;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}
				return deck;
			}
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		private class IcyVeinsWrapper
		{
#pragma warning disable 649
			public IcyVeinsCardObj[] deck_cards;
			public string deck_name;

			public class IcyVeinsCardObj
			{
				public string name;
				public int quantity;
			}
#pragma warning restore 649
		}
	}
}
