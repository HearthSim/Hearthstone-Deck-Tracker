#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HearthDb;
using HearthDb.Enums;
using Rarity = Hearthstone_Deck_Tracker.Enums.Rarity;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class Database
	{
		public static Card GetCardFromId(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return null;
			HearthDb.Card dbCard;
			if(Cards.All.TryGetValue(cardId, out dbCard))
				return new Card(dbCard);
			Logger.WriteLine("Could not find card with ID=" + cardId, "Database");
			return new Card(cardId, null, Rarity.Free, "Minion", "UNKNOWN", 0, "UNKNOWN", 0, 1, "", "", 0, 0, "UNKNOWN", null, 0, "", "");
		}

		public static Card GetCardFromName(string name, bool localized = false, bool showErrorMessage = true, bool collectible = true)
		{
			Language lang = Language.enUS;
			if(localized)
				Enum.TryParse(Config.Instance.SelectedLanguage, out lang);
			var card = Cards.GetFromName(name, lang, collectible);
			if(card != null)
				return new Card(card);
			if(showErrorMessage)
				Logger.WriteLine("Could not get card from name: " + name, "Database");
			return new Card("UNKNOWN", null, Rarity.Free, "Minion", name, 0, name, 0, 1, "", "", 0, 0, "UNKNOWN", null, 0, "", "");
		}

		public static List<Card> GetActualCards()
		{
			return Cards.Collectible.Values.Select(x => new Card(x)).ToList();
		}

		public static string GetHeroNameFromId(string id, bool returnIdIfNotFound = true)
		{
			string name;
			var match = Regex.Match(id, @"(?<base>(.*_\d+)).*");
			if(match.Success)
				id = match.Groups["base"].Value;
			if(CardIds.HeroIdDict.TryGetValue(id, out name))
				return name;
			var card = GetCardFromId(id);
			if(card == null || string.IsNullOrEmpty(card.Name) || card.Name == "UNKNOWN" || card.Type != "Hero")
				return returnIdIfNotFound ? id : null;
			return card.Name;
		}

		public static bool IsActualCard(Card card)
		{
			return card != null && Cards.Collectible.ContainsKey(card.Id);
		}
	}
}