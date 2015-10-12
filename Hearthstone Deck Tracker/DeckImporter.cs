﻿#region

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
using Newtonsoft.Json;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

#endregion

namespace Hearthstone_Deck_Tracker
{
	public static class DeckImporter
	{
		public static async Task<Deck> Import(string url)
		{
			Logger.WriteLine("Importing deck from " + url, "DeckImporter");
			if(url.Contains("hearthstats") || url.Contains("hss.io"))
				return await ImportHearthStats(url);
			if (url.Contains("hearthpwn"))
			{
				if (url.Contains("deckbuilder"))
					return await ImportHearthPwnDeckBuilder(url);
				else
					return await ImportHearthPwn(url);
			}
			if(url.Contains("hearthhead"))
				return await ImportHearthHead(url);
			if(url.Contains("hearthstoneplayers"))
				return await ImportHearthstonePlayers(url);
			if(url.Contains("tempostorm"))
				return await ImportTempostorm(url);
			if(url.Contains("hearthstonetopdecks"))
				return await ImportHsTopdeck_s(url);
			if(url.Contains("hearthstonetopdeck."))
				return await ImportHsTopdeck(url);
			if(url.Contains("hearthnews.fr"))
				return await ImportHearthNewsFr(url);
			if(url.Contains("arenavalue"))
				return await ImportArenaValue(url);
			if(url.Contains("hearthstone-decks"))
				return await ImportHearthstoneDecks(url);
			if(url.Contains("heartharena"))
				return await ImportHearthArena(url);
			if(url.Contains("hearthstoneheroes"))
				return await ImportHearthstoneheroes(url);
			if(url.Contains("elitedecks"))
				return await ImportEliteDecks(url);
			if(url.Contains("icy-veins"))
				return await ImportIcyVeins(url);
			if(url.Contains("hearthbuilder"))
				return await ImportHearthBuilder(url);
			Logger.WriteLine("invalid url", "DeckImporter");
			return null;
		}

		private static async Task<Deck> ImportIcyVeins(string url)
		{
			try
			{
				string json;
				using(var wc = new WebClient())
					json = await wc.DownloadStringTaskAsync(url + ".json");
				var wrapper = JsonConvert.DeserializeObject<IcyVeinsWrapper>(json);
				var deck = new Deck {Name = wrapper.deck_name};
				foreach(var cardObj in wrapper.deck_cards)
				{
					var cardName = cardObj.name;
					if(cardName.EndsWith(" Naxx"))
						cardName = cardName.Replace(" Naxx", "");
					if(cardName.EndsWith(" GvG"))
						cardName = cardName.Replace(" GvG", "");
					if(cardName.EndsWith(" BrM"))
						cardName = cardName.Replace(" BrM", "");
					var card = Database.GetCardFromName(cardName);
					card.Count = cardObj.quantity;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}
				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		private static async Task<Deck> ImportHsTopdeck(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);

				var deck = new Deck();
				deck.Name = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("/html/body/div/div[4]/div/div[2]/div/div[1]/h3").InnerText.Trim());

				var cards = doc.DocumentNode.SelectNodes("//div[contains(@class, 'cardname')]/span");

				var deckInfo = doc.DocumentNode.SelectSingleNode("//div[@id='subinfo']").SelectNodes("//span[contains(@class, 'midlarge')]/span");
				if (deckInfo.Count == 2)
				{
					deck.Class = HttpUtility.HtmlDecode(deckInfo[0].InnerText).Trim();

					var decktype = HttpUtility.HtmlDecode(deckInfo[1].InnerText).Trim();
					if(!String.IsNullOrEmpty(decktype) && decktype != "None" && Config.Instance.TagDecksOnImport)
					{
						if(!DeckList.Instance.AllTags.Contains(decktype))
						{
							DeckList.Instance.AllTags.Add(decktype);
							DeckList.Save();
							if(Helper.MainWindow != null) // to avoid errors when running tests
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
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		private static async Task<Deck> ImportHearthArena(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
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

		private static async Task<Deck> ImportHearthstoneDecks(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
				var deck = new Deck
				{
					Name =
						HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='content']/div[contains(@class, 'deck')]/h1").InnerText).Trim()
				};

				var nodes = doc.DocumentNode.SelectNodes("//a[@real_id]");

				foreach(var cardNode in nodes)
				{
					var id = cardNode.Attributes["real_id"].Value;
					var count = int.Parse(cardNode.Attributes["nb_card"].Value);

					var card = Database.GetCardFromId(id);
					card.Count = count;

					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		private static async Task<Deck> ImportArenaValue(string url)
		{
			try
			{
				var deck = new Deck {Name = Helper.ParseDeckNameTemplate(Config.Instance.ArenaDeckNameTemplate), IsArenaDeck = true};

				const string baseUrl = @"http://www.arenavalue.com/deckpopout.php?id=";
				var newUrl = baseUrl + url.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).Last();


				HtmlNodeCollection nodes = null;
				using(var wb = new WebBrowser())
				{
					var done = false;
					wb.ScriptErrorsSuppressed = true;
					wb.Navigate(newUrl + "#" + DateTime.Now.Ticks);
					wb.DocumentCompleted += (sender, args) => done = true;

					while(!done)
						await Task.Delay(50);

					for(var i = 0; i < 20; i++)
					{
						var doc = new HtmlDocument();
						doc.Load(wb.DocumentStream);
						if((nodes = doc.DocumentNode.SelectNodes("//*[@id='deck']/div[contains(@class, 'screenshot')]")) != null)
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
					int count;
					int.TryParse(node.Attributes["data-count"].Value, out count);

					var text = HttpUtility.HtmlDecode(node.InnerText).Trim();

					var pattern = @"^\d+\s*(.+?)\s*(x \d+)?$";
					var match = Regex.Match(text, pattern);

					var name = "";
					if(match.Success && match.Groups.Count == 3)
					{
						name = match.Groups[1].ToString();
					}
					if(String.IsNullOrWhiteSpace(name))
						continue;

					var card = Database.GetCardFromName(name);
					card.Count = count;
					deck.Cards.Add(card);

					if(string.IsNullOrEmpty(deck.Class) && card.GetPlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}
				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		private static async Task<Deck> ImportHearthNewsFr(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//span[contains(@class, 'deckName')]").InnerText).Trim();
				deck.Name = deckName;

				var cardNodes = doc.DocumentNode.SelectNodes("//table[@class='deck_card_list']/tbody/tr/td/a[@class='real_id']");

				foreach(var cardNode in cardNodes)
				{
					var id = cardNode.Attributes["real_id"].Value;
					var count = int.Parse(cardNode.Attributes["nb_card"].Value);

					var card = Database.GetCardFromId(id);
					card.Count = count;

					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		private static async Task<Deck> ImportHearthStats(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'page-title')]").FirstChild.InnerText);
				deck.Name = deckName;

				var cardNameNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'name')]");
				var cardCountNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'qty')]");

				var cardNames = cardNameNodes.Select(cardNameNode => HttpUtility.HtmlDecode(cardNameNode.InnerText));
				var cardCosts = cardCountNodes.Select(countNode => int.Parse(countNode.InnerText));

				var cardInfo = cardNames.Zip(cardCosts, (n, c) => new {Name = n, Count = c});
				foreach(var info in cardInfo)
				{
					var card = Database.GetCardFromName(info.Name);
					card.Count = info.Count;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		private static async Task<Deck> ImportHearthHead(string url)
		{
			try
			{
				if(!url.Contains("http://www."))
					url = "http://www." + url.Split('.').Skip(1).Aggregate((c, n) => c + "." + n);

				// don't seem to need to Get with WebBrowser anymore
				var doc = await GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='deckguide-name']").InnerText);
				deck.Name = deckName;


				var cardNodes = doc.DocumentNode.SelectNodes("//*[contains(@class,'deckguide-cards-type')]//ul//li");

				foreach(var cardNode in cardNodes)
				{
					var nameRaw = cardNode.SelectSingleNode(".//a").InnerText;
					var name = HttpUtility.HtmlDecode(nameRaw);
					var count = cardNode.InnerText.Remove(0, nameRaw.Length - 1).Contains("2") ? 2 : 1;
					var card = Database.GetCardFromName(name);
					card.Count = count;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
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

				var cardNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'guide-deck-list')]/div/div[contains(@class,'card')]");
				foreach(var cardNode in cardNodes)
				{
					//silly names contain right-single quotation mark
					var cardName =
						cardNode.SelectSingleNode(".//span[contains(@class, 'card-title')]")
						        .InnerText.Replace("&#8217", "&#39")
						        .Replace("&#8216", "&#39");

					var name = HttpUtility.HtmlDecode(cardName);

					//no count there if count == 1
					var countNode = cardNode.SelectSingleNode(".//span[contains(@class, 'card-count')]");
					var count = 1;
					if(countNode != null)
						count = int.Parse(countNode.InnerText);

					var card = Database.GetCardFromName(name);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
					card.Count = count;
					deck.Cards.Add(card);
				}
				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}
		
		private static async Task<Deck> ImportHearthPwnDeckBuilder(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//div[contains(@class,'deck-name-container')]/h2").InnerText);
				deck.Name = deckName;

				var cardNodes = doc.DocumentNode.SelectNodes("//tr[contains(@class,'deck-card-link')]");

				/* <tr class="deck-card-link odd" data-tooltip-href="//www.hearthpwn.com/cards/22385-power-word-glory" 
				 *     data-description="Choose a minion. Whenever it attacks, restore 4 Health to your hero." data-race=""
				 *     data-rarity="1" data-class="6" data-cost="1" data-hp="0" data-attack="0"
				 *     data-image="http://media-Hearth.cursecdn.com/avatars/252/489/22385.png" data-type="5" data-id="22385"
				 *     data-name="Power Word: Glory" data-mechanics="">
				 */
				Dictionary<int, String> cardDatabase = new Dictionary<int, String>();
				foreach (var cardtr in cardNodes)
				{
					var cardId = cardtr.GetAttributeValue("data-id", -1);
					var cardName = HttpUtility.HtmlDecode(cardtr.GetAttributeValue("data-name", ""));
					cardDatabase[cardId] = cardName;
				}

				// http://www.hearthpwn.com/deckbuilder/priest#38:1;117:2;207:2;212:2;346:2;395:2;409:2;415:2;431:2;435:2;544:1;554:2;600:2;7750:2;7753:2;7755:2;
				var cardMatches = Regex.Matches(url, @"(\d+):(\d+)");

				foreach (Match cardMatch in cardMatches)
				{
					var cardId = int.Parse(cardMatch.Groups[1].Value);
					var cardCount = int.Parse(cardMatch.Groups[2].Value);
					var cardName = cardDatabase[cardId];

					var card = Database.GetCardFromName(cardName);
					card.Count = cardCount;
					deck.Cards.Add(card);
					if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch (Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		private static async Task<Deck> ImportHearthPwn(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//section[contains(@class,'deck-info')]/h2[contains(@class,'deck-title')]").InnerText);
				deck.Name = deckName;

				var cardNameNodes =
					doc.DocumentNode.SelectNodes("//td[contains(@class,'col-name')]//a[contains(@href,'/cards/') and contains(@class,'rarity')]");
				var cardCountNodes = doc.DocumentNode.SelectNodes("//td[contains(@class,'col-name')]");
				//<span class="deck-type">Midrange</span>
				var decktype = doc.DocumentNode.SelectSingleNode("//span[contains(@class,'deck-type')]").InnerText;
				if(decktype != "None" && Config.Instance.TagDecksOnImport)
				{
					if(!DeckList.Instance.AllTags.Contains(decktype))
					{
						DeckList.Instance.AllTags.Add(decktype);
						DeckList.Save();
						if (Helper.MainWindow != null) // to avoid errors when running tests
							Core.MainWindow.ReloadTags();
					}
					deck.Tags.Add(decktype);
				}


				var cardNames = cardNameNodes.Select(cardNameNode => HttpUtility.HtmlDecode(cardNameNode.InnerText));
				var cardCosts = cardCountNodes.Select(countNode => int.Parse(Regex.Match(countNode.LastChild.InnerText, @"\d+").Value));

				var cardInfo = cardNames.Zip(cardCosts, (n, c) => new {Name = n, Count = c});
				foreach(var info in cardInfo)
				{
					var card = Database.GetCardFromName(info.Name);
					card.Count = info.Count;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		private static async Task<Deck> ImportTempostorm(string url)
		{
			try
			{
				// check url looks correct
				var pattern = "/decks/([^/]+)$";
				var match = Regex.Match(url, pattern);
				// get deck name from url, and post the json request
				if(match.Success && match.Groups.Count == 2)
				{
					var slug = match.Groups[1].ToString();
					var data = await PostJson("https://tempostorm.com/deck", "{\"slug\": \"" + slug + "\"}");
					// parse json
					var jsonObject = JsonConvert.DeserializeObject<dynamic>(data);
					if(jsonObject.success.ToObject<Boolean>())
					{
						var deck = new Deck();

						deck.Name = jsonObject.deck.name.ToString();
						//deck.Class = jsonObject.deck.playerClass.ToString();
						var cards = jsonObject.deck.cards;

						foreach(var item in cards)
						{
							var card = Database.GetCardFromName(item.card.name.ToString());
							card.Count = item.qty.ToString().Equals("2") ? 2 : 1;
							deck.Cards.Add(card);
							if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
								deck.Class = card.PlayerClass;
						}

						return deck;
					}
					throw new Exception("JSON request failed for '" + slug + "'.");
				}
				throw new Exception("The url (" + url + ") is not a vaild TempoStorm deck.");
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		private static async Task<Deck> ImportHearthstoneheroes(string url)
		{
			try
			{
				var doc = await GetHtmlDocGzip(url);
				var deck = new Deck
				{
					Name =
						HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//header[@class='panel-heading']/h1[@class='panel-title']").InnerText)
						           .Trim()
				};
				var nodes = doc.DocumentNode.SelectNodes("//table[@class='table table-bordered table-hover table-db']/tbody/tr");

				foreach(var cardNode in nodes)
				{
					var name = HttpUtility.HtmlDecode(cardNode.SelectSingleNode(".//a").Attributes[3].Value);

					var temp = HttpUtility.HtmlDecode(cardNode.SelectSingleNode(".//a/small").InnerText[0].ToString());
					var count = int.Parse(temp);

					var card = Database.GetCardFromName(name);
					card.Count = count;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		private static async Task<Deck> ImportHsTopdeck_s(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);

				var deck = new Deck();

				var deckName =
					HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//header[@class='entry-header']/h1[@class='entry-title']").InnerText);
				deck.Name = deckName;


				var cardNodes = doc.DocumentNode.SelectNodes("//ul[contains(@class,'deck-class')]/li");

				foreach(var cardNode in cardNodes)
				{
					var name = HttpUtility.HtmlDecode(cardNode.SelectSingleNode(".//a/span[@class='card-name']").InnerText);
					var count = int.Parse(HttpUtility.HtmlDecode(cardNode.SelectSingleNode(".//a/span[@class='card-count']").InnerText));

					var card = Database.GetCardFromName(name);
					card.Count = count;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		private static async Task<Deck> ImportEliteDecks(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
				var deck = new Deck();

				var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//h2[contains(@class, 'dname')]").InnerText);
				deck.Name = deckName;

				var cardNodes = doc.DocumentNode.SelectNodes("//ul[@class='vminionslist' or @class='vspellslist']/li");

				foreach(var cardNode in cardNodes)
				{
					var count = int.Parse(cardNode.SelectSingleNode(".//span[@class='cantidad']").InnerText);
					var name =
						HttpUtility.HtmlDecode(
						                       cardNode.SelectSingleNode(
						                                                 ".//span[@class='nombreCarta rarity_legendary' or @class='nombreCarta rarity_epic' or @class='nombreCarta rarity_rare' or @class='nombreCarta rarity_common' or @class='nombreCarta rarity_basic']")
						                               .InnerText);
					var card = Database.GetCardFromName(name);
					card.Count = count;
					deck.Cards.Add(card);
					if(string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}

				return deck;
			}
			catch(Exception e)
			{
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		private static async Task<Deck> ImportHearthBuilder(string url)
		{
			try
			{
				var doc = await GetHtmlDoc(url);
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
				Logger.WriteLine(e.ToString(), "DeckImporter");
				return null;
			}
		}

		public static async Task<HtmlDocument> GetHtmlDoc(string url)
		{
			return await GetHtmlDoc(url, null, null);
		}

		public static async Task<HtmlDocument> GetHtmlDocGzip(string url)
		{
			using(var wc = new GzipWebClient())
			{
				wc.Encoding = Encoding.UTF8;
				// add an user-agent to stop some 403's
				wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0");
				//if(header != null)
				//	wc.Headers.Add(header, headerValue);

				var websiteContent = await wc.DownloadStringTaskAsync(new Uri(url));
				using(var reader = new StringReader(websiteContent))
				{
					var doc = new HtmlDocument();
					doc.Load(reader);
					return doc;
				}
			}
		}

		public static async Task<HtmlDocument> GetHtmlDoc(string url, string header, string headerValue)
		{
			using(var wc = new WebClient())
			{
				wc.Encoding = Encoding.UTF8;
				// add an user-agent to stop some 403's
				wc.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.3; rv:36.0) Gecko/20100101 Firefox/36.0");
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

		public static async Task<string> PostJson(string url, string jsonData)
		{
			using(var wc = new WebClient())
			{
				wc.Encoding = Encoding.UTF8;
				wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");

				var response = await wc.UploadStringTaskAsync(new Uri(url), jsonData);

				return response;
			}
		}

		public static async Task<HtmlDocument> GetHtmlDocJs(string url)
		{
			using(var wb = new WebBrowser())
			{
				var done = false;				
				var doc = new HtmlDocument();
				wb.ScriptErrorsSuppressed = true;
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

		private class IcyVeinsWrapper
		{
			public IcyVeinsCardObj[] deck_cards;
			public string deck_name;

			public class IcyVeinsCardObj
			{
				public string name;
				public int quantity;
			}
		}

		// To handle GZipped Web Content
		// http://stackoverflow.com/a/4567408/2762059
		private class GzipWebClient : WebClient
		{
			protected override WebRequest GetWebRequest(Uri address)
			{
				HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
				request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
				return request;
			}
		}
	}
}
