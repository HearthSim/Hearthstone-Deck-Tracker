#region

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Hearthstats
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'page-title')]").FirstChild.InnerText);
				deck.Name = deckName;

				var cardNameNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'name')]");
				var cardCountNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'qty')]");

				var cardNames = cardNameNodes.Select(cardNameNode => HttpUtility.HtmlDecode(cardNameNode.InnerText));
				var cardCosts = cardCountNodes.Select(countNode => int.Parse(countNode.InnerText));

				var cardInfo = cardNames.Zip(cardCosts, (n, c) => new {Name = n, Count = c});
				foreach(var info in cardInfo)
				{
					var card = Database.GetCardFromName(info.Name);
					card.Count = info.Count;
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