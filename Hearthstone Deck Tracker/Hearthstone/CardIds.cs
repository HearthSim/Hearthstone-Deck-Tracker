#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HearthDb.Enums;
using static HearthDb.CardIds;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class CardIds
	{
		public const int KeyMasterAlabasterDbfId = 61174;
		public const int SuspiciousMysteryDbfId = 75867;

		public static List<string> HiddenCardidPrefixes = new List<string>()
		{
			"PVPDR_TEST_"
		};

		public static Dictionary<string, string> UpgradeOverrides = new Dictionary<string, string>
		{
			[NonCollectible.Druid.LivingSeedRank1_LivingSeedRank2Token] = Collectible.Druid.LivingSeedRank1,
			[NonCollectible.Druid.LivingSeedRank1_LivingSeedRank3Token] = Collectible.Druid.LivingSeedRank1,
			[NonCollectible.Hunter.TameBeastRank1_TameBeastRank2Token] = Collectible.Hunter.TameBeastRank1,
			[NonCollectible.Hunter.TameBeastRank1_TameBeastRank3Token] = Collectible.Hunter.TameBeastRank1,
			[NonCollectible.Shaman.ChainLightningRank1_ChainLightningRank2Token] = Collectible.Shaman.ChainLightningRank1,
			[NonCollectible.Shaman.ChainLightningRank1_ChainLightningRank3Token] = Collectible.Shaman.ChainLightningRank1,
			[NonCollectible.Rogue.WickedStabRank1_WickedStabRank2Token] = Collectible.Rogue.WickedStabRank1,
			[NonCollectible.Rogue.WickedStabRank1_WickedStabRank3Token] = Collectible.Rogue.WickedStabRank1,
			[NonCollectible.Demonhunter.FuryRank1_FuryRank2Token] = Collectible.Demonhunter.FuryRank1,
			[NonCollectible.Demonhunter.FuryRank1_FuryRank3Token] = Collectible.Demonhunter.FuryRank1,
			[NonCollectible.Mage.FlurryRank1_FlurryRank2Token] = Collectible.Mage.FlurryRank1,
			[NonCollectible.Mage.FlurryRank1_FlurryRank3Token] = Collectible.Mage.FlurryRank1,
			[NonCollectible.Paladin.ConvictionRank1_ConvictionRank2Token] = Collectible.Paladin.ConvictionRank1,
			[NonCollectible.Paladin.ConvictionRank1_ConvictionRank3Token] = Collectible.Paladin.ConvictionRank1,
			[NonCollectible.Priest.CondemnRank1_CondemnRank2Token] = Collectible.Priest.CondemnRank1,
			[NonCollectible.Priest.CondemnRank1_CondemnRank3Token] = Collectible.Priest.CondemnRank1,
			[NonCollectible.Warlock.ImpSwarmRank1_ImpSwarmRank2Token] = Collectible.Warlock.ImpSwarmRank1,
			[NonCollectible.Warlock.ImpSwarmRank1_ImpSwarmRank3Token] = Collectible.Warlock.ImpSwarmRank1,
			[NonCollectible.Warrior.ConditioningRank1_ConditioningRank2Token] = Collectible.Warrior.ConditioningRank1,
			[NonCollectible.Warrior.ConditioningRank1_ConditioningRank3Token] = Collectible.Warrior.ConditioningRank1,
		};

		// todo: conditional deathrattle summons: Voidcaller
		public static readonly Dictionary<string, int> DeathrattleSummonCardIds = new Dictionary<string, int>
		{
			{Collectible.Druid.MountedRaptor, 1},
			{Collectible.Hunter.InfestedWolf, 2},
			{Collectible.Hunter.KindlyGrandmother, 1},
			{Collectible.Hunter.RatPack, 2},
			{Collectible.Hunter.SavannahHighmane, 2},
			{Collectible.Rogue.AnubarakTGT, 1},
			{Collectible.Rogue.JadeSwarmerGANGS, 1},
			{Collectible.Warlock.Dreadsteed, 1},
			{Collectible.Warlock.PossessedVillager, 1},
			{Collectible.Warlock.Voidcaller, 1}, //false negative better than false positive
			{Collectible.Neutral.AyaBlackpawGANGS, 1},
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
			{Collectible.Neutral.SludgeBelcherNAXX, 1},
			{Collectible.Neutral.SneedsOldShredder, 1},
			{Collectible.Neutral.TwilightSummoner, 1},
			{Collectible.Neutral.WobblingRunts, 3},
		};

		public static readonly Dictionary<string, string> HeroIdDict = new Dictionary<string, string>
		{
			{Collectible.Deathknight.TheLichKingHeroHeroSkins, "Deathknight"},
			{Collectible.Demonhunter.IllidanStormrageHeroHeroSkins, "DemonHunter"},
			{Collectible.Warrior.GarroshHellscreamHeroHeroSkins, "Warrior"},
			{Collectible.Shaman.ThrallHeroHeroSkins, "Shaman"},
			{Collectible.Rogue.ValeeraSanguinarHeroHeroSkins, "Rogue"},
			{Collectible.Paladin.UtherLightbringerHeroHeroSkins, "Paladin"},
			{Collectible.Hunter.RexxarHeroHeroSkins, "Hunter"},
			{Collectible.Druid.MalfurionStormrageHeroHeroSkins, "Druid"},
			{Collectible.Warlock.GuldanHeroHeroSkins, "Warlock"},
			{Collectible.Mage.JainaProudmooreHeroHeroSkins, "Mage"},
			{Collectible.Priest.AnduinWrynnHeroHeroSkins, "Priest"},
			{Collectible.Warlock.LordJaraxxus, "Jaraxxus"},
			{Collectible.Neutral.MajordomoExecutus, "Ragnaros the Firelord"},
			{"GILA_600", "Warrior"},
			{"GILA_500", "Rogue"},
			{"GILA_400", "Hunter"},
			{"GILA_900", "Mage"},
		};

		public static readonly Dictionary<string, string> HeroNameDict = new Dictionary<string, string>
		{
			{"Warrior", Collectible.Warrior.GarroshHellscreamHeroHeroSkins},
			{"Shaman", Collectible.Shaman.ThrallHeroHeroSkins},
			{"Rogue", Collectible.Rogue.ValeeraSanguinarHeroHeroSkins},
			{"Paladin", Collectible.Paladin.UtherLightbringerHeroHeroSkins},
			{"Hunter", Collectible.Hunter.RexxarHeroHeroSkins},
			{"Druid", Collectible.Druid.MalfurionStormrageHeroHeroSkins},
			{"Warlock", Collectible.Warlock.GuldanHeroHeroSkins},
			{"Mage", Collectible.Mage.JainaProudmooreHeroHeroSkins},
			{"Priest", Collectible.Priest.AnduinWrynnHeroHeroSkins},
			{"DemonHunter", Collectible.Demonhunter.IllidanStormrageHeroHeroSkins},
			{"Deathknight", Collectible.Deathknight.TheLichKingHeroHeroSkins},
		};

		public static readonly Dictionary<CardClass, string> CardClassHero = new Dictionary<CardClass, string>
		{
			{CardClass.WARRIOR, Collectible.Warrior.GarroshHellscreamHeroHeroSkins},
			{CardClass.SHAMAN, Collectible.Shaman.ThrallHeroHeroSkins},
			{CardClass.ROGUE, Collectible.Rogue.ValeeraSanguinarHeroHeroSkins},
			{CardClass.PALADIN, Collectible.Paladin.UtherLightbringerHeroHeroSkins},
			{CardClass.HUNTER, Collectible.Hunter.RexxarHeroHeroSkins},
			{CardClass.DRUID, Collectible.Druid.MalfurionStormrageHeroHeroSkins},
			{CardClass.WARLOCK, Collectible.Warlock.GuldanHeroHeroSkins},
			{CardClass.MAGE, Collectible.Mage.JainaProudmooreHeroHeroSkins},
			{CardClass.PRIEST, Collectible.Priest.AnduinWrynnHeroHeroSkins},
			{CardClass.DEMONHUNTER, Collectible.Demonhunter.IllidanStormrageHeroHeroSkins},
			{CardClass.DEATHKNIGHT, Collectible.Deathknight.TheLichKingHeroHeroSkins}
		};

		public static readonly Dictionary<string, string[]> DuelsHeroNameClass = new Dictionary<string, string[]>
		{
			{"Star Student Stelina", new string[]{"DemonHunter"}},
			{"Forest Warden Omu", new string[]{"Druid"}},
			{"Elise Starseeker", new string[]{"Druid", "Priest"}},
			{"Professor Slate", new string[]{"Hunter"}},
			{"Brann Bronzebeard", new string[]{"Hunter", "Warrior"}},
			{"Mozaki, Master Duelist", new string[]{"Mage"}},
			{"Reno Jackson", new string[]{"Mage", "Rogue"}},
			{"Turalyon, the Tenured", new string[]{"Paladin"}},
			{"Sir Finley", new string[]{"Paladin", "Shaman"}},
			{"Mindrender Illucia", new string[]{"Priest"}},
			{"Infiltrator Lilian", new string[]{"Rogue"}},
			{"Instructor Fireheart", new string[]{"Shaman"}},
			{"Archwitch Willow", new string[]{"Warlock"}},
			{"Darius Crowley", new string[]{"Warrior"}},
			{"Rattlegore", new string[]{"Warrior"}},
			{"Drek'Thar", new string[]{"Neutral"}},
			{"Vanndar Stormpike", new string[]{"Neutral"}},
			{"Diablo", new string[]{ "Warlock", "Warrior"}},
		};

		public static class Secrets
		{
			public static readonly IReadOnlyList<MultiIdCard> FastCombat = new List<MultiIdCard>
			{
				Hunter.FreezingTrap,
				Hunter.ExplosiveTrap,
				Hunter.Misdirection,
				Paladin.NobleSacrifice,
				Mage.Vaporize
			};

			public static IReadOnlyList<MultiIdCard> All { get; } = Hunter.All.Concat(Mage.All).Concat(Paladin.All).Concat(Rogue.All).ToList();

			public static MultiIdCard? GetSecretMultiIdCard(string id) =>
				All.FirstOrDefault(m => m.Ids.Contains(id));

			public class Hunter : EnumerateMultiId<Hunter>
			{
				public static readonly MultiIdCard BaitAndSwitch = new MultiIdCard(Collectible.Hunter.BaitAndSwitch);
				public static readonly MultiIdCard BargainBin = new MultiIdCard(Collectible.Hunter.BargainBin);
				public static readonly MultiIdCard BearTrap = new MultiIdCard(Collectible.Hunter.BearTrap);
				public static readonly MultiIdCard CatTrick = new MultiIdCard(Collectible.Hunter.CatTrick, Collectible.Hunter.CatTrickCorePlaceholder);
				public static readonly MultiIdCard DartTrap = new MultiIdCard(Collectible.Hunter.DartTrap);
				public static readonly MultiIdCard ExplosiveTrap = new MultiIdCard(Collectible.Hunter.ExplosiveTrap, Collectible.Hunter.ExplosiveTrapCore, Collectible.Hunter.ExplosiveTrapVanilla);
				public static readonly MultiIdCard EmergencyManeuvers = new MultiIdCard(Collectible.Hunter.EmergencyManeuvers);
				public static readonly MultiIdCard FreezingTrap = new MultiIdCard(Collectible.Hunter.FreezingTrap, Collectible.Hunter.FreezingTrapCore, Collectible.Hunter.FreezingTrapVanilla);
				public static readonly MultiIdCard HiddenCache = new MultiIdCard(Collectible.Hunter.HiddenCache);
				public static readonly MultiIdCard HiddenMeaning = new MultiIdCard(Collectible.Hunter.HiddenMeaning);
				public static readonly MultiIdCard IceTrap = new MultiIdCard(Collectible.Hunter.IceTrap, Collectible.Hunter.IceTrapCore);
				public static readonly MultiIdCard Misdirection = new MultiIdCard(Collectible.Hunter.Misdirection, Collectible.Hunter.MisdirectionVanilla);
				public static readonly MultiIdCard MotionDenied = new MultiIdCard(Collectible.Hunter.MotionDenied);
				public static readonly MultiIdCard OpenTheCages = new MultiIdCard(Collectible.Hunter.OpenTheCages);
				public static readonly MultiIdCard PackTactics = new MultiIdCard(Collectible.Hunter.PackTactics);
				public static readonly MultiIdCard PressurePlate = new MultiIdCard(Collectible.Hunter.PressurePlate);
				public static readonly MultiIdCard RatTrap = new MultiIdCard(Collectible.Hunter.RatTrap, Collectible.Hunter.RatTrapCore);
				public static readonly MultiIdCard SnakeTrap = new MultiIdCard(Collectible.Hunter.SnakeTrap, Collectible.Hunter.SnakeTrapCorePlaceholder, Collectible.Hunter.SnakeTrapVanilla);
				public static readonly MultiIdCard Snipe = new MultiIdCard(Collectible.Hunter.SnipeExpert1, Collectible.Hunter.SnipeVanilla, Collectible.Hunter.SnipeWONDERS);
				public static readonly MultiIdCard VenomstrikeTrap = new MultiIdCard(Collectible.Hunter.VenomstrikeTrap, Collectible.Hunter.VenomstrikeTrapCorePlaceholder);
				public static readonly MultiIdCard WanderingMonster = new MultiIdCard(Collectible.Hunter.WanderingMonster, Collectible.Hunter.WanderingMonsterCorePlaceholder);
				public static readonly MultiIdCard Zombeeees = new MultiIdCard(Collectible.Hunter.Zombeeees);
			}

			public class Mage : EnumerateMultiId<Mage>
			{
				public static readonly MultiIdCard AzeriteVein = new MultiIdCard(Collectible.Mage.AzeriteVein);
				public static readonly MultiIdCard Counterspell = new MultiIdCard(Collectible.Mage.Counterspell, Collectible.Mage.CounterspellCore, Collectible.Mage.CounterspellVanilla);
				public static readonly MultiIdCard Duplicate = new MultiIdCard(Collectible.Mage.Duplicate);
				public static readonly MultiIdCard Effigy = new MultiIdCard(Collectible.Mage.Effigy);
				public static readonly MultiIdCard ExplosiveRunes = new MultiIdCard(Collectible.Mage.ExplosiveRunes, Collectible.Mage.ExplosiveRunesCore);
				public static readonly MultiIdCard FlameWard = new MultiIdCard(Collectible.Mage.FlameWard);
				public static readonly MultiIdCard FrozenClone = new MultiIdCard(Collectible.Mage.FrozenClone, Collectible.Mage.FrozenCloneCorePlaceholder);
				public static readonly MultiIdCard IceBarrier = new MultiIdCard(Collectible.Mage.IceBarrier, Collectible.Mage.IceBarrierCore, Collectible.Mage.IceBarrierVanilla);
				public static readonly MultiIdCard IceBlock = new MultiIdCard(Collectible.Mage.IceBlock, Collectible.Mage.IceBlockVanilla);
				public static readonly MultiIdCard ManaBind = new MultiIdCard(Collectible.Mage.ManaBind);
				public static readonly MultiIdCard MirrorEntity = new MultiIdCard(Collectible.Mage.MirrorEntity, Collectible.Mage.MirrorEntityCorePlaceholder, Collectible.Mage.MirrorEntityVanilla);
				public static readonly MultiIdCard NetherwindPortal = new MultiIdCard(Collectible.Mage.NetherwindPortal);
				public static readonly MultiIdCard OasisAlly = new MultiIdCard(Collectible.Mage.OasisAlly, Collectible.Mage.OasisAllyCore);
				public static readonly MultiIdCard Objection = new MultiIdCard(Collectible.Mage.Objection);
				public static readonly MultiIdCard PotionOfPolymorph = new MultiIdCard(Collectible.Mage.PotionOfPolymorph);
				public static readonly MultiIdCard RiggedFaireGame = new MultiIdCard(Collectible.Mage.RiggedFaireGame);
				public static readonly MultiIdCard Spellbender = new MultiIdCard(Collectible.Mage.Spellbender, Collectible.Mage.SpellbenderVanilla);
				public static readonly MultiIdCard SplittingImage = new MultiIdCard(Collectible.Mage.SplittingImage);
				public static readonly MultiIdCard Vaporize = new MultiIdCard(Collectible.Mage.Vaporize, Collectible.Mage.VaporizeVanilla);
				public static readonly MultiIdCard VengefulVisage = new MultiIdCard(Collectible.Mage.VengefulVisage);
				public static readonly MultiIdCard SummoningWard = new MultiIdCard(Collectible.Mage.SummoningWard);
			}

			public class Paladin : EnumerateMultiId<Paladin>
			{
				public static readonly MultiIdCard AutodefenseMatrix = new MultiIdCard(Collectible.Paladin.AutodefenseMatrix);
				public static readonly MultiIdCard Avenge = new MultiIdCard(Collectible.Paladin.Avenge, Collectible.Paladin.AvengeCorePlaceholder);
				public static readonly MultiIdCard CompetitiveSpirit = new MultiIdCard(Collectible.Paladin.CompetitiveSpirit);
				public static readonly MultiIdCard EyeForAnEye = new MultiIdCard(Collectible.Paladin.EyeForAnEye, Collectible.Paladin.EyeForAnEyeVanilla);
				public static readonly MultiIdCard GetawayKodo = new MultiIdCard(Collectible.Paladin.GetawayKodo);
				public static readonly MultiIdCard GallopingSavior = new MultiIdCard(Collectible.Paladin.GallopingSavior);
				public static readonly MultiIdCard HandOfSalvation = new MultiIdCard(NonCollectible.Paladin.HandOfSalvationLegacy);
				public static readonly MultiIdCard HiddenWisdom = new MultiIdCard(Collectible.Paladin.HiddenWisdom);
				public static readonly MultiIdCard NeverSurrender = new MultiIdCard(Collectible.Paladin.NeverSurrender);
				public static readonly MultiIdCard NobleSacrifice = new MultiIdCard(Collectible.Paladin.NobleSacrifice, Collectible.Paladin.NobleSacrificeCorePlaceholder, Collectible.Paladin.NobleSacrificeVanilla);
				public static readonly MultiIdCard OhMyYogg = new MultiIdCard(Collectible.Paladin.OhMyYogg);
				public static readonly MultiIdCard Reckoning = new MultiIdCard(Collectible.Paladin.ReckoningLegacy, Collectible.Paladin.ReckoningCorePlaceholder);
				public static readonly MultiIdCard Redemption = new MultiIdCard(Collectible.Paladin.Redemption, Collectible.Paladin.RedemptionVanilla);
				public static readonly MultiIdCard Repentance = new MultiIdCard(Collectible.Paladin.Repentance, Collectible.Paladin.RepentanceVanilla);
				public static readonly MultiIdCard SacredTrial = new MultiIdCard(Collectible.Paladin.SacredTrial);
				public static readonly MultiIdCard JudgmentOfJustice = new MultiIdCard(Collectible.Paladin.JudgmentOfJustice);
			}

			public class Rogue : EnumerateMultiId<Rogue>
			{
				public static readonly MultiIdCard Ambush = new MultiIdCard(Collectible.Rogue.Ambush, Collectible.Rogue.AmbushCorePlaceholder);
				public static readonly MultiIdCard Bamboozle = new MultiIdCard(Collectible.Rogue.Bamboozle);
				public static readonly MultiIdCard CheatDeath = new MultiIdCard(Collectible.Rogue.CheatDeath, Collectible.Rogue.Core2024CheatDeathCorePlaceholder);
				public static readonly MultiIdCard DirtyTricks = new MultiIdCard(Collectible.Rogue.DirtyTricks);
				public static readonly MultiIdCard DoubleCross = new MultiIdCard(Collectible.Rogue.DoubleCross);
				public static readonly MultiIdCard Evasion = new MultiIdCard(Collectible.Rogue.Evasion);
				public static readonly MultiIdCard Kidnap = new MultiIdCard(Collectible.Rogue.Kidnap);
				public static readonly MultiIdCard Perjury = new MultiIdCard(Collectible.Rogue.Perjury);
				public static readonly MultiIdCard Plagiarize = new MultiIdCard(Collectible.Rogue.Plagiarize, Collectible.Rogue.PlagiarizeCorePlaceholder);
				public static readonly MultiIdCard ShadowClone = new MultiIdCard(Collectible.Rogue.ShadowClone);
				public static readonly MultiIdCard Shenanigans = new MultiIdCard(Collectible.Rogue.Shenanigans);
				public static readonly MultiIdCard StickySituation = new MultiIdCard(Collectible.Rogue.StickySituation);
				public static readonly MultiIdCard SuddenBetrayal = new MultiIdCard(Collectible.Rogue.SuddenBetrayal);
			}
		}
	}
}

public class EnumerateMultiId<T> where T: EnumerateMultiId<T>
{
	private static IReadOnlyList<MultiIdCard>? _all = null;
	public static IReadOnlyList<MultiIdCard> All
	{
		get
		{
			if (_all == null)
			{
				_all = typeof(T)
					.GetFields(BindingFlags.Public | BindingFlags.Static)
					.Where(x => x.FieldType == typeof(MultiIdCard))
					.Select(x => x.GetValue(null))
					.Cast<MultiIdCard>()
					.ToList();
			}
			return _all;
		}
	}
}
