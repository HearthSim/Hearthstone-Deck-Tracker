#region

using System;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class HearthnewsFr
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'deckName')]").InnerText).Trim();
				deck.Name = deckName;

				var cardNodes = doc.DocumentNode.SelectNodes("//table[@class='deck_card_list']/tbody/tr/td/a[@class='real_id']");

				foreach(var cardNode in cardNodes)
				{
					var id = cardNode.Attributes["real_id"].Value;
					var count = int.Parse(cardNode.Attributes["nb_card"].Value);

					var card = Database.GetCardFromId(id);
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