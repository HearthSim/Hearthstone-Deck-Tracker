#region

using System;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Disguisedtoast
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);

				var deck = new Deck();

				var deckName =
					HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//div[@class='dt-well-header']/h2").InnerText);
				deck.Name = deckName;


				var cardNodes = doc.DocumentNode.SelectNodes("//ul[contains(@class,'dt-cardlist')]/li");

				foreach(var cardNode in cardNodes)
				{
					var name = HttpUtility.HtmlDecode(cardNode.SelectSingleNode(".//div[@class='dt-card-name']").InnerText);
					var count = int.Parse(HttpUtility.HtmlDecode(cardNode.SelectSingleNode(".//div[@class='dt-card-quantity']").InnerText));

					var card = Database.GetCardFromName(name.Replace("\n", ""));
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