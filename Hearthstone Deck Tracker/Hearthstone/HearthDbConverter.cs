#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using static HearthDb.Enums.BnetGameType;

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
			{4, "Hall of Fame"},
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
			{20, "League of Explorers"},
			{21, "Whispers of the Old Gods"},
			{23, "One Night in Karazhan"},
			{25, "Mean Streets of Gadgetzan"},
			{27, "Journey to Un'Goro"},
			{1001, "Knights of the Frozen Throne"}
		};

		public static string ConvertClass(CardClass cardClass) => (int)cardClass < 2 || (int)cardClass > 10
																	  ? null : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cardClass.ToString().ToLowerInvariant());

		public static string CardTypeConverter(CardType type)
		{
			switch(type)
			{
				case CardType.ABILITY:
					return "Spell";
				case CardType.HERO_POWER:
					return "Hero Power";
				default:
					return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(type.ToString().ToLowerInvariant());
			}
		}


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

		public static string SetConverter(CardSet set) => SetDict.TryGetValue((int)set, out var str) ? str : string.Empty;

		public static GameMode GetGameMode(GameType gameType)
		{
			switch(gameType)
			{
				case GameType.GT_VS_AI:
					return GameMode.Practice;
				case GameType.GT_VS_FRIEND:
					return GameMode.Friendly;
				case GameType.GT_ARENA:
					return GameMode.Arena;
				case GameType.GT_RANKED:
					return GameMode.Ranked;
				case GameType.GT_CASUAL:
					return GameMode.Casual;
				case GameType.GT_TAVERNBRAWL:
				case GameType.GT_TB_2P_COOP:
				case GameType.GT_TB_1P_VS_AI:
					return GameMode.Brawl;
				default:
					return GameMode.None;
			}
		}

		public static BnetGameType GetGameType(GameMode mode, Format? format)
		{
			switch(mode)
			{
				case GameMode.Arena:
					return BGT_ARENA;
				case GameMode.Ranked:
					return format == Format.Standard ? BGT_RANKED_STANDARD : BGT_RANKED_WILD;
				case GameMode.Casual:
					return format == Format.Standard ? BGT_CASUAL_STANDARD : BGT_CASUAL_WILD;
				case GameMode.Brawl:
					return BGT_TAVERNBRAWL_PVP;
				case GameMode.Friendly:
					return BGT_FRIENDS;
				case GameMode.Practice:
					return BGT_VS_AI;
				default:
					return BGT_UNKNOWN;
			}
		}

		public static Format? GetFormat(FormatType format)
		{
			switch(format)
			{
				case FormatType.FT_WILD:
					return Format.Wild;
				case FormatType.FT_STANDARD:
					return Format.Standard;
				default:
					return null;
			}
		}

		public static FormatType GetFormatType(Format? format)
		{
			if(format == null)
				return FormatType.FT_UNKNOWN;
			switch(format.Value)
			{
				case Format.Standard:
					return FormatType.FT_STANDARD;
				case Format.Wild:
					return FormatType.FT_WILD;
			}
			return FormatType.FT_UNKNOWN;
		}

		public static BnetGameType GetBnetGameType(GameType gameType, Format? format)
		{
			switch(gameType)
			{
				case GameType.GT_UNKNOWN:
					return BGT_UNKNOWN;
				case GameType.GT_VS_AI:
					return BGT_VS_AI;
				case GameType.GT_VS_FRIEND:
					return BGT_FRIENDS;
				case GameType.GT_TUTORIAL:
					return BGT_TUTORIAL;
				case GameType.GT_ARENA:
					return BGT_ARENA;
				case GameType.GT_TEST:
					return BGT_TEST1;
				case GameType.GT_RANKED:
					return format == Format.Standard ? BGT_RANKED_STANDARD : BGT_RANKED_WILD;
				case GameType.GT_CASUAL:
					return format == Format.Standard ? BGT_CASUAL_STANDARD : BGT_CASUAL_WILD;
				case GameType.GT_TAVERNBRAWL:
					return BGT_TAVERNBRAWL_PVP;
				case GameType.GT_TB_1P_VS_AI:
					return BGT_TAVERNBRAWL_1P_VERSUS_AI;
				case GameType.GT_TB_2P_COOP:
					return BGT_TAVERNBRAWL_2P_COOP;
				case GameType.GT_FSG_BRAWL:
					return BGT_FSG_BRAWL_VS_FRIEND;
				case GameType.GT_FSG_BRAWL_1P_VS_AI:
					return BGT_FSG_BRAWL_1P_VERSUS_AI;
				case GameType.GT_FSG_BRAWL_2P_COOP:
					return BGT_FSG_BRAWL_2P_COOP;
				case GameType.GT_FSG_BRAWL_VS_FRIEND:
					return BGT_FSG_BRAWL_VS_FRIEND;
				default:
					return BGT_UNKNOWN;
			}
		}

		public static HearthDb.Deckstrings.Deck ToHearthDbDeck(Deck deck)
		{
			var card = Database.GetHeroCardFromClass(deck.Class);
			if(card?.DbfIf > 0)
			{
				return new HearthDb.Deckstrings.Deck
				{
					Name = deck.Name,
					Format = deck.IsWildDeck ? FormatType.FT_WILD : FormatType.FT_STANDARD,
					ZodiacYear = (ZodiacYear)Enum.GetValues(typeof(ZodiacYear)).Cast<int>().OrderByDescending(x => x).First(),
					HeroDbfId = card.DbfIf,
					CardDbfIds = deck.Cards.ToDictionary(c => c.DbfIf, c => c.Count)
				};
			}
			return null;
		}

		public static Deck FromHearthDbDeck(HearthDb.Deckstrings.Deck hDbDeck)
		{
			var deck = new Deck
			{
				Name = hDbDeck.Name,
				Class = Database.GetCardFromDbfId(hDbDeck.HeroDbfId, false).PlayerClass
			};
			foreach(var c in hDbDeck.GetCards())
				deck.Cards.Add(new Card(c.Key) { Count = c.Value });
			return deck;
		}
	}
}
