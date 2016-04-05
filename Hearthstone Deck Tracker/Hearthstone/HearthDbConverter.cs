#region

using System.Collections.Generic;
using System.Globalization;
using HearthDb.Enums;
using Rarity = Hearthstone_Deck_Tracker.Enums.Rarity;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class HearthDbConverter
	{
		public static readonly Dictionary<int, Enums.CardSet> SetDict = new Dictionary<int, Enums.CardSet>
		{
			{0, Enums.CardSet.None},
			{2, Enums.CardSet.Basic},
			{3, Enums.CardSet.Classic},
			{4, Enums.CardSet.Reward},
			{5, Enums.CardSet.Missions},
			{7, Enums.CardSet.System},
			{8, Enums.CardSet.Debug},
			{11, Enums.CardSet.Promotion},
			{12, Enums.CardSet.Curse_of_Naxxramas},
			{13, Enums.CardSet.Goblins_vs_Gnomes},
			{14, Enums.CardSet.Blackrock_Mountain},
			{15, Enums.CardSet.The_Grand_Tournament},
			{16, Enums.CardSet.Credits},
			{17, Enums.CardSet.Hero_Skins},
			{18, Enums.CardSet.Tavern_Brawl},
			{20, Enums.CardSet.League_of_Explorers}
		};

		public static string ConvertClass(CardClass cardClass) => (int)cardClass < 2 || (int)cardClass > 10
																	  ? null : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cardClass.ToString().ToLowerInvariant());

		public static Rarity RariryConverter(HearthDb.Enums.Rarity rarity)
		{
			switch(rarity)
			{
				case HearthDb.Enums.Rarity.FREE:
					return Rarity.Free;
				case HearthDb.Enums.Rarity.COMMON:
					return Rarity.Common;
				case HearthDb.Enums.Rarity.RARE:
					return Rarity.Rare;
				case HearthDb.Enums.Rarity.EPIC:
					return Rarity.Epic;
				case HearthDb.Enums.Rarity.LEGENDARY:
					return Rarity.Legendary;
				default:
					return Rarity.Free;
			}
		}

		public static string CardTypeConverter(CardType type) => type == CardType.HERO_POWER ? "Hero Power" : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(type.ToString().ToLowerInvariant().Replace("_", ""));


		public static string RaceConverter(Race race)
		{
			switch(race)
			{
				case Race.INVALID:
					return null;
				case Race.GOBLIN2:
					return "Goblin";
				case Race.PET:
					return "Beast";
				default:
					return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(race.ToString().ToLowerInvariant());
			}
		}

		public static Enums.CardSet SetConverter(CardSet set)
		{
			Enums.CardSet cardSet;
			SetDict.TryGetValue((int)set, out cardSet);
			return cardSet;
		}
	}
}