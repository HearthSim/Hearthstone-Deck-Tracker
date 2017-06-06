#region

using System;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Hearthstoneplayers
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='deck-list-title']").InnerText);
				deck.Name = deckName;

				var cardNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'guide-deck-list')]/div/div[contains(@class,'card')]");
				foreach(var cardNode in cardNodes)
				{
					//silly names contain right-single quotation mark
					var cardName =
						cardNode.SelectSingleNode(".//span[contains(@class, 'card-title')]")
						        .InnerText.Replace("&#8217", "&#39")
						        .Replace("&#8216", "&#39");

					var name = HttpUtility.HtmlDecode(cardName);

					//no count there if count == 1
					var countNode = cardNode.SelectSingleNode(".//span[contains(@class, 'card-count')]");
					var count = 1;
					if(countNode != null)
						count = int.Parse(countNode.InnerText);

					var card = Database.GetCardFromName(name);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
					card.Count = count;
					deck.Cards.Add(card);
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
