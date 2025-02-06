﻿#region

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
		public static Card? GetCardFromId(string? cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return null;
			return new Card(cardId!);
		}

		public static Card? GetCardFromDbfId(int dbfId, bool collectible = true)
		{
			if(dbfId == 0)
				return null;
			var card = Cards.GetFromDbfId(dbfId, collectible);
			if(card != null)
				return new Card(card);
			// TODO ?? should be have a dbfId ctor?
			return null;
		}

		public static Card? GetCardFromName(string name, bool localized = false, bool showErrorMessage = true, bool collectible = true)
		{
			var langs = new List<Locale> {Locale.enUS};
			foreach(var lang in langs)
			{
				try
				{
					var card = Cards.GetFromName(name, lang, collectible);
					if(card != null)
						return new Card(card);
				}
				catch(Exception ex)
				{
					Log.Error(ex);
				}
			}
			if(showErrorMessage)
				Log.Warn("Could not get card from name: " + name);
			return null;
		}

		public static List<Card> GetActualCards() => Cards.Collectible.Values.Select(x => new Card(x)).ToList();

		public static string? GetHeroNameFromId(string? id, bool returnIdIfNotFound = true)
		{
			if(string.IsNullOrEmpty(id))
				return returnIdIfNotFound ? id : null;
			var baseId = GetBaseId(id);
			if(string.IsNullOrEmpty(baseId))
				return returnIdIfNotFound ? id : null;
			if(CardIds.HeroIdDict.TryGetValue(baseId!, out var name))
				return name;
			var card = GetCardFromId(baseId);
			bool IsValidHeroCard(Card? c) => !string.IsNullOrEmpty(c?.Name) && c!.Name != "UNKNOWN" && c!.Type == "Hero";
			if(!IsValidHeroCard(card))
			{
				card = GetCardFromId(id);
				if(!IsValidHeroCard(card))
					return returnIdIfNotFound ? baseId : null;
			}
			return card?.Name;
		}

		public static Card? GetHeroCardFromClass(string? className)
		{
			if(string.IsNullOrEmpty(className))
				return null;
			if(!CardIds.HeroNameDict.TryGetValue(className!, out var heroId) || string.IsNullOrEmpty(heroId))
				return null;
			return GetCardFromId(heroId);
		}

		public static Card? GetBattlegroundsHeroFromDbf(int dbfId)
		{
			var hero = GetCardFromDbfId(dbfId, false);
			var parentSkinDbfId = hero?.BattlegroundsSkinParentId;
			if(parentSkinDbfId > 0)
				return GetCardFromDbfId(parentSkinDbfId.Value, false);
			return hero;
		}

		private static string? GetBaseId(string? cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return cardId;
			var match = Regex.Match(cardId, @"(?<base>(.*_\d+)).*");
			return match.Success ? match.Groups["base"].Value : cardId;
		}

		public static bool IsActualCard(Card card) => card != null && Cards.Collectible.ContainsKey(card.Id);
	}
}
