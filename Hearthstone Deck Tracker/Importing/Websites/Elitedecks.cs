#region

using System;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Elitedecks
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//h2[contains(@class, 'dname')]").InnerText);
				deck.Name = deckName;

				var cardNodes = doc.DocumentNode.SelectNodes("//ul[@class='vminionslist' or @class='vspellslist']/li");

				foreach(var cardNode in cardNodes)
				{
					var count = int.Parse(cardNode.SelectSingleNode(".//span[@class='cantidad']").InnerText);
					var name =
						HttpUtility.HtmlDecode(
						                       cardNode.SelectSingleNode(
						                                                 ".//span[@class='nombreCarta rarity_legendary' or @class='nombreCarta rarity_epic' or @class='nombreCarta rarity_rare' or @class='nombreCarta rarity_common' or @class='nombreCarta rarity_basic']")
						                               .InnerText);
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
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}
	}
}