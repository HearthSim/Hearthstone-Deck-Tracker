#region

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

#endregion

namespace Hearthstone_Deck_Tracker.Importing.Websites
{
	internal class Arenavalue
	{
		public static async Task<Deck> Import(string url)
		{
			try
			{
				var deck = new Deck {IsArenaDeck = true};

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

					var match = Regex.Match(text, @"^\d+\s*(.+?)\s*(x \d+)?$");

					var name = "";
					if(match.Success && match.Groups.Count == 3)
						name = match.Groups[1].ToString();
					if(string.IsNullOrWhiteSpace(name))
						continue;

					var card = Database.GetCardFromName(name);
					card.Count = count;
					deck.Cards.Add(card);

					if(string.IsNullOrEmpty(deck.Class) && card.GetPlayerClass != "Neutral")
						deck.Class = card.PlayerClass;
				}
				deck.Name = Helper.ParseDeckNameTemplate(Config.Instance.ArenaDeckNameTemplate, deck);
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
