#region

using System;
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
				var deck = new Deck();

				var deckName = doc.DocumentNode.SelectSingleNode("//h1[@id='deck-title']").InnerText;
				deck.Name = deckName;


				var cardList = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//table[@id='deck-guide']").Attributes["data-deck"].Value);
				cardList = cardList.Replace("\"", "").Replace("[", "").Replace("]", "").Replace("\\", "");

				string[] cardList_exploded = cardList.Split(',');
				Array.Sort(cardList_exploded);

				Card previous_card = null;

				foreach (var cardNode in cardList_exploded)
				{
					var card = Database.GetCardFromId(cardNode);
					card.Count = 1;

					if (card.Equals(previous_card))
					{
						deck.Cards.Remove(previous_card);
						card.Count = 2;
					}
					deck.Cards.Add(card);
					previous_card = card;
					
					// Set class
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