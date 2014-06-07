using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            try
            {
                var doc = GetHtmlDoc(url);
                var deck = new Deck();

                //get deck name
                foreach (var a in doc.DocumentNode.SelectNodes("//meta[contains(@name,'description')]"))
                {
                    foreach (var attribute in a.Attributes)
                    {
                        if (attribute.Name == "content")
                        {
                            deck.Name = HttpUtility.HtmlDecode(attribute.Value);
                            break;
                        }
                    }
                    if (!string.IsNullOrEmpty(deck.Name))
                        break;
                }


                foreach (var a in doc.DocumentNode.SelectNodes("//div"))
                {
                    if (string.IsNullOrEmpty(deck.Class))
                    {
                        //get deck class
                        var colmd6Attr = a.Attributes.FirstOrDefault(attr => attr.Value == "col-md-6");

                        if (colmd6Attr != null)
                        {
                            var classAttr = a.ChildNodes.FirstOrDefault(c => c.InnerText.Contains("Class:"));
                            if (classAttr != null)
                            {
                                deck.Class = classAttr.InnerText.Split(';')[1];
                            }
                        }
                    }


                    var cardName = "";
                    var cardCount = 0;
                    foreach (var node in a.ChildNodes)
                    {
                        if (node.Attributes.Count == 0)
                            continue;
                        if (node.Attributes[0].Value == "name")
                        {
                            cardName = HttpUtility.HtmlDecode(node.InnerText);
                        }
                        else if (node.Attributes[0].Value == "qty")
                        {
                            cardCount = int.Parse(node.InnerText);
                        }

                    }

                    if (cardName != string.Empty)
                    {
                        var realCard = _hearthstone.GetCardFromName(cardName);
                        realCard.Count = cardCount;
                        deck.Cards.Add(realCard);
                    }
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
