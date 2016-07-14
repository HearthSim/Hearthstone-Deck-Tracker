#region

using System;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HtmlAgilityPack;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Manacrystals
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck();

				var deckRoot = doc.DocumentNode.SelectSingleNode("//div[@class='decklist-meta-data']");
				var deckInfo = deckRoot.SelectNodes(".//div[@class='row']/div");
				if(deckInfo.Count != 3)
				{
					Log.Error("Wrong number of columns.");
					return null;
				}

				// get metadata
				deck.Name = HttpUtility.HtmlDecode(deckRoot.SelectSingleNode(".//h2/a").InnerText).Trim();							
				var deckType = HttpUtility.HtmlDecode(deckInfo[0].SelectNodes(".//p")[2].InnerText).Trim();
				if(!string.IsNullOrWhiteSpace(deckType) && Config.Instance.TagDecksOnImport)
				{
					if(!DeckList.Instance.AllTags.Contains(deckType))
					{
						DeckList.Instance.AllTags.Add(deckType);
						DeckList.Save();
						if(Core.MainWindow != null) // to avoid errors when running tests
							Core.MainWindow.ReloadTags();
					}
					deck.Tags.Add(deckType);
				}
				// get cards
				CardNodes(deckInfo[1], deck);
				CardNodes(deckInfo[2], deck);

				return deck;
			}
			catch (Exception e)
			{
				Log.Error(e);
				return null;
			}
		}

		private static void CardNodes(HtmlNode node, Deck deck)
		{
			var cardNodes = node.SelectNodes(".//ul/li");
			foreach (var cardNode in cardNodes)
			{
				var count = HttpUtility.HtmlDecode(
					cardNode.SelectSingleNode(".//div[contains(@class,'quantity')]").InnerText.Trim());
				var name = HttpUtility.HtmlDecode(
					cardNode.SelectSingleNode(".//div[contains(@class, 'card-name')]").InnerText.Trim());
				var card = Database.GetCardFromName(name);
				card.Count = count == "2" ? 2 : 1;
				deck.Cards.Add(card);
				if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
					deck.Class = card.PlayerClass;
			}
		}
	}
}