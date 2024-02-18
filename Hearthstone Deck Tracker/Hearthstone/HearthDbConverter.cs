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
			{(int)CardSet.BASIC, "Basic"},
			{(int)CardSet.EXPERT1, "Classic"},
			{(int)CardSet.HOF, "Hall of Fame"},
			{(int)CardSet.MISSIONS, "Missions"},
			{(int)CardSet.NONE, "System"},
			{(int)CardSet.CHEAT, "Debug"},
			{(int)CardSet.PROMO, "Promotion"},
			{(int)CardSet.NAXX, "Curse of Naxxramas"},
			{(int)CardSet.GVG, "Goblins vs Gnomes"},
			{(int)CardSet.BRM, "Blackrock Mountain"},
			{(int)CardSet.TGT, "The Grand Tournament"},
			{(int)CardSet.CREDITS, "Credits"},
			{(int)CardSet.HERO_SKINS, "Hero Skins"},
			{(int)CardSet.TB, "Tavern Brawl"},
			{(int)CardSet.LOE, "League of Explorers"},
			{(int)CardSet.OG, "Whispers of the Old Gods"},
			{(int)CardSet.KARA, "One Night in Karazhan"},
			{(int)CardSet.GANGS, "Mean Streets of Gadgetzan"},
			{(int)CardSet.UNGORO, "Journey to Un'Goro"},
			{(int)CardSet.ICECROWN, "Knights of the Frozen Throne"},
			{(int)CardSet.LOOTAPALOOZA, "Kobolds and Catacombs"},
			{(int)CardSet.GILNEAS, "The Witchwood"},
			{(int)CardSet.BOOMSDAY, "The Boomsday Project"},
			{(int)CardSet.TROLL, "Rastakhan's Rumble"},
			{(int)CardSet.DALARAN, "Rise of Shadows"},
			{(int)CardSet.ULDUM, "Saviors of Uldum"},
			{(int)CardSet.DRAGONS, "Descent of Dragons"},
			{(int)CardSet.YEAR_OF_THE_DRAGON, "Galakrond's Awakening"},
			{(int)CardSet.BLACK_TEMPLE, "Ashes of Outland"},
			{(int)CardSet.WILD_EVENT, "Wild Event"},
			{(int)CardSet.DEMON_HUNTER_INITIATE, "Demon Hunter Initiate"},
			{(int)CardSet.SCHOLOMANCE, "Scholomance Academy"},
			{(int)CardSet.DARKMOON_FAIRE, "Darkmoon Faire"},
			{(int)CardSet.THE_BARRENS, "The Barrens"},
			{(int)CardSet.WAILING_CAVERNS, "Wailing Caverns"},
			{(int)CardSet.LEGACY, "Legacy"},
			{(int)CardSet.CORE, "Core"},
			{(int)CardSet.VANILLA, "Vanilla"},
			{(int)CardSet.STORMWIND, "United in Stormwind"},
			{(int)CardSet.ALTERAC_VALLEY, "Fractured in Alterac Valley"},
			{(int)CardSet.THE_SUNKEN_CITY, "Voyage to the Sunken City"},
			{(int)CardSet.REVENDRETH, "Murder at Castle Nathria" },
			{(int)CardSet.RETURN_OF_THE_LICH_KING, "March of the Lich King" },
			{(int)CardSet.PATH_OF_ARTHAS, "Path of Arthas" },
			{(int)CardSet.BATTLE_OF_THE_BANDS, "Festival of Legends" },
			{(int)CardSet.TITANS, "TITANS" },
			{(int)CardSet.WONDERS, "Caverns of Time" },
			{(int)CardSet.WILD_WEST, "Showdown in the Badlands" },
			{(int)CardSet.WHIZBANGS_WORKSHOP, "Whizbang's Workshop" },
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

		public static string? GetLocalizedSpellSchool(SpellSchool spellSchool) => spellSchool switch
		{
			SpellSchool.NONE => null,
			SpellSchool.ARCANE => LocUtil.Get("Spell_School_Arcane", useCardLanguage: true),
			SpellSchool.FIRE => LocUtil.Get("Spell_School_Fire", useCardLanguage: true),
			SpellSchool.FROST => LocUtil.Get("Spell_School_Frost", useCardLanguage: true),
			SpellSchool.HOLY => LocUtil.Get("Spell_School_Holy", useCardLanguage: true),
			SpellSchool.NATURE => LocUtil.Get("Spell_School_Nature", useCardLanguage: true),
			SpellSchool.SHADOW => LocUtil.Get("Spell_School_Shadow", useCardLanguage: true),
			SpellSchool.FEL => LocUtil.Get("Spell_School_Fel", useCardLanguage: true),
			SpellSchool.PHYSICAL_COMBAT => LocUtil.Get("Spell_School_Physical_Combat", useCardLanguage: true),
			_ => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(spellSchool.ToString().ToLowerInvariant())
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
					new
					{
						owner = Database.GetCardFromId(s.Key),
						sideboard = s.Value.ToDictionary(c => Database.GetCardFromId(c.Id)?.DbfId ?? 0, c => c.Count)
					}
				).Where(s => s.owner != null && s.sideboard.Keys.All(x => x != 0)).ToDictionary(s => s.owner!.DbfId, s => s.sideboard);
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
