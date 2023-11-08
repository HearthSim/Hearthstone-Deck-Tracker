#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using static HearthDb.Enums.BnetGameType;


#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public static class HearthDbConverter
	{
		public static readonly Dictionary<int, string?> SetDict = new Dictionary<int, string?>
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
			{1001, "Knights of the Frozen Throne"},
			{1004, "Kobolds and Catacombs"},
			{1125, "The Witchwood"},
			{1127, "The Boomsday Project"},
			{1129, "Rastakhan's Rumble"},
			{1130, "Rise of Shadows"},
			{1158, "Saviors of Uldum"},
			{1347, "Descent of Dragons"},
			{1403, "Galakrond's Awakening"},
			{1414, "Ashes of Outland"},
			{1439, "Wild Event"},
			{1463, "Demon Hunter Initiate"},
			{1443, "Scholomance Academy"},
			{1466, "Darkmoon Faire"},
			{1525, "The Barrens"},
			{1559, "Wailing Caverns"},
			{1635, "Legacy"},
			{1637, "Core"},
			{1646, "Vanilla"},
			{1578, "United in Stormwind"},
			{1626, "Fractured in Alterac Valley"},
			{1658, "Voyage to the Sunken City"},
			{1691, "Murder at Castle Nathria" },
			{1776, "March of the Lich King" },
			{1869, "Path of Arthas" },
			{(int)CardSet.BATTLE_OF_THE_BANDS, "Festival of Legends" },
			{(int)CardSet.TITANS, "TITANS" },
			{(int)CardSet.WONDERS, "Caverns of Time" },
			{(int)CardSet.WILD_WEST, "Showdown in the Badlands" },
		};

		public static string? ConvertClass(CardClass cardClass)
		{
			if(cardClass == CardClass.DEMONHUNTER)
				return "DemonHunter";
			return (int)cardClass < 1 || (int)cardClass > 10
				  ? null : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cardClass.ToString().ToLowerInvariant());
		}

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

		[Obsolete("Use GetLocalizedRace instead unless you specifically want the English version")]
		public static string? RaceConverter(Race race)
		{
			switch(race)
			{
				case Race.INVALID:
					return null;
				case Race.GOBLIN2:
					return "Goblin";
				case Race.PET:
					return "Beast";
				case Race.MECHANICAL:
					return "Mech";
				default:
					return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(race.ToString().ToLowerInvariant());
			}
		}

		public static string? GetLocalizedRace(Race race) => race switch
			{
				Race.DEMON => LocUtil.Get("Race_Demon", useCardLanguage: true),
				Race.MECHANICAL => LocUtil.Get("Race_Mechanical", useCardLanguage: true),
				Race.BEAST => LocUtil.Get("Race_Beast", useCardLanguage: true),
				Race.DRAGON => LocUtil.Get("Race_Dragon", useCardLanguage: true),
				Race.MURLOC => LocUtil.Get("Race_Murloc", useCardLanguage: true),
				Race.PIRATE => LocUtil.Get("Race_Pirate", useCardLanguage: true),
				Race.ELEMENTAL => LocUtil.Get("Race_Elemental", useCardLanguage: true),
				Race.QUILBOAR => LocUtil.Get("Race_Quilboar", useCardLanguage: true),
				Race.NAGA => LocUtil.Get("Race_Naga", useCardLanguage: true),
				Race.UNDEAD => LocUtil.Get("Race_Undead", useCardLanguage: true),
				Race.TOTEM => LocUtil.Get("Race_Totem", useCardLanguage: true),
				Race.ALL => LocUtil.Get("Race_All", useCardLanguage: true),
				Race.INVALID => LocUtil.Get("Race_Other", useCardLanguage: true),
				_ => RaceConverter(race),
			};

		public static string SetConverter(CardSet set) => SetDict.TryGetValue((int)set, out var str) ? str ?? string.Empty : string.Empty;

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
				case GameType.GT_BATTLEGROUNDS:
				case GameType.GT_BATTLEGROUNDS_FRIENDLY:
					return GameMode.Battlegrounds;
				case GameType.GT_RANKED:
					return GameMode.Ranked;
				case GameType.GT_CASUAL:
					return GameMode.Casual;
				case GameType.GT_TAVERNBRAWL:
				case GameType.GT_TB_2P_COOP:
				case GameType.GT_TB_1P_VS_AI:
				case GameType.GT_FSG_BRAWL_VS_FRIEND:
				case GameType.GT_FSG_BRAWL:
				case GameType.GT_FSG_BRAWL_1P_VS_AI:
				case GameType.GT_FSG_BRAWL_2P_COOP:
					return GameMode.Brawl;
				case GameType.GT_PVPDR:
					return GameMode.Duels;
				case GameType.GT_PVPDR_PAID:
					return GameMode.Duels;
				case GameType.GT_MERCENARIES_AI_VS_AI:
				case GameType.GT_MERCENARIES_FRIENDLY:
				case GameType.GT_MERCENARIES_PVE:
				case GameType.GT_MERCENARIES_PVP:
				case GameType.GT_MERCENARIES_PVE_COOP:
					return GameMode.Mercenaries;
				default:
					return GameMode.None;
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
				case FormatType.FT_CLASSIC:
					return Format.Classic;
				case FormatType.FT_TWIST:
					return Format.Twist;
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
				case Format.Classic:
					return FormatType.FT_CLASSIC;
				case Format.Twist:
					return FormatType.FT_TWIST;
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
					return format switch
					{
						Format.Standard => BGT_RANKED_STANDARD,
						Format.Classic => BGT_RANKED_CLASSIC,
						Format.Twist => BGT_RANKED_TWIST,
						_ => BGT_RANKED_WILD,
					};
				case GameType.GT_CASUAL:
					return format switch
					{
						Format.Standard => BGT_CASUAL_STANDARD,
						Format.Classic => BGT_CASUAL_CLASSIC,
						Format.Twist => BGT_CASUAL_TWIST,
						_ => BGT_CASUAL_WILD,
					};
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
				case GameType.GT_BATTLEGROUNDS:
					return BGT_BATTLEGROUNDS;
				case GameType.GT_BATTLEGROUNDS_FRIENDLY:
					return BGT_BATTLEGROUNDS_FRIENDLY;
				case GameType.GT_PVPDR:
					return BGT_PVPDR;
				case GameType.GT_PVPDR_PAID:
					return BGT_PVPDR_PAID;
				case GameType.GT_MERCENARIES_AI_VS_AI:
					return BGT_UNKNOWN; // Does not exist in BGT
				case GameType.GT_MERCENARIES_FRIENDLY:
					return BGT_MERCENARIES_FRIENDLY;
				case GameType.GT_MERCENARIES_PVE:
					return BGT_MERCENARIES_PVE;
				case GameType.GT_MERCENARIES_PVP:
					return BGT_MERCENARIES_PVP;
				case GameType.GT_MERCENARIES_PVE_COOP:
					return BGT_MERCENARIES_PVE_COOP;
				default:
					return BGT_UNKNOWN;
			}
		}

		public static HearthDb.Deckstrings.Deck? ToHearthDbDeck(Deck deck)
		{
			var card = Database.GetHeroCardFromClass(deck.Class);
			if(card?.DbfId > 0)
			{
				return new HearthDb.Deckstrings.Deck
				{
					Name = deck.Name,
					Format = deck.GuessFormatType(),
					ZodiacYear = (ZodiacYear)Enum.GetValues(typeof(ZodiacYear)).Cast<int>().OrderByDescending(x => x).First(),
					HeroDbfId = card.DbfId,
					CardDbfIds = deck.Cards.ToDictionary(c => c.DbfId, c => c.Count),
					Sideboards = deck.Sideboards.Select(s =>
						new { owner = Database.GetCardFromId(s.OwnerCardId), sideboard = s.Cards.ToDictionary(c => c.DbfId, c => c.Count) }
					).Where(s => s.owner != null).ToDictionary(s => s.owner!.DbfId, s => s.sideboard)
				};
			}
			return null;
		}

		public static HearthDb.Deckstrings.Deck? ToHearthDbDeck(HearthMirror.Objects.Deck deck, FormatType format)
		{
			var heroCard = Database.GetCardFromId(deck.Hero);
			if(heroCard == null)
				return null;

			var cards = deck.Cards.Select(x =>
			{
				var card = Database.GetCardFromId(x.Id);
				if(card == null)
					return null;
				card.Count = x.Count;
				return card;
			}).WhereNotNull();

			Dictionary<int, int> dbfIds;
			Dictionary<int, Dictionary<int, int>> sideboards;
			try
			{
				dbfIds = cards.ToDictionary(c => c.DbfId, c => c.Count);
				sideboards = deck.Sideboards.Select(s =>
					new { owner = Database.GetCardFromId(s.Key), sideboard = s.Value.ToDictionary(c => c.DbfId, c => c.Count) }
				).Where(s => s.owner != null).ToDictionary(s => s.owner!.DbfId, s => s.sideboard);
			}
			catch
			{
				return null;
			}

			return new HearthDb.Deckstrings.Deck
			{
				Name = deck.Name,
				Format = format,
				HeroDbfId = heroCard.DbfId,
				CardDbfIds = dbfIds,
				Sideboards = sideboards,
			};
		}

		public static Deck FromHearthDbDeck(HearthDb.Deckstrings.Deck hDbDeck)
		{
			var deck = new Deck
			{
				Name = hDbDeck.Name,
				Class = Database.GetCardFromDbfId(hDbDeck.HeroDbfId, false)?.PlayerClass
			};
			foreach(var c in hDbDeck.GetCards())
				deck.Cards.Add(new Card(c.Key) { Count = c.Value });
			foreach(var s in hDbDeck.GetSideboards())
				deck.Sideboards.Add(new Sideboard(
					s.Key.Id,
					s.Value.Select(c =>
					{
						var card = Database.GetCardFromId(c.Key.Id);
						if(card == null)
							return null;
						card.Count = c.Value;
						return card;
					}).WhereNotNull().ToList()
				));
			return deck;
		}
	}
}
