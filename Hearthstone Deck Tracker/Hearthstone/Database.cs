#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HearthDb;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Logging;

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
			Log.Warn("Could not find card with ID=" + cardId);
			return UnknownCard;
		}

		public static Card GetCardFromName(string name, bool localized = false, bool showErrorMessage = true, bool collectible = true)
		{
			var lang = Language.enUS;
			if(localized)
				Enum.TryParse(Config.Instance.SelectedLanguage, out lang);
			try
			{
				var card = Cards.GetFromName(name, lang, collectible);
				if (card != null)
					return new Card(card);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			if(showErrorMessage)
				Log.Warn("Could not get card from name: " + name);
			return UnknownCard;
		}

		public static List<Card> GetActualCards() => Cards.Collectible.Values.Select(x => new Card(x)).ToList();

		public static string GetHeroNameFromId(string id, bool returnIdIfNotFound = true)
		{
			if(string.IsNullOrEmpty(id))
				return returnIdIfNotFound ? id : null;
			string name;
			var baseId = GetBaseId(id);
			if(CardIds.HeroIdDict.TryGetValue(baseId, out name))
				return name;
			var card = GetCardFromId(baseId);
			if(string.IsNullOrEmpty(card?.Name) || card.Name == "UNKNOWN" || card.Type != "Hero")
				return returnIdIfNotFound ? baseId : null;
			return card.Name;
		}

		private static string GetBaseId(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return cardId;
			var match = Regex.Match(cardId, @"(?<base>(.*_\d+)).*");
			return match.Success ? match.Groups["base"].Value : cardId;
		}

		public static bool IsActualCard(Card card) => card != null && Cards.Collectible.ContainsKey(card.Id);

		public static Card UnknownCard => new Card(Cards.All[HearthDb.CardIds.NonCollectible.Neutral.Noooooooooooo]);

		public static string UnknownCardId => HearthDb.CardIds.NonCollectible.Neutral.Noooooooooooo;
	}
}