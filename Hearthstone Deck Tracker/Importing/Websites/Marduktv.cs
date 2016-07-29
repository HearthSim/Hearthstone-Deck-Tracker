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
	public static class Marduktv
    {
		public static async Task<Deck> Import(string url)
		{
			return await ImportMarduktv(url);
		}

		private static async Task<Deck> ImportMarduktv(string url)
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
	}
}