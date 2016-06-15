#region

using System;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class EliteHearthstone
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'deck-info')]//h1").InnerText);
				deck.Name = deckName;

				var cardNodes = doc.DocumentNode.SelectNodes("//ul[@class='listado mazo-cartas']/li");

				foreach(var cardNode in cardNodes)
				{
					var count = int.Parse(cardNode.SelectSingleNode(".//span[@class='cantidad']").InnerText);
					var name = HttpUtility.HtmlDecode(cardNode.SelectSingleNode(".//span[@class='nombreCarta']").InnerText);
					var card = Database.GetCardFromName(name);
					card.Count = count;
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
	}
}