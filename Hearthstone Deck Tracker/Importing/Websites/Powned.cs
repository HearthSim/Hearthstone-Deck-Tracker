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
	public static class Powned
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck {Name = doc.DocumentNode.SelectSingleNode("//h1[@id='deck-title']").InnerText};

				var cardList = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//table[@id='deck-guide']").Attributes["data-deck"].Value);
				cardList = cardList.Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\\", "");

				foreach(var cardNode in cardList.Split(',').GroupBy(x => x))
				{
					var card = Database.GetCardFromId(cardNode.Key);
					card.Count = cardNode.Count();
					deck.Cards.Add(card);

					if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

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