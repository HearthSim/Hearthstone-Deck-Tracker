#region

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Hearthbuilder
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var deck = new Deck();

				var deckName =
					HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'single-deck-title-wrap')]/h2").InnerText);
				deck.Name = deckName;

				var cardNodes = doc.DocumentNode.SelectNodes("//table[contains(@class, 'cards-table')]/tbody/tr/td[1]/div");

				foreach(var cardNode in cardNodes)
				{
					var nameString = HttpUtility.HtmlDecode(cardNode.InnerText);
					var match = Regex.Match(nameString, @"^\s*(.*)\s*(x 2)?\s*$");

					if(match.Success)
					{
						var name = match.Groups[1].Value;
						var count = match.Groups[2].Value;

						var card = Database.GetCardFromName(name);
						card.Count = String.IsNullOrEmpty(count) ? 1 : 2;
						deck.Cards.Add(card);
						if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
							deck.Class = card.PlayerClass;
					}
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
