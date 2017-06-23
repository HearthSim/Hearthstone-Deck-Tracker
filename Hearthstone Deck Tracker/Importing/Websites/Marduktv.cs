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
	public static class Marduktv
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);
				var titleNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'header__title internal')]/div[contains(@class,'container')]/h1");
				var cardNodes = doc.DocumentNode.SelectNodes("//ul[contains(@class,'list-unstyled cartas_list')]/li");

				var deck = new Deck();
				deck.Name = HttpUtility.HtmlDecode(titleNode.ChildNodes.FirstOrDefault(x => x.Name == "#text").InnerText);
				foreach (var node in cardNodes)
				{
					var nameNode = node.SelectSingleNode("span[contains(@class,'cartas__name')]/a");
					var countNode = node.SelectSingleNode("span[contains(@class,'cartas__qtd')]");
					var validChild = countNode?.ChildNodes.SingleOrDefault(c => c.Name == "#text");

					var id = nameNode.Attributes.FirstOrDefault(a => a.Name == "data-hcfw-card-id").Value;
					var count = validChild != null ? int.Parse(countNode.InnerText) : 1;

					var card = Database.GetCardFromId(id);
					card.Count = count;
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
