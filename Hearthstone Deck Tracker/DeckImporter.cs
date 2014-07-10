using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

namespace Hearthstone_Deck_Tracker
{
    public class DeckImporter
    {
        private Hearthstone _hearthstone;
        public DeckImporter(Hearthstone hearthstone)
        {
            _hearthstone = hearthstone;
        }

        public async Task<Deck> Import(string url)
        {
            if (url.Contains("hearthstats") || url.Contains("hss.io"))
            {
                return await ImportHearthStats(url);
            }
            if (url.Contains("hearthpwn"))
            {
                return await ImportHearthPwn(url);
            }
            if (url.Contains("hearthhead"))
            {
                return await ImportHearthHead(url);
            }
            if (url.Contains("hearthstoneplayers"))
            {
                return await ImportHearthstonePlayers(url);
            }
            if (url.Contains("tempostorm"))
            {
                return await ImportTempostorm(url);
            }
            if (url.Contains("hearthstonetopdeck"))
            {
                return await ImportHsTopdeck(url);
            }
            return null;
        }

        private async Task<Deck> ImportHearthStats(string url)
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
                var cardCosts =
                    cardCountNodes.Select(countNode => int.Parse(countNode.InnerText));

                var cardInfo = cardNames.Zip(cardCosts, (n, c) => new { Name = n, Count = c });
                foreach (var info in cardInfo)
                {
                    var card = _hearthstone.GetCardFromName(info.Name);
                    card.Count = info.Count;
                    deck.Cards.Add(card);
                    if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
                    {
                        deck.Class = card.PlayerClass;
                    }
                }

                return deck;
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.Message + "\n" + e.StackTrace);
                return null;
            }
        }

        private async Task<Deck> ImportHearthHead(string url)
        {
            //problems with cache (?), can only download each deck one. webclient cache options dont help, neither do "url extentions" like &randomnumber
            try
            {
                var doc = await GetHtmlDoc(url);

                var deck = new Deck();
                
                var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='main-contents']/div[3]/h1").InnerText);
                deck.Name = deckName;


                var cardNodes = doc.DocumentNode.SelectNodes("//*[contains(@class,'deckguide-cards-type')]//ul//li");

                foreach (var cardNode in cardNodes)
                {
                    var nameRaw = cardNode.SelectSingleNode(".//a").InnerText;
                    var name = HttpUtility.HtmlDecode(nameRaw);
                    var count = cardNode.InnerText.Remove(0, nameRaw.Length - 1).Contains("2")? 2 : 1;
                    var card = _hearthstone.GetCardFromName(name);
                    card.Count = count;
                    deck.Cards.Add(card);
                    if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
                    {
                        deck.Class = card.PlayerClass;
                    }
                }

                return deck;
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.Message + "\n" + e.StackTrace);
                return null;
            }
        }

        private async Task<Deck> ImportHearthstonePlayers(string url)
        {
            try
            {
                var doc = await GetHtmlDoc(url);

                var deck = new Deck();

                var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='deck-list-title']").InnerText);
                deck.Name = deckName;

                var cardNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'deck-list')]/div[contains(@class,'card')]");
                foreach (var cardNode in cardNodes)
                {
                    //silly names contain right-single quotation mark
                    var cardName = cardNode.SelectSingleNode(".//span[contains(@class, 'card-title')]")
                                               .InnerText.Replace("&#8217", "&#39")
                                               .Replace("&#8216", "&#39");

                    var name = HttpUtility.HtmlDecode(cardName);

                    //no count there if count == 1
                    var countNode = cardNode.SelectSingleNode(".//span[contains(@class, 'card-count')]");
                    int count = 1;
                    if(countNode != null)
                        count = int.Parse(countNode.InnerText);

                    var card = _hearthstone.GetCardFromName(name);
                    if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
                    {
                        deck.Class = card.PlayerClass;
                    }
                    card.Count = count;
                    deck.Cards.Add(card);
                    
                }
                return deck;
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.Message + "\n" + e.StackTrace);
                return null;
            }
        }

        private async Task<Deck> ImportHearthPwn(string url)
        {
            try
            {
                var doc = await GetHtmlDoc(url);

                var deck = new Deck();

                var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//header/h2[contains(@class,'t-deck-title')]").InnerText);
                deck.Name = deckName;

                var cardNameNodes = doc.DocumentNode.SelectNodes("//td[contains(@class,'col-name')]//a[contains(@href,'/cards/') and contains(@class,'rarity')]");
                var cardCountNodes = doc.DocumentNode.SelectNodes("//td[contains(@class,'col-name')]");
                
                var cardNames = cardNameNodes.Select(cardNameNode => HttpUtility.HtmlDecode(cardNameNode.InnerText));
                var cardCosts =
                    cardCountNodes.Select(countNode => int.Parse(Regex.Match(countNode.LastChild.InnerText, @"\d+").Value));

                var cardInfo = cardNames.Zip(cardCosts, (n , c) => new {Name = n, Count = c});
                foreach (var info in cardInfo)
                {
                    var card = _hearthstone.GetCardFromName(info.Name);
                    card.Count = info.Count;
                    deck.Cards.Add(card);
                    if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
                    {
                        deck.Class = card.PlayerClass;
                    }
                }

                return deck;
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.Message + "\n" + e.StackTrace);
                return null;
            }
        }

        private async Task<Deck> ImportTempostorm(string url)
        {
            try
            {
                var doc = await GetHtmlDoc(url);

                var deck = new Deck();

                var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='main']/h1").InnerText);
                deck.Name = deckName;


                var cardNodes = doc.DocumentNode.SelectNodes("//*[@class='card card-over']");

                foreach (var cardNode in cardNodes)
                {
                    var nameRaw = cardNode.SelectSingleNode(".//span[@class='card-name']").InnerText;
                    var name = HttpUtility.HtmlDecode(nameRaw);
                    var count = cardNode.SelectSingleNode(".//div").Attributes[0].Value.EndsWith("2") ? 2 : 1;
                    var card = _hearthstone.GetCardFromName(name);
                    card.Count = count;
                    deck.Cards.Add(card);
                    if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
                    {
                        deck.Class = card.PlayerClass;
                    }
                }

                return deck;
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.Message + "\n" + e.StackTrace);
                return null;
            }
        }

        private async Task<Deck> ImportHsTopdeck(string url)
        {
            try
            {
                var doc = await GetHtmlDoc(url);

                var deck = new Deck();

                var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//*[@id='deckname']/h1").InnerText).Split('-')[1].Trim();
                deck.Name = deckName;


                var cardNodes = doc.DocumentNode.SelectNodes("//*[@class='cardname']");

                foreach (var cardNode in cardNodes)
                {
                    var text = HttpUtility.HtmlDecode(cardNode.InnerText).Split(' ');
                    var count = int.Parse(text[0].Trim());
                    var name = string.Join(" ", text.Skip(1));
                    
                    var card = _hearthstone.GetCardFromName(name);
                    card.Count = count;
                    deck.Cards.Add(card);
                    if (string.IsNullOrEmpty(deck.Class) && card.PlayerClass != "Neutral")
                    {
                        deck.Class = card.PlayerClass;
                    }
                }

                return deck;
            }
            catch (Exception e)
            {
                Logger.WriteLine(e.Message + "\n" + e.StackTrace);
                return null;
            }
        }

        public async Task<HtmlDocument> GetHtmlDoc(string url)
        {
            using (var wc = new WebClient())
            {
                var websiteContent = await wc.DownloadStringTaskAsync(new Uri(url));
                using (var reader = new StringReader(websiteContent))
                {
                    var doc = new HtmlDocument();
                    doc.Load(reader);
                    return doc;
                }
            }
        }

        public async Task<List<string>> GetPopularDeckLists()
        {
            string url = @"http://hearthstats.net/decks/public?class=&items=500&sort=num_users&order=desc";

            var doc = await GetHtmlDoc(url);

            var deckUrls = new List<string>();
            foreach (var node in doc.DocumentNode.SelectNodes("//td[contains(@class,'name')]"))
            {
                var hrefAttr = node.ChildNodes[0].Attributes.FirstOrDefault(a => a.Name == "href");
                if (hrefAttr != null)
                    deckUrls.Add(@"http://www.hearthstats.net/" + hrefAttr.Value);
            }
            
            return deckUrls;
        }
    }
}
