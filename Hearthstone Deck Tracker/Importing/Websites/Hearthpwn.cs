#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Hearthpwn
	{
		public static async Task<Deck> Import(string url)
		{
			if(url.Contains("deckbuilder"))
				return await ImportHearthPwnDeckBuilder(url);
			return await ImportHearthPwn(url);
		}

		private static async Task<Deck> ImportHearthPwn(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck();

				var deckName =
					HttpUtility.HtmlDecode(
					                       doc.DocumentNode.SelectSingleNode(
					                                                         "//section[contains(@class,'deck-info')]/h2[contains(@class,'deck-title')]")
					                          .InnerText);
				deck.Name = deckName;

				var cardNameNodes =
					doc.DocumentNode.SelectNodes("//td[contains(@class,'col-name')]//a[contains(@href,'/cards/') and contains(@class,'rarity')]");
				//<span class="deck-type">Midrange</span>
				var decktype = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'deck-type')]").InnerText;
				if(decktype != "None" && Config.Instance.TagDecksOnImport)
				{
					if(!DeckList.Instance.AllTags.Contains(decktype))
					{
						DeckList.Instance.AllTags.Add(decktype);
						DeckList.Save();
						if(Core.MainWindow != null) // to avoid errors when running tests
							Core.MainWindow.ReloadTags();
					}
					deck.Tags.Add(decktype);
				}


				var cardNames = cardNameNodes.Select(cardNameNode => HttpUtility.HtmlDecode(cardNameNode.InnerText));
				var cardCosts = cardNameNodes.Select(cardNameNode => int.Parse(cardNameNode.Attributes["data-Count"].Value));

				var cardInfo = cardNames.Zip(cardCosts, (n, c) => new {Name = n, Count = c});
				foreach(var info in cardInfo)
				{
					var card = Database.GetCardFromName(info.Name.Trim());
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

		private static async Task<Deck> ImportHearthPwnDeckBuilder(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck();

				var deckName =
					HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//div[contains(@class,'deck-name-container')]/h2").InnerText);
				deck.Name = deckName;

				var cardNodes = doc.DocumentNode.SelectNodes("//tr[contains(@class,'deck-card-link')]");

				/* <tr class="deck-card-link odd" data-tooltip-href="//www.hearthpwn.com/cards/22385-power-word-glory" 
				 *     data-description="Choose a minion. Whenever it attacks, restore 4 Health to your hero." data-race=""
				 *     data-rarity="1" data-class="6" data-cost="1" data-hp="0" data-attack="0"
				 *     data-image="http://media-Hearth.cursecdn.com/avatars/252/489/22385.png" data-type="5" data-id="22385"
				 *     data-name="Power Word: Glory" data-mechanics="">
				 */
				Dictionary<int, string> cardDatabase = new Dictionary<int, string>();
				foreach(var cardtr in cardNodes)
				{
					var cardId = cardtr.GetAttributeValue("data-id", -1);
					var cardName = HttpUtility.HtmlDecode(cardtr.GetAttributeValue("data-name", ""));
					cardDatabase[cardId] = cardName;
				}

				// http://www.hearthpwn.com/deckbuilder/priest#38:1;117:2;207:2;212:2;346:2;395:2;409:2;415:2;431:2;435:2;544:1;554:2;600:2;7750:2;7753:2;7755:2;
				var cardMatches = Regex.Matches(url, @"(\d+):(\d+)");

				foreach(Match cardMatch in cardMatches)
				{
					var cardId = int.Parse(cardMatch.Groups[1].Value);
					var cardCount = int.Parse(cardMatch.Groups[2].Value);
					var cardName = cardDatabase[cardId];

					var card = Database.GetCardFromName(cardName);
					card.Count = cardCount;
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
