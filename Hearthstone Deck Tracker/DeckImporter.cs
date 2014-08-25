using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Hearthstone_Deck_Tracker
{
	public static class DeckImporter
	{
		public static async Task<Deck> Import(string url)
		{
			if(url.Contains("hearthstats") || url.Contains("hss.io"))
				return await ImportHearthStats(url);
			if(url.Contains("hearthpwn"))
				return await ImportHearthPwn(url);
			if(url.Contains("hearthhead"))
				return await ImportHearthHead(url);
			if(url.Contains("hearthstoneplayers"))
				return await ImportHearthstonePlayers(url);
			if(url.Contains("tempostorm"))
				return await ImportTempostorm(url);
			if(url.Contains("hearthstonetopdeck"))
				return await ImportHsTopdeck(url);
			if(url.Contains("hearthnews.fr"))
				return await ImportHearthNewsFr(url);
			if(url.Contains("arenavalue"))
				return await ImportArenaValue(url);
			return null;
		}

		private static async Task<Deck> ImportArenaValue(string url)
		{
			try
			{
				var deck = new Deck {Name = "Arena " + DateTime.Now.ToString("dd-MM hh:mm")};

				const string baseUrl = @"http://www.arenavalue.com/deckpopout.php?id=";
				var newUrl = baseUrl + url.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).Last();


				HtmlNodeCollection nodes = null;
				using(var wb = new WebBrowser())
				{
					var done = false;
					wb.Navigate(newUrl + "#" + DateTime.Now.Ticks);
					wb.DocumentCompleted += (sender, args) => done = true;

					while(!done)
						await Task.Delay(50);

					for(var i = 0; i < 20; i++)
					{
						var doc = new HtmlDocument();
						doc.Load(wb.DocumentStream);
						if((nodes = doc.DocumentNode.SelectNodes("//*[@id='deck']/div[@class='deck screenshot']")) != null)
						{
							try
							{
								if(nodes.Sum(x => int.Parse(x.Attributes["data-count"].Value)) == 30)
									break;
							}
							catch
							{
							}
						}
						await Task.Delay(500);
					}
				}

				if(nodes == null)
					return null;

				foreach(var node in nodes)
				{
					var cardId = node.Attributes["data-original"].Value;
					int count;
					int.TryParse(node.Attributes["data-count"].Value, out count);
					var card = Game.GetCardFromId(cardId);
					card.Count = count;
					deck.Cards.Add(card);

					if(string.IsNullOrEmpty(deck.Class) && card.GetPlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.Message + "\n" + e.StackTrace);
				return null;
			}
		}


		private static async Task<Deck> ImportHearthNewsFr(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
				var deck = new Deck();

				var deckName =
					HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'deckName')]").InnerText).Trim();
				deck.Name = deckName;

				var cardNodes = doc.DocumentNode.SelectNodes("//table[@class='deck_card_list']/tbody/tr/td[3]/a");

				foreach(var cardNode in cardNodes)
				{
					var id = cardNode.Attributes["real_id"].Value;
					var count = int.Parse(cardNode.Attributes["nb_card"].Value);

					var card = Game.GetCardFromId(id);
					card.Count = count;

					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.Message + "\n" + e.StackTrace);
				return null;
			}
		}

		private static async Task<Deck> ImportHearthStats(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
				var deck = new Deck();

				var deckName =
					HttpUtility.HtmlDecode(
						doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'page-title')]").FirstChild.InnerText);
				deck.Name = deckName;

				var cardNameNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'name')]");
				var cardCountNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'qty')]");

				var cardNames = cardNameNodes.Select(cardNameNode => HttpUtility.HtmlDecode(cardNameNode.InnerText));
				var cardCosts =
					cardCountNodes.Select(countNode => int.Parse(countNode.InnerText));

				var cardInfo = cardNames.Zip(cardCosts, (n, c) => new {Name = n, Count = c});
				foreach(var info in cardInfo)
				{
					var card = Game.GetCardFromName(info.Name);
					card.Count = info.Count;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.Message + "\n" + e.StackTrace);
				return null;
			}
		}

		private static async Task<Deck> ImportHearthHead(string url)
		{
			try
			{
				var doc = await GetHtmlDocJs(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='deckguide-name']").InnerText);
				deck.Name = deckName;


				var cardNodes = doc.DocumentNode.SelectNodes("//*[contains(@class,'deckguide-cards-type')]//ul//li");

				foreach(var cardNode in cardNodes)
				{
					var nameRaw = cardNode.SelectSingleNode(".//a").InnerText;
					var name = HttpUtility.HtmlDecode(nameRaw);
					var count = cardNode.InnerText.Remove(0, nameRaw.Length - 1).Contains("2") ? 2 : 1;
					var card = Game.GetCardFromName(name);
					card.Count = count;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.Message + "\n" + e.StackTrace);
				return null;
			}
		}

		private static async Task<Deck> ImportHearthstonePlayers(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='deck-list-title']").InnerText);
				deck.Name = deckName;

				var cardNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'deck-list')]/div[contains(@class,'card')]");
				foreach(var cardNode in cardNodes)
				{
					//silly names contain right-single quotation mark
					var cardName = cardNode.SelectSingleNode(".//span[contains(@class, 'card-title')]")
					                       .InnerText.Replace("&#8217", "&#39")
					                       .Replace("&#8216", "&#39");

					var name = HttpUtility.HtmlDecode(cardName);

					//no count there if count == 1
					var countNode = cardNode.SelectSingleNode(".//span[contains(@class, 'card-count')]");
					var count = 1;
					if(countNode != null)
						count = int.Parse(countNode.InnerText);

					var card = Game.GetCardFromName(name);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
					card.Count = count;
					deck.Cards.Add(card);
				}
				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.Message + "\n" + e.StackTrace);
				return null;
			}
		}

		private static async Task<Deck> ImportHearthPwn(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
				var deck = new Deck();

				var deckName =
					HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//header/h2[contains(@class,'t-deck-title')]").InnerText);
				deck.Name = deckName;

				var cardNameNodes =
					doc.DocumentNode.SelectNodes(
						"//td[contains(@class,'col-name')]//a[contains(@href,'/cards/') and contains(@class,'rarity')]");
				var cardCountNodes = doc.DocumentNode.SelectNodes("//td[contains(@class,'col-name')]");

				var cardNames = cardNameNodes.Select(cardNameNode => HttpUtility.HtmlDecode(cardNameNode.InnerText));
				var cardCosts =
					cardCountNodes.Select(countNode => int.Parse(Regex.Match(countNode.LastChild.InnerText, @"\d+").Value));

				var cardInfo = cardNames.Zip(cardCosts, (n, c) => new {Name = n, Count = c});
				foreach(var info in cardInfo)
				{
					var card = Game.GetCardFromName(info.Name);
					card.Count = info.Count;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.Message + "\n" + e.StackTrace);
				return null;
			}
		}

		private static async Task<Deck> ImportTempostorm(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='main']/h1").InnerText);
				deck.Name = deckName;


				var cardNodes = doc.DocumentNode.SelectNodes("//*[@class='card card-over']");

				foreach(var cardNode in cardNodes)
				{
					var nameRaw = cardNode.SelectSingleNode(".//span[@class='card-name']").InnerText;
					var name = HttpUtility.HtmlDecode(nameRaw);
					var count = cardNode.SelectSingleNode(".//div").Attributes[0].Value.EndsWith("2") ? 2 : 1;
					var card = Game.GetCardFromName(name);
					card.Count = count;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.Message + "\n" + e.StackTrace);
				return null;
			}
		}

		private static async Task<Deck> ImportHsTopdeck(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);

				var deck = new Deck();

				var deckName =
					HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='deckname']/h1").InnerText).Split('-')[1].Trim();
				deck.Name = deckName;


				var cardNodes = doc.DocumentNode.SelectNodes("//*[@class='cardname']");

				foreach(var cardNode in cardNodes)
				{
					var text = HttpUtility.HtmlDecode(cardNode.InnerText).Split(' ');
					var count = int.Parse(text[0].Trim());
					var name = string.Join(" ", text.Skip(1));

					var card = Game.GetCardFromName(name);
					card.Count = count;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.Message + "\n" + e.StackTrace);
				return null;
			}
		}

		public static async Task<HtmlDocument> GetHtmlDoc(string url)
		{
			return await GetHtmlDoc(url, null, null);
		}

		public static async Task<HtmlDocument> GetHtmlDoc(string url, string header, string headerValue)
		{
			using(var wc = new WebClient())
			{
				wc.Encoding = Encoding.UTF8;

				if(header != null)
					wc.Headers.Add(header, headerValue);

				var websiteContent = await wc.DownloadStringTaskAsync(new Uri(url));
				using(var reader = new StringReader(websiteContent))
				{
					var doc = new HtmlDocument();
					doc.Load(reader);
					return doc;
				}
			}
		}

		public static async Task<HtmlDocument> GetHtmlDocJs(string url)
		{
			using(var wb = new WebBrowser())
			{
				var done = false;
				var doc = new HtmlDocument();
				//                  avoid cache
				wb.Navigate(url + "?" + DateTime.Now.Ticks);
				wb.DocumentCompleted += (sender, args) => done = true;

				while(!done)
					await Task.Delay(50);
				doc.Load(wb.DocumentStream);
				return doc;
			}
		}

		public static async Task<List<string>> GetPopularDeckLists()
		{
			const string url = @"http://hearthstats.net/decks/public?class=&items=500&sort=num_users&order=desc";

			var doc = await GetHtmlDoc(url);

			var deckUrls = new List<string>();
			foreach(var node in doc.DocumentNode.SelectNodes("//td[contains(@class,'name')]"))
			{
				var hrefAttr = node.ChildNodes[0].Attributes.FirstOrDefault(a => a.Name == "href");
				if(hrefAttr != null)
					deckUrls.Add(@"http://www.hearthstats.net/" + hrefAttr.Value);
			}

			return deckUrls;
		}
	}
}