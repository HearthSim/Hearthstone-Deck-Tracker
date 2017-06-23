#region

using System;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Hearthstonedecks
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck
				{
					Name =
						HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='content']/div[contains(@class, 'deck')]/h1").InnerText).Trim()
				};

				var nodes = doc.DocumentNode.SelectNodes("//a[@real_id]");

				foreach(var cardNode in nodes)
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
				Log.Error(e);
				return null;
			}
		}
	}
}
