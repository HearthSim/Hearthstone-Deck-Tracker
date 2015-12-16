#region

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Hearthhead
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				if(!url.Contains("http://www."))
					url = "http://www." + url.Split('.').Skip(1).Aggregate((c, n) => c + "." + n);

				// don't seem to need to Get with WebBrowser anymore
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='deckguide-name']").InnerText);
				deck.Name = deckName;


				var cardNodes = doc.DocumentNode.SelectNodes("//*[contains(@class,'deckguide-cards-type')]//ul//li");

				foreach(var cardNode in cardNodes)
				{
					var nameRaw = cardNode.SelectSingleNode(".//a").InnerText;
					var name = HttpUtility.HtmlDecode(nameRaw);
					var count = cardNode.InnerText.Remove(0, nameRaw.Length - 1).Contains("2") ? 2 : 1;
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