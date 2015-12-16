using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	public static class Hearthstonetopdeck
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);

				var deck = new Deck();
				deck.Name = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("/html/body/div/div[4]/div/div[2]/div/div[1]/h3").InnerText.Trim());

				var cards = doc.DocumentNode.SelectNodes("//div[contains(@class, 'cardname')]/span");

				var deckInfo = doc.DocumentNode.SelectSingleNode("//div[@id='subinfo']").SelectNodes("//span[contains(@class, 'midlarge')]/span");
				if(deckInfo.Count == 2)
				{
					deck.Class = HttpUtility.HtmlDecode(deckInfo[0].InnerText).Trim();

					var decktype = HttpUtility.HtmlDecode(deckInfo[1].InnerText).Trim();
					if(!string.IsNullOrEmpty(decktype) && decktype != "None" && Config.Instance.TagDecksOnImport)
					{
						if(!DeckList.Instance.AllTags.Contains(decktype))
						{
							DeckList.Instance.AllTags.Add(decktype);
							DeckList.Save();
							if(Helper.MainWindow != null)  // to avoid errors when running tests
								Core.MainWindow.ReloadTags();
						}
						deck.Tags.Add(decktype);
					}
				}

				foreach(var cardNode in cards)
				{
					var nameString = HttpUtility.HtmlDecode(cardNode.InnerText);
					var match = Regex.Match(nameString, @"^\s*(\d+)\s+(.*)\s*$");

					if(match.Success)
					{
						var count = match.Groups[1].Value;
						var name = match.Groups[2].Value;

						var card = Database.GetCardFromName(name);
						card.Count = count.Equals("2") ? 2 : 1;
						deck.Cards.Add(card);
						if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
							deck.Class = card.PlayerClass;
					}
				}

				return deck;
			}
			catch (Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}
	}
}
