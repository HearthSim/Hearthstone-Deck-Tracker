#region

using System.Collections.Generic;
using System.Globalization;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Rarity = Hearthstone_Deck_Tracker.Enums.Rarity;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class HearthDbConverter
	{
		public static readonly Dictionary<int, string> SetDict = new Dictionary<int, string>
		{
			{0, null},
			{2, "Basic"},
			{3, "Classic"},
			{4, "Reward"},
			{5, "Missions"},
			{7, "System"},
			{8, "Debug"},
			{11, "Promotion"},
			{12, "Curse of Naxxramas"},
			{13, "Goblins vs Gnomes"},
			{14, "Blackrock Mountain"},
			{15, "The Grand Tournament"},
			{16, "Credits"},
			{17, "Hero Skins"},
			{18, "Tavern Brawl"},
			{20, "League of Explorers"}
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

		public static string SetConverter(CardSet set)
		{
			string str;
			SetDict.TryGetValue((int)set, out str);
			return str;
		}

		public static GameType GetGameType(GameMode mode)
		{
			switch(mode)
			{
				case GameMode.Arena:
					return GameType.GT_ARENA;
				case GameMode.Ranked:
					return GameType.GT_RANKED;
				case GameMode.Casual:
					return GameType.GT_UNRANKED;
				case GameMode.Brawl:
					return GameType.GT_TAVERNBRAWL;
				case GameMode.Friendly:
					return GameType.GT_VS_FRIEND;
				case GameMode.Practice:
					return GameType.GT_VS_AI;
				default:
					return GameType.GT_UNKNOWN;
			}
		}
	}
}