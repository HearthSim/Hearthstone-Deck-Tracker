using System;
using System.Linq;
using System.Text.RegularExpressions;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Importing
{
	public static class StringImporter
	{
		private static readonly Regex CardLineRegexCountFirst = new Regex(@"(^(\s*)(?<count>\d)(\s*x)?\s+)(?<cardname>[\w\s'\.:!\-(),]+)");
		private static readonly Regex CardLineRegexCountLast = new Regex(@"(?<cardname>[\w\s'\.:!\-(),]+?)(\s+(x\s*)?(?<count>\d))(\s*)$");
		public static char[] Separators = { '\n', '|' };

		public static bool IsValidImportString(string importString)
		{
			var separatorCount = importString.Count(x => Separators.Contains(x));
			return separatorCount > 5 && separatorCount < 40;
		}

		public static Deck? Import(string cards, bool localizedNames = false)
		{
			CardClass[] AvailableClasses(Card x)
			{
				var card = HearthDb.Cards.GetFromDbfId(x.DbfId);
				switch((MultiClassGroup)card.Entity.GetTag(GameTag.MULTI_CLASS_GROUP))
				{
					case MultiClassGroup.GRIMY_GOONS:
						return new[] { CardClass.WARRIOR, CardClass.HUNTER, CardClass.PALADIN };
					case MultiClassGroup.JADE_LOTUS:
						return new[] { CardClass.ROGUE, CardClass.DRUID, CardClass.SHAMAN };
					case MultiClassGroup.KABAL:
						return new[] { CardClass.MAGE, CardClass.PRIEST, CardClass.WARLOCK };
					case MultiClassGroup.PALADIN_PRIEST:
						return new[] { CardClass.PALADIN, CardClass.PRIEST };
					case MultiClassGroup.PRIEST_WARLOCK:
						return new[] { CardClass.PRIEST, CardClass.WARLOCK };
					case MultiClassGroup.WARLOCK_DEMONHUNTER:
						return new[] { CardClass.WARLOCK, CardClass.DEMONHUNTER };
					case MultiClassGroup.HUNTER_DEMONHUNTER:
						return new[] { CardClass.HUNTER, CardClass.DEMONHUNTER };
					case MultiClassGroup.DRUID_HUNTER:
						return new[] { CardClass.DRUID, CardClass.HUNTER };
					case MultiClassGroup.DRUID_SHAMAN:
						return new[] { CardClass.DRUID, CardClass.SHAMAN };
					case MultiClassGroup.MAGE_SHAMAN:
						return new[] { CardClass.MAGE, CardClass.SHAMAN };
					case MultiClassGroup.MAGE_ROGUE:
						return new[] { CardClass.MAGE, CardClass.ROGUE };
					case MultiClassGroup.ROGUE_WARRIOR:
						return new[] { CardClass.ROGUE, CardClass.WARRIOR };
					case MultiClassGroup.PALADIN_WARRIOR:
						return new[] { CardClass.PALADIN, CardClass.WARRIOR };
					default:
						return new[] { card.Class };
				}
			}
			try
			{
				var deck = new Deck();
				var lines = cards.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
				foreach(var line in lines)
				{
					var count = 1;
					var cardName = line.Trim();
					Match? match = null;
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
					if(string.IsNullOrEmpty(card?.Name) && card!.IsKnownCard)
						continue;
					card!.Count = count;

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
				var deckClass = deck.Cards
					.Where(x => x.DbfId != 0)
					.Select(AvailableClasses)
					.Where(x => x.Length > 1 || x[0] != CardClass.NEUTRAL)
					.Aggregate((a, b) => a.Concat(b).GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToArray());
				if(deckClass.Length > 1)
				{
					Log.Warn("Could not identify a class for this deck. Found multiple potential classes: " + string.Join(", ", deckClass.Select(HearthDbConverter.ConvertClass)));
					return null;
				}
				else if(deckClass.Length == 0)
				{
					Log.Warn("Could not identify a class for this deck. Found conflicting classes.");
					return null;
				}
				else
				{
					deck.Class = HearthDbConverter.ConvertClass(deckClass[0]);
					return deck;
				}
			}
			catch(Exception ex)
			{
				Log.Error(ex);
				return null;
			}
		}
	}
}
