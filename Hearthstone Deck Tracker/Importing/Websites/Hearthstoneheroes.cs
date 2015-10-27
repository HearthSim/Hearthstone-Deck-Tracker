#region

using System;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Hearthstoneheroes
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDocGzip(url);
				var deck = new Deck
				{
					Name =
						HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//header[@class='panel-heading']/h1[@class='panel-title']").InnerText)
						           .Trim()
				};
				var nodes = doc.DocumentNode.SelectNodes("//table[@class='table table-bordered table-hover table-db']/tbody/tr");

				foreach(var cardNode in nodes)
				{
					var name = HttpUtility.HtmlDecode(cardNode.SelectSingleNode(".//a").Attributes[3].Value);

					var temp = HttpUtility.HtmlDecode(cardNode.SelectSingleNode(".//a/small").InnerText[0].ToString());
					var count = int.Parse(temp);

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