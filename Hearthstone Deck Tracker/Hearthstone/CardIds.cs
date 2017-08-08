#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class CardIds
	{
		public static readonly List<string> InvalidCardIds = new List<string>
		{
			"EX1_tk34",
			"EX1_tk29",
			"EX1_tk28",
			"EX1_tk11",
			"EX1_598",
			"NEW1_032",
			"NEW1_033",
			"NEW1_034",
			"NEW1_009",
			"CS2_052",
			"CS2_082",
			"CS2_051",
			"CS2_050",
			"CS2_152",
			"skele11",
			"skele21",
			"GAME",
			"DREAM",
			"NEW1_006",
			"NAX",
			"FP1_006",
			"PART",
			"BRMA",
			"BRMC",
			"TBA",
			"TB_",
			"TBST_",
			"LOEA",
			"LOE_008",
			"LOE_030"
		};

		// todo: conditional deathrattle summons: Voidcaller
		public static readonly Dictionary<string, int> DeathrattleSummonCardIds = new Dictionary<string, int>
		{
			{HearthDb.CardIds.Collectible.Druid.MountedRaptor, 1},
			{HearthDb.CardIds.Collectible.Hunter.SavannahHighmane, 2},
			{HearthDb.CardIds.Collectible.Rogue.Anubarak, 1},
			{HearthDb.CardIds.Collectible.Warlock.Dreadsteed, 1},
			{HearthDb.CardIds.Collectible.Warlock.Voidcaller, 1}, //false negative better than false positive
			{HearthDb.CardIds.Collectible.Neutral.CairneBloodhoof, 1},
			{HearthDb.CardIds.Collectible.Neutral.HarvestGolem, 1},
			{HearthDb.CardIds.Collectible.Neutral.PilotedShredder, 1},
			{HearthDb.CardIds.Collectible.Neutral.PilotedSkyGolem, 1},
			{HearthDb.CardIds.Collectible.Neutral.SneedsOldShredder, 1},
			{HearthDb.CardIds.Collectible.Neutral.HauntedCreeper, 2},
			{HearthDb.CardIds.Collectible.Neutral.NerubianEgg, 1},
			{HearthDb.CardIds.Collectible.Neutral.SludgeBelcher, 1},
			{HearthDb.CardIds.Collectible.Neutral.WobblingRunts, 3}
		};

		public static readonly Dictionary<string, string> HeroIdDict = new Dictionary<string, string>
		{
			{HearthDb.CardIds.Collectible.Warrior.GarroshHellscream, "Warrior"},
			{HearthDb.CardIds.Collectible.Shaman.Thrall, "Shaman"},
			{HearthDb.CardIds.Collectible.Rogue.ValeeraSanguinar, "Rogue"},
			{HearthDb.CardIds.Collectible.Paladin.UtherLightbringer, "Paladin"},
			{HearthDb.CardIds.Collectible.Hunter.Rexxar, "Hunter"},
			{HearthDb.CardIds.Collectible.Druid.MalfurionStormrage, "Druid"},
			{HearthDb.CardIds.Collectible.Warlock.Guldan, "Warlock"},
			{HearthDb.CardIds.Collectible.Mage.JainaProudmoore, "Mage"},
			{HearthDb.CardIds.Collectible.Priest.AnduinWrynn, "Priest"},
			{HearthDb.CardIds.Collectible.Warlock.LordJaraxxus, "Jaraxxus"},
			{HearthDb.CardIds.Collectible.Neutral.MajordomoExecutus, "Ragnaros the Firelord"}
		};

		public static readonly Dictionary<string, string> HeroNameDict = new Dictionary<string, string>
		{
			{"Warrior", HearthDb.CardIds.Collectible.Warrior.GarroshHellscream},
			{"Shaman", HearthDb.CardIds.Collectible.Shaman.Thrall},
			{"Rogue", HearthDb.CardIds.Collectible.Rogue.ValeeraSanguinar},
			{"Paladin", HearthDb.CardIds.Collectible.Paladin.UtherLightbringer},
			{"Hunter", HearthDb.CardIds.Collectible.Hunter.Rexxar},
			{"Druid", HearthDb.CardIds.Collectible.Druid.MalfurionStormrage},
			{"Warlock", HearthDb.CardIds.Collectible.Warlock.Guldan},
			{"Mage", HearthDb.CardIds.Collectible.Mage.JainaProudmoore},
			{"Priest", HearthDb.CardIds.Collectible.Priest.AnduinWrynn}
		};

		public static class Secrets
		{
			public static List<string> FastCombat = new List<string>
			{
				Hunter.FreezingTrap,
				Hunter.ExplosiveTrap,
				Hunter.Misdirection,
				Paladin.NobleSacrifice,
				Mage.Vaporize
			};

			public static class Hunter
			{
				public static List<string> All => new List<string> {BearTrap, CatTrick, DartTrap, ExplosiveTrap, FreezingTrap, HiddenCache, Misdirection, Snipe, SnakeTrap, VemonstrikeTrap};
				public static string BearTrap => HearthDb.CardIds.Collectible.Hunter.BearTrap;
				public static string CatTrick => HearthDb.CardIds.Collectible.Hunter.CatTrick;
				public static string DartTrap => HearthDb.CardIds.Collectible.Hunter.DartTrap;
				public static string ExplosiveTrap => HearthDb.CardIds.Collectible.Hunter.ExplosiveTrap;
				public static string FreezingTrap => HearthDb.CardIds.Collectible.Hunter.FreezingTrap;
				public static string HiddenCache => HearthDb.CardIds.Collectible.Hunter.HiddenCache;
				public static string Misdirection => HearthDb.CardIds.Collectible.Hunter.Misdirection;
				public static string Snipe => HearthDb.CardIds.Collectible.Hunter.Snipe;
				public static string SnakeTrap => HearthDb.CardIds.Collectible.Hunter.SnakeTrap;
				public static string VemonstrikeTrap => HearthDb.CardIds.Collectible.Hunter.VenomstrikeTrap;

				public static List<string> GetCards(bool standardOnly) => 
					standardOnly ? All.Where(x => !Helper.WildOnlySets.Contains(Database.GetCardFromId(x).Set)).ToList() : All;
			}

			public static class Mage
			{
				public static List<string> All => new List<string> {Counterspell, Duplicate, Effigy, FrozenClone, IceBarrier, IceBlock, ManaBind, MirrorEntity, PotionOfPolymorph, Spellbender, Vaporize};
				public static string Counterspell => HearthDb.CardIds.Collectible.Mage.Counterspell;
				public static string Duplicate => HearthDb.CardIds.Collectible.Mage.Duplicate;
				public static string Effigy => HearthDb.CardIds.Collectible.Mage.Effigy;
				public static string FrozenClone => HearthDb.CardIds.Collectible.Mage.FrozenClone;
				public static string IceBarrier => HearthDb.CardIds.Collectible.Mage.IceBarrier;
				public static string IceBlock => HearthDb.CardIds.Collectible.Mage.IceBlock;
				public static string ManaBind => HearthDb.CardIds.Collectible.Mage.ManaBind;
				public static string MirrorEntity => HearthDb.CardIds.Collectible.Mage.MirrorEntity;
				public static string PotionOfPolymorph => HearthDb.CardIds.Collectible.Mage.PotionOfPolymorph;
				public static string Spellbender => HearthDb.CardIds.Collectible.Mage.Spellbender;
				public static string Vaporize => HearthDb.CardIds.Collectible.Mage.Vaporize;

				public static List<string> GetCards(bool standardOnly) =>
					standardOnly ? All.Where(x => !Helper.WildOnlySets.Contains(Database.GetCardFromId(x).Set)).ToList() : All;
			}

			public static class Paladin
			{
				public static List<string> All => new List<string> {Avenge, CompetitiveSpirit, EyeForAnEye, GetawayKodo, NobleSacrifice, Redemption, Repentance, SacredTrial};
				public static string Avenge => HearthDb.CardIds.Collectible.Paladin.Avenge;
				public static string CompetitiveSpirit => HearthDb.CardIds.Collectible.Paladin.CompetitiveSpirit;
				public static string EyeForAnEye => HearthDb.CardIds.Collectible.Paladin.EyeForAnEye;
				public static string GetawayKodo => HearthDb.CardIds.Collectible.Paladin.GetawayKodo;
				public static string NobleSacrifice => HearthDb.CardIds.Collectible.Paladin.NobleSacrifice;
				public static string Redemption => HearthDb.CardIds.Collectible.Paladin.Redemption;
				public static string Repentance => HearthDb.CardIds.Collectible.Paladin.Repentance;
				public static string SacredTrial => HearthDb.CardIds.Collectible.Paladin.SacredTrial;

				public static List<string> GetCards(bool standardOnly) =>
					standardOnly ? All.Where(x => !Helper.WildOnlySets.Contains(Database.GetCardFromId(x).Set)).ToList() : All;
			}
		}
	}
}
