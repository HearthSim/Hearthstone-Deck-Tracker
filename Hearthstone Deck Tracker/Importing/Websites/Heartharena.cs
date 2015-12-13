#region

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Heartharena
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck {Name = Helper.ParseDeckNameTemplate(Config.Instance.ArenaDeckNameTemplate), IsArenaDeck = true};

				var cardNodes = doc.DocumentNode.SelectSingleNode(".//ul[@class='deckList']");
				var nameNodes = cardNodes.SelectNodes(".//span[@class='name']");
				var countNodes = cardNodes.SelectNodes(".//span[@class='quantity']");
				var numberOfCards = nameNodes.Count;
				for(var i = 0; i < numberOfCards; i++)
				{
					var nameRaw = nameNodes.ElementAt(i).InnerText;
					var name = HttpUtility.HtmlDecode(nameRaw);
					var card = Database.GetCardFromName(name);
					card.Count = int.Parse(countNodes.ElementAt(i).InnerText);
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}
				if(DeckList.Instance.AllTags.Contains("Arena"))
					deck.Tags.Add("Arena");
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