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
	public static class Hearthstonetopdeck
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var doc = await ImportingHelper.GetHtmlDoc(url);

				var deck = new Deck();
				var dname = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode(
					"//h1[contains(@class, 'panel-title')]").InnerText);
				deck.Name = Regex.Replace(dname, @"\s+", " "); // remove sequence of tabs

				var cards = doc.DocumentNode.SelectNodes("//div[contains(@class, 'cardname')]/span");

				var deckExtra = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'deck_banner_description')]");
				var deckInfo = deckExtra.SelectNodes("//span[contains(@class, 'midlarge')]/span");

				// get class and tags
				if(deckInfo.Count == 3)
				{
					deck.Class = HttpUtility.HtmlDecode(deckInfo[1].InnerText).Trim();

					var decktype = HttpUtility.HtmlDecode(deckInfo[2].InnerText).Trim();
					if(!string.IsNullOrWhiteSpace(decktype)
						&& decktype != "None" && Config.Instance.TagDecksOnImport)
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
				}

				// TODO uncomment for standard/wild tags
				/*
				var deckFormat = deckExtra.SelectSingleNode("//span[contains(@class, 'small')]").InnerText.Trim();
				if(!string.IsNullOrWhiteSpace(deckFormat) && Config.Instance.TagDecksOnImport)
				{
					var format = "Standard";
					if(Regex.IsMatch(deckFormat, @"Format:\s*Wild"))
						format = "Wild";
					if(!DeckList.Instance.AllTags.Contains(format))
					{
						DeckList.Instance.AllTags.Add(format);
						DeckList.Save();
						if(Core.MainWindow != null) // to avoid errors when running tests
							Core.MainWindow.ReloadTags();
					}
					deck.Tags.Add(format);
				}
				*/

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
			catch(Exception e)
			{
				Log.Error(e);
				return null;
			}
		}
	}
}
