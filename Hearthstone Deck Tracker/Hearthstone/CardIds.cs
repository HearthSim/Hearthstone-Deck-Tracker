#region

using System.Collections.Generic;
using System.Linq;
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
			{Collectible.Warrior.GarroshHellscream, "Warrior"},
			{Collectible.Shaman.Thrall, "Shaman"},
			{Collectible.Rogue.ValeeraSanguinar, "Rogue"},
			{Collectible.Paladin.UtherLightbringer, "Paladin"},
			{Collectible.Hunter.Rexxar, "Hunter"},
			{Collectible.Druid.MalfurionStormrage, "Druid"},
			{Collectible.Warlock.Guldan, "Warlock"},
			{Collectible.Mage.JainaProudmoore, "Mage"},
			{Collectible.Priest.AnduinWrynn, "Priest"},
			{Collectible.Warlock.LordJaraxxus, "Jaraxxus"},
			{Collectible.Neutral.MajordomoExecutus, "Ragnaros the Firelord"},
			{"GILA_600", "Warrior"},
			{"GILA_500", "Rogue"},
			{"GILA_400", "Hunter"},
			{"GILA_900", "Mage"},
		};

		public static readonly Dictionary<string, string> HeroNameDict = new Dictionary<string, string>
		{
			{"Warrior", Collectible.Warrior.GarroshHellscream},
			{"Shaman", Collectible.Shaman.Thrall},
			{"Rogue", Collectible.Rogue.ValeeraSanguinar},
			{"Paladin", Collectible.Paladin.UtherLightbringer},
			{"Hunter", Collectible.Hunter.Rexxar},
			{"Druid", Collectible.Druid.MalfurionStormrage},
			{"Warlock", Collectible.Warlock.Guldan},
			{"Mage", Collectible.Mage.JainaProudmoore},
			{"Priest", Collectible.Priest.AnduinWrynn}
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
			public static List<string> ArenaExcludes = new List<string>
			{
				Hunter.Snipe
			};

			public static List<string> ArenaOnly = new List<string>
			{
				Paladin.HandOfSalvation
			};

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
				public static List<string> All => new List<string> {BearTrap, CatTrick, DartTrap, ExplosiveTrap, FreezingTrap, HiddenCache, Misdirection, RatTrap, Snipe, SnakeTrap, VenomstrikeTrap, WanderingMonster};
				public static string BearTrap => Collectible.Hunter.BearTrap;
				public static string CatTrick => Collectible.Hunter.CatTrick;
				public static string DartTrap => Collectible.Hunter.DartTrap;
				public static string ExplosiveTrap => Collectible.Hunter.ExplosiveTrap;
				public static string FreezingTrap => Collectible.Hunter.FreezingTrap;
				public static string HiddenCache => Collectible.Hunter.HiddenCache;
				public static string Misdirection => Collectible.Hunter.Misdirection;
				public static string RatTrap => Collectible.Hunter.RatTrap;
				public static string Snipe => Collectible.Hunter.Snipe;
				public static string SnakeTrap => Collectible.Hunter.SnakeTrap;
				public static string VenomstrikeTrap => Collectible.Hunter.VenomstrikeTrap;
				public static string WanderingMonster => Collectible.Hunter.WanderingMonster;
			}

			public static class Mage
			{
				public static List<string> All => new List<string> {Counterspell, Duplicate, Effigy, ExplosiveRunes, FrozenClone, IceBarrier, IceBlock, ManaBind, MirrorEntity, PotionOfPolymorph, Spellbender, Vaporize};
				public static string Counterspell => Collectible.Mage.Counterspell;
				public static string Duplicate => Collectible.Mage.Duplicate;
				public static string Effigy => Collectible.Mage.Effigy;
				public static string ExplosiveRunes => Collectible.Mage.ExplosiveRunes;
				public static string FrozenClone => Collectible.Mage.FrozenClone;
				public static string IceBarrier => Collectible.Mage.IceBarrier;
				public static string IceBlock => Collectible.Mage.IceBlock;
				public static string ManaBind => Collectible.Mage.ManaBind;
				public static string MirrorEntity => Collectible.Mage.MirrorEntity;
				public static string PotionOfPolymorph => Collectible.Mage.PotionOfPolymorph;
				public static string Spellbender => Collectible.Mage.Spellbender;
				public static string Vaporize => Collectible.Mage.Vaporize;
			}

			public static class Paladin
			{
				public static List<string> All => new List<string> {Avenge, CompetitiveSpirit, EyeForAnEye, GetawayKodo, HiddenWisdom, HandOfSalvation, NobleSacrifice, Redemption, Repentance, SacredTrial};
				public static string Avenge => Collectible.Paladin.Avenge;
				public static string CompetitiveSpirit => Collectible.Paladin.CompetitiveSpirit;
				public static string EyeForAnEye => Collectible.Paladin.EyeForAnEye;
				public static string GetawayKodo => Collectible.Paladin.GetawayKodo;
				public static string HandOfSalvation => NonCollectible.Paladin.HandOfSalvation;
				public static string HiddenWisdom => Collectible.Paladin.HiddenWisdom;
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
