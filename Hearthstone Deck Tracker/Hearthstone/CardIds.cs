#region

using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using static HearthDb.CardIds;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class CardIds
	{
		// todo: conditional deathrattle summons: Voidcaller
		public static readonly Dictionary<string, int> DeathrattleSummonCardIds = new Dictionary<string, int>
		{
			{Collectible.Druid.MountedRaptor, 1},
			{Collectible.Hunter.InfestedWolf, 2},
			{Collectible.Hunter.KindlyGrandmother, 1},
			{Collectible.Hunter.RatPack, 2},
			{Collectible.Hunter.SavannahHighmane, 2},
			{Collectible.Rogue.Anubarak, 1},
			{Collectible.Rogue.JadeSwarmer, 1},
			{Collectible.Warlock.Dreadsteed, 1},
			{Collectible.Warlock.PossessedVillager, 1},
			{Collectible.Warlock.Voidcaller, 1}, //false negative better than false positive
			{Collectible.Neutral.AyaBlackpaw, 1},
			{Collectible.Neutral.CairneBloodhoof, 1},
			{Collectible.Neutral.DevilsaurEgg, 1},
			{Collectible.Neutral.Eggnapper, 2},
			{Collectible.Neutral.HarvestGolem, 1},
			{Collectible.Neutral.HauntedCreeper, 2},
			{Collectible.Neutral.InfestedTauren, 1},
			{Collectible.Neutral.NerubianEgg, 1},
			{Collectible.Neutral.PilotedShredder, 1},
			{Collectible.Neutral.PilotedSkyGolem, 1},
			{Collectible.Neutral.SatedThreshadon, 3},
			{Collectible.Neutral.SludgeBelcher, 1},
			{Collectible.Neutral.SneedsOldShredder, 1},
			{Collectible.Neutral.TwilightSummoner, 1},
			{Collectible.Neutral.WobblingRunts, 3},
		};

		public static readonly Dictionary<string, string> HeroIdDict = new Dictionary<string, string>
		{
			{Collectible.Warrior.GarroshHellscreamHero, "Warrior"},
			{Collectible.Shaman.ThrallHero, "Shaman"},
			{Collectible.Rogue.ValeeraSanguinarHero, "Rogue"},
			{Collectible.Paladin.UtherLightbringerHero, "Paladin"},
			{Collectible.Hunter.RexxarHero, "Hunter"},
			{Collectible.Druid.MalfurionStormrageHero, "Druid"},
			{Collectible.Warlock.GuldanHero, "Warlock"},
			{Collectible.Mage.JainaProudmooreHero, "Mage"},
			{Collectible.Priest.AnduinWrynnHero, "Priest"},
			{Collectible.Warlock.LordJaraxxus, "Jaraxxus"},
			{Collectible.Neutral.MajordomoExecutus, "Ragnaros the Firelord"},
			{"GILA_600", "Warrior"},
			{"GILA_500", "Rogue"},
			{"GILA_400", "Hunter"},
			{"GILA_900", "Mage"},
		};

		public static readonly Dictionary<string, string> HeroNameDict = new Dictionary<string, string>
		{
			{"Warrior", Collectible.Warrior.GarroshHellscreamHero},
			{"Shaman", Collectible.Shaman.ThrallHero},
			{"Rogue", Collectible.Rogue.ValeeraSanguinarHero},
			{"Paladin", Collectible.Paladin.UtherLightbringerHero},
			{"Hunter", Collectible.Hunter.RexxarHero},
			{"Druid", Collectible.Druid.MalfurionStormrageHero},
			{"Warlock", Collectible.Warlock.GuldanHero},
			{"Mage", Collectible.Mage.JainaProudmooreHero},
			{"Priest", Collectible.Priest.AnduinWrynnHero}
		};

		public static readonly Dictionary<CardClass, string> CardClassHero = new Dictionary<CardClass, string>
		{
			{CardClass.WARRIOR, Collectible.Warrior.GarroshHellscreamHero},
			{CardClass.SHAMAN, Collectible.Shaman.ThrallHero},
			{CardClass.ROGUE, Collectible.Rogue.ValeeraSanguinarHero},
			{CardClass.PALADIN, Collectible.Paladin.UtherLightbringerHero},
			{CardClass.HUNTER, Collectible.Hunter.RexxarHero},
			{CardClass.DRUID, Collectible.Druid.MalfurionStormrageHero},
			{CardClass.WARLOCK, Collectible.Warlock.GuldanHero},
			{CardClass.MAGE, Collectible.Mage.JainaProudmooreHero},
			{CardClass.PRIEST, Collectible.Priest.AnduinWrynnHero}
		};

		// cards that should have an entourage list but don't in the game data
		public static readonly Dictionary<string, string[]> EntourageAdditionalCardIds = new Dictionary<string, string[]>
		{
			{Collectible.Shaman.KalimosPrimalLord,
				new string[]
				{
					NonCollectible.Shaman.KalimosPrimalLord_InvocationOfAir,
					NonCollectible.Shaman.KalimosPrimalLord_InvocationOfEarth,
					NonCollectible.Shaman.KalimosPrimalLord_InvocationOfFire,
					NonCollectible.Shaman.KalimosPrimalLord_InvocationOfWater
				}
			}
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
				public static List<string> All => new List<string> {BearTrap, CatTrick, DartTrap, ExplosiveTrap, FreezingTrap, HiddenCache, Misdirection, PressurePlate, RatTrap, Snipe, SnakeTrap, VenomstrikeTrap, WanderingMonster};
				public static string BearTrap => Collectible.Hunter.BearTrap;
				public static string CatTrick => Collectible.Hunter.CatTrick;
				public static string DartTrap => Collectible.Hunter.DartTrap;
				public static string ExplosiveTrap => Collectible.Hunter.ExplosiveTrap;
				public static string FreezingTrap => Collectible.Hunter.FreezingTrap;
				public static string HiddenCache => Collectible.Hunter.HiddenCache;
				public static string Misdirection => Collectible.Hunter.Misdirection;
				public static string PressurePlate => Collectible.Hunter.PressurePlate;
				public static string RatTrap => Collectible.Hunter.RatTrap;
				public static string Snipe => Collectible.Hunter.Snipe;
				public static string SnakeTrap => Collectible.Hunter.SnakeTrap;
				public static string VenomstrikeTrap => Collectible.Hunter.VenomstrikeTrap;
				public static string WanderingMonster => Collectible.Hunter.WanderingMonster;
			}

			public static class Mage
			{
				public static List<string> All => new List<string> {Counterspell, Duplicate, Effigy, ExplosiveRunes, FlameWard, FrozenClone, IceBarrier, IceBlock, ManaBind, MirrorEntity, PotionOfPolymorph, Spellbender, SplittingImage, Vaporize};
				public static string Counterspell => Collectible.Mage.Counterspell;
				public static string Duplicate => Collectible.Mage.Duplicate;
				public static string Effigy => Collectible.Mage.Effigy;
				public static string ExplosiveRunes => Collectible.Mage.ExplosiveRunes;
				public static string FlameWard => Collectible.Mage.FlameWard;
				public static string FrozenClone => Collectible.Mage.FrozenClone;
				public static string IceBarrier => Collectible.Mage.IceBarrier;
				public static string IceBlock => Collectible.Mage.IceBlock;
				public static string ManaBind => Collectible.Mage.ManaBind;
				public static string MirrorEntity => Collectible.Mage.MirrorEntity;
				public static string PotionOfPolymorph => Collectible.Mage.PotionOfPolymorph;
				public static string Spellbender => Collectible.Mage.Spellbender;
				public static string SplittingImage => Collectible.Mage.SplittingImage;
				public static string Vaporize => Collectible.Mage.Vaporize;
			}

			public static class Paladin
			{
				public static List<string> All => new List<string> {AutodefenseMatrix, Avenge, CompetitiveSpirit, EyeForAnEye, GetawayKodo, HiddenWisdom, HandOfSalvation, NeverSurrender, NobleSacrifice, Redemption, Repentance, SacredTrial};
				public static string AutodefenseMatrix => Collectible.Paladin.AutodefenseMatrix;
				public static string Avenge => Collectible.Paladin.Avenge;
				public static string CompetitiveSpirit => Collectible.Paladin.CompetitiveSpirit;
				public static string EyeForAnEye => Collectible.Paladin.EyeForAnEye;
				public static string GetawayKodo => Collectible.Paladin.GetawayKodo;
				public static string HandOfSalvation => NonCollectible.Paladin.HandOfSalvation;
				public static string HiddenWisdom => Collectible.Paladin.HiddenWisdom;
				public static string NeverSurrender => Collectible.Paladin.NeverSurrender;
				public static string NobleSacrifice => Collectible.Paladin.NobleSacrifice;
				public static string Redemption => Collectible.Paladin.Redemption;
				public static string Repentance => Collectible.Paladin.Repentance;
				public static string SacredTrial => Collectible.Paladin.SacredTrial;
			}

			public static class Rogue
			{
				public static List<string> All => new List<string> {CheatDeath, Evasion, SuddenBetrayal};
				public static string CheatDeath => Collectible.Rogue.CheatDeath;
				public static string Evasion => Collectible.Rogue.Evasion;
				public static string SuddenBetrayal => Collectible.Rogue.SuddenBetrayal;
			}
		}
	}
}
