using System;
using System.Linq;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Importing
{
	public static class StringImporter
	{
		private static readonly Regex CardLineRegexCountFirst = new Regex(@"(^(\s*)(?<count>\d)(\s*x)?\s+)(?<cardname>[\w\s'\.:!\-,]+)");
		private static readonly Regex CardLineRegexCountLast = new Regex(@"(?<cardname>[\w\s'\.:!\-,]+?)(\s+(x\s*)?(?<count>\d))(\s*)$");
		public static char[] Separators = { '\n', '|' };

		public static bool IsValidImportString(string importString)
		{
			var separatorCount = importString.Count(x => Separators.Contains(x));
			return separatorCount > 5 && separatorCount < 40;
		}

		public static Deck Import(string cards, bool localizedNames = false)
		{
			try
			{
				var deck = new Deck();
				var lines = cards.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
				foreach(var line in lines)
				{
					var count = 1;
					var cardName = line.Trim();
					Match match = null;
					if(CardLineRegexCountFirst.IsMatch(cardName))
						match = CardLineRegexCountFirst.Match(cardName);
					else if(CardLineRegexCountLast.IsMatch(cardName))
						match = CardLineRegexCountLast.Match(cardName);
					if(match != null)
					{
						var tmpCount = match.Groups["count"];
						if(tmpCount.Success)
							count = int.Parse(tmpCount.Value);
						cardName = match.Groups["cardname"].Value.Trim();
					}

					var card = Database.GetCardFromName(cardName.Replace("’", "'"), localizedNames);
					if(string.IsNullOrEmpty(card?.Name) || card.Id == Database.UnknownCardId)
						continue;
					card.Count = count;

					if(deck.Cards.Contains(card))
					{
						var deckCard = deck.Cards.First(c => c.Equals(card));
						deck.Cards.Remove(deckCard);
						deckCard.Count += count;
						deck.Cards.Add(deckCard);
					}
					else
						deck.Cards.Add(card);
				}
				var classes = deck.Cards.Where(x => x.PlayerClass != null).GroupBy(x => x.PlayerClass).ToList();
				if(classes.Count != 1)
				{
					Log.Warn($"Could not identify a class for this deck. Found class cards for {classes.Count} classes.");
					return null;
				}
				deck.Class = classes.Single().Key;
				return deck;
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}
	}
}
