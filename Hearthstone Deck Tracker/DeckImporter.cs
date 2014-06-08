using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
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
        
        public Deck Import(string url)
        {
            if (url.Contains("hearthstats"))
            {
                return ImportHearthStats(url);
            }
            if (url.Contains("hearthpwn"))
            {
                return ImportHearthPwn(url);
            }
            if (url.Contains("hearthhead"))
            {
                return null; //ImportHearthHead(url);
            }
            return null;
        }

        private Deck ImportHearthStats(string url)
        {
            try
            {
                var doc = GetHtmlDoc(url);

                var deck = new Deck();

                var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'page-title')]").FirstChild.InnerText);
                deck.Name = deckName;

                var deckClass =
                    HttpUtility.HtmlDecode(
                        doc.DocumentNode.SelectSingleNode("//div[contains(@class,'col-md-6')]/p").LastChild.InnerText);
                deck.Class = char.ToUpper(deckClass[0]) + deckClass.Substring(1);

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
                }

                return deck;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                return null;
            }
        }
        private Deck ImportHearthHead(string url)
        {
            try
            {
                var doc = GetHtmlDoc(url);

                var deck = new Deck();
                
                var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//div[contains(@class,'text')]/h1").InnerText);
                deck.Name = deckName;

                var deckClass =
                    HttpUtility.HtmlDecode(
                        doc.DocumentNode.SelectSingleNode("//span[contains(@class,'breadcrumb-arrow')]/a[contains(@href,'/decks?filter')]").InnerText);
                deck.Class = char.ToUpper(deckClass[0]) + deckClass.Substring(1);
                
                var cardNameNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'deckguide-cards-type')]//ul//li//a");

                var cardCountNodes = doc.DocumentNode.SelectNodes("//div[contains(@class,'deckguide-cards-type')]/ul/li");
                
                var cardNames = cardNameNodes.Select(cardNameNode => HttpUtility.HtmlDecode(cardNameNode.InnerText));
                var cardCosts =
                    cardCountNodes.Select(countNode => int.Parse(countNode.LastChild.InnerText));

                var cardInfo = cardNames.Zip(cardCosts, (n, c) => new { Name = n, Count = c });
                foreach (var info in cardInfo)
                {
                    var card = _hearthstone.GetCardFromName(info.Name);
                    card.Count = info.Count;
                    deck.Cards.Add(card);
                }

                return deck;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                return null;
            }
        }

        private Deck ImportHearthPwn(string url)
        {
            try
            {
                var doc = GetHtmlDoc(url);

                var deck = new Deck();

                var deckName = HttpUtility.HtmlDecode(doc.DocumentNode.SelectSingleNode("//header/h2[contains(@class,'t-deck-title')]").InnerText);
                deck.Name = deckName;

                var deckClass =
                    HttpUtility.HtmlDecode(
                        doc.DocumentNode.SelectSingleNode("//header/span[contains(@class,'class')]").Attributes[0].Value.Split('-')[1]);
                deck.Class = char.ToUpper(deckClass[0]) + deckClass.Substring(1);

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
                }

                return deck;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                return null;
            }
        }

        public HtmlDocument GetHtmlDoc(string url)
        {
            using (var wc = new WebClient())
            {
                var websiteContent = wc.DownloadString(url);
                using (var reader = new StringReader(websiteContent))
                {
                    var doc = new HtmlDocument();
                    doc.Load(reader);
                    return doc;
                }
            }
        }

        public List<string> GetPopularDeckLists()
        {
            string url = @"http://hearthstats.net/decks/public?class=&items=500&sort=num_users&order=desc";

            var doc = GetHtmlDoc(url);

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
