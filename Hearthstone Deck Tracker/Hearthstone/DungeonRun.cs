using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using static HearthDb.CardIds;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class DungeonRun
	{
		public static Deck? GetDefaultDeck(string playerClass, CardSet set, string shrineCardId = "")
		{
			var cards = GetCards(playerClass, set, shrineCardId);
			if(cards == null)
				return null;
			return GetDeck(playerClass, set, false, cards.Select(Database.GetCardFromId).WhereNotNull());
		}

		public static Deck? GetDeckFromDbfIds(string playerClass, CardSet set, bool isPVPDR, IEnumerable<int> dbfIds)
		{
			return GetDeck(playerClass, set, isPVPDR, dbfIds.Select(dbfId => Database.GetCardFromDbfId(dbfId)).WhereNotNull());
		}

		public static Deck? GetDeck(string playerClass, CardSet set, bool isPVPDR, IEnumerable<Card> cards)
		{
			var deck = new Deck
			{
				Cards = new ObservableCollection<Card>(cards),
				Class = playerClass.ToLower() == "demonhunter" ? "DemonHunter" : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(playerClass.ToLowerInvariant()),
				IsDuelsDeck = isPVPDR,
				IsDungeonDeck = !isPVPDR,
				LastEdited = DateTime.Now
			};
			var template = GetDeckTemplate(set);
			if(template == null)
				return null;
			deck.Name = Helper.ParseDeckNameTemplate(template, deck);
			return deck;
		}

		private static string? GetDeckTemplate(CardSet set)
		{
			switch(set)
			{
				case CardSet.LOOTAPALOOZA:
					return Config.Instance.DungeonRunDeckNameTemplate;
				case CardSet.GILNEAS:
					return Config.Instance.MonsterHuntDeckNameTemplate;
				case CardSet.TROLL:
					return Config.Instance.RumbleRunDeckNameTemplate;
				case CardSet.DALARAN:
					return Config.Instance.DalaranHeistDeckNameTemplate;
				case CardSet.ULDUM:
					return Config.Instance.TombsOfTerrorDeckNameTemplate;
				case CardSet.DARKMOON_FAIRE:
					return Config.Instance.PVPDungeonRunDeckNameTemplate;
				default:
					return null;
			}
		}

		private static List<string>? GetCards(string playerClass, CardSet set, string shrineCardId = "")
		{
			switch(set)
			{
				case CardSet.LOOTAPALOOZA:
				{
					switch(playerClass.ToUpperInvariant())
					{
						// Todo: Add Demon Hunter
						case "ROGUE":
							return LootDefaultDecks.Rogue;
						case "WARRIOR":
							return LootDefaultDecks.Warrior;
						case "SHAMAN":
							return LootDefaultDecks.Shaman;
						case "PALADIN":
							return LootDefaultDecks.Paladin;
						case "HUNTER":
							return LootDefaultDecks.Hunter;
						case "DRUID":
							return LootDefaultDecks.Druid;
						case "WARLOCK":
							return LootDefaultDecks.Warlock;
						case "MAGE":
							return LootDefaultDecks.Mage;
						case "PRIEST":
							return LootDefaultDecks.Priest;
					}
				}
					break;
				case CardSet.GILNEAS:
				{
					switch(playerClass.ToUpperInvariant())
					{
						// Todo: Add Demon Hunter
						case "ROGUE":
							return GilDefaultDecks.Rogue;
						case "WARRIOR":
							return GilDefaultDecks.Warrior;
						case "HUNTER":
							return GilDefaultDecks.Hunter;
						case "MAGE":
							return GilDefaultDecks.Mage;
					}
				}
					break;
				case CardSet.TROLL:
				{
					switch(playerClass.ToUpperInvariant())
					{
						// Todo: Add Demon Hunter
						case "ROGUE":
							return TrlDefaultDecks.Rogue.FirstOrDefault(x => x.Contains(shrineCardId));
						case "WARRIOR":
							return TrlDefaultDecks.Warrior.FirstOrDefault(x => x.Contains(shrineCardId));
						case "SHAMAN":
							return TrlDefaultDecks.Shaman.FirstOrDefault(x => x.Contains(shrineCardId));
						case "PALADIN":
							return TrlDefaultDecks.Paladin.FirstOrDefault(x => x.Contains(shrineCardId));
						case "HUNTER":
							return TrlDefaultDecks.Hunter.FirstOrDefault(x => x.Contains(shrineCardId));
						case "DRUID":
							return TrlDefaultDecks.Druid.FirstOrDefault(x => x.Contains(shrineCardId));
						case "WARLOCK":
							return TrlDefaultDecks.Warlock.FirstOrDefault(x => x.Contains(shrineCardId));
						case "MAGE":
							return TrlDefaultDecks.Mage.FirstOrDefault(x => x.Contains(shrineCardId));
						case "PRIEST":
							return TrlDefaultDecks.Priest.FirstOrDefault(x => x.Contains(shrineCardId));
					}
				}
					break;
			}
			return null;
		}

		public static string? GetUldumHeroPlayerClass(string? identifier)
		{
			switch(identifier)
			{
				// Todo: Add Demon Hunter
				case "Mage":
				case "Rogue":
					return "Mage";
				case "Paladin":
				case "Shaman":
					return "Paladin";
				case "Druid":
				case "Priest":
					return "Druid";
				case "Hunter":
				case "Warrior":
					return "Hunter";
				default:
					return null;
			}
		}

		public static bool IsDungeonBoss(string? cardId) => cardId != null && (cardId.Contains("LOOT") || cardId.Contains("GIL") || cardId.Contains("TRL")) && cardId.Contains("BOSS");

		private static class GilDefaultDecks
		{
			public static List<string> Rogue => new List<string>
			{
				Collectible.Neutral.ElvenArcherLegacy,
				Collectible.Rogue.SinisterStrikeLegacy,
				Collectible.Neutral.WorgenInfiltrator,
				Collectible.Neutral.BloodsailRaider,
				Collectible.Hunter.Glaivezooka,
				Collectible.Hunter.SnakeTrap,
				Collectible.Rogue.BlinkFox,
				Collectible.Rogue.FanOfKnivesLegacy,
				Collectible.Neutral.HiredGun,
				Collectible.Rogue.Si7Agent
			};

			public static List<string> Warrior => new List<string>
			{
				Collectible.Neutral.AbusiveSergeant,
				NonCollectible.Neutral.ExtraPowder,
				Collectible.Neutral.LowlySquire,
				Collectible.Neutral.AmaniBerserker,
				Collectible.Warrior.CruelTaskmaster,
				Collectible.Warrior.RedbandWasp,
				Collectible.Warrior.Bash,
				Collectible.Warrior.FierceMonkey,
				Collectible.Warrior.KingsDefender,
				Collectible.Warrior.BloodhoofBrave
			};

			public static List<string> Hunter => new List<string>
			{
				Collectible.Hunter.FieryBat,
				Collectible.Hunter.OnTheHunt,
				Collectible.Neutral.SwampLeech,
				Collectible.Hunter.CracklingRazormaw,
				Collectible.Hunter.Toxmonger_HuntingMastiffToken,
				Collectible.Hunter.ForlornStalker,
				Collectible.Hunter.KillCommandLegacy,
				Collectible.Hunter.UnleashTheHounds,
				Collectible.Hunter.HoundmasterLegacy,
				Collectible.Neutral.SwiftMessenger
			};

			public static List<string> Mage => new List<string>
			{
				Collectible.Mage.BabblingBook,
				Collectible.Neutral.MadBomber,
				Collectible.Mage.PrimordialGlyph,
				Collectible.Mage.UnstablePortal,
				Collectible.Mage.Cinderstorm,
				Collectible.Mage.Flamewaker,
				Collectible.Mage.SpellslingerTGT,
				Collectible.Neutral.TinkmasterOverspark,
				Collectible.Mage.WaterElementalLegacy,
				Collectible.Neutral.Blingtron3000
			};
		}

		private static class LootDefaultDecks
		{
			public static List<string> Rogue => new List<string>
			{
				Collectible.Rogue.BackstabLegacy,
				Collectible.Rogue.DeadlyPoisonLegacy,
				Collectible.Rogue.PitSnake,
				Collectible.Rogue.SinisterStrikeLegacy,
				Collectible.Neutral.GilblinStalker,
				Collectible.Rogue.UndercityHucksterOG,
				Collectible.Rogue.Si7Agent,
				Collectible.Rogue.UnearthedRaptor,
				Collectible.Rogue.AssassinateLegacy,
				Collectible.Rogue.VanishLegacy,
			};

			public static List<string> Warrior = new List<string>
			{
				Collectible.Warrior.Warbot,
				Collectible.Neutral.AmaniBerserker,
				Collectible.Warrior.CruelTaskmaster,
				Collectible.Warrior.HeroicStrikeLegacy,
				Collectible.Warrior.Bash,
				Collectible.Warrior.FieryWarAxeLegacy,
				Collectible.Neutral.HiredGun,
				Collectible.Neutral.RagingWorgen,
				Collectible.Neutral.DreadCorsair,
				Collectible.Warrior.Brawl,
			};

			public static List<string> Shaman = new List<string>
			{
				Collectible.Shaman.AirElemental,
				Collectible.Shaman.LightningBolt,
				Collectible.Shaman.FlametongueTotemLegacy,
				Collectible.Neutral.MurlocTidehunterLegacy,
				Collectible.Shaman.StormforgedAxe,
				Collectible.Shaman.LightningStorm,
				Collectible.Shaman.UnboundElemental,
				Collectible.Neutral.DefenderOfArgus,
				Collectible.Shaman.HexLegacy,
				Collectible.Shaman.FireElementalLegacy,
			};

			public static List<string> Paladin = new List<string>
			{
				Collectible.Paladin.BlessingOfMightLegacy,
				Collectible.Neutral.GoldshireFootmanLegacy,
				Collectible.Paladin.NobleSacrifice,
				Collectible.Paladin.ArgentProtector,
				Collectible.Paladin.Equality,
				Collectible.Paladin.HolyLightLegacy,
				Collectible.Neutral.EarthenRingFarseer,
				Collectible.Paladin.ConsecrationLegacy,
				Collectible.Neutral.StormwindKnightLegacy,
				Collectible.Paladin.TruesilverChampionLegacy,
			};

			public static List<string> Hunter = new List<string>
			{
				Collectible.Hunter.HuntersMarkLegacy,
				Collectible.Neutral.StonetuskBoarLegacy,
				Collectible.Neutral.DireWolfAlpha,
				Collectible.Hunter.ExplosiveTrap,
				Collectible.Hunter.AnimalCompanionLegacy,
				Collectible.Hunter.DeadlyShot,
				Collectible.Hunter.EaglehornBow,
				Collectible.Neutral.JunglePanther,
				Collectible.Hunter.UnleashTheHounds,
				Collectible.Neutral.OasisSnapjawLegacy,
			};

			public static List<string> Druid = new List<string>
			{
				Collectible.Druid.EnchantedRaven,
				Collectible.Druid.PowerOfTheWild,
				Collectible.Druid.TortollanForager,
				Collectible.Druid.MountedRaptor,
				Collectible.Druid.Mulch,
				Collectible.Neutral.ShadeOfNaxxramas,
				Collectible.Druid.KeeperOfTheGrove,
				Collectible.Druid.SavageCombatant,
				Collectible.Druid.SwipeLegacy,
				Collectible.Druid.DruidOfTheClaw,
			};

			public static List<string> Warlock = new List<string>
			{
				Collectible.Warlock.CorruptionLegacy,
				Collectible.Warlock.MortalCoilLegacy,
				Collectible.Warlock.VoidwalkerLegacy,
				Collectible.Neutral.KnifeJuggler,
				Collectible.Neutral.SunfuryProtector,
				Collectible.Warlock.DrainLifeLegacy,
				Collectible.Neutral.ImpMaster,
				Collectible.Neutral.DarkIronDwarf,
				Collectible.Warlock.HellfireLegacy,
				Collectible.Warlock.Doomguard,
			};

			public static List<string> Mage = new List<string>
			{
				Collectible.Neutral.ArcaneAnomaly,
				Collectible.Mage.ArcaneMissilesLegacy,
				Collectible.Neutral.Doomsayer,
				Collectible.Mage.FrostboltLegacy,
				Collectible.Mage.SorcerersApprentice,
				Collectible.Neutral.EarthenRingFarseer,
				Collectible.Mage.IceBarrier,
				Collectible.Neutral.ChillwindYetiLegacy,
				Collectible.Mage.FireballLegacy,
				Collectible.Mage.Blizzard,
			};

			public static List<string> Priest = new List<string>
			{
				Collectible.Priest.HolySmiteLegacy,
				Collectible.Priest.NorthshireClericLegacy,
				Collectible.Priest.PotionOfMadness,
				Collectible.Neutral.FaerieDragon,
				Collectible.Priest.MindBlastLegacy,
				Collectible.Priest.ShadowWordPainLegacy,
				Collectible.Priest.DarkCultist,
				Collectible.Priest.AuchenaiSoulpriest,
				Collectible.Priest.Lightspawn,
				Collectible.Priest.HolyNovaLegacy,
			};
		}

		private static class TrlDefaultDecks
		{
			public static List<string>[] Rogue => new[]
			{
				new List<string>
				{
					NonCollectible.Rogue.BottledTerror,
					Collectible.Rogue.Buccaneer,
					Collectible.Rogue.ColdBlood,
					Collectible.Rogue.DefiasRingleader,
					Collectible.Rogue.ShadowSensei,
					Collectible.Neutral.AbusiveSergeant,
					Collectible.Neutral.SouthseaDeckhand,
					Collectible.Neutral.CaptainsParrotLegacy,
					Collectible.Neutral.SharkfinFan,
					Collectible.Neutral.ShipsCannon,
					Collectible.Neutral.HenchClanThug,
				},
				new List<string>
				{
					NonCollectible.Rogue.TreasureFromBelow,
					Collectible.Rogue.Preparation,
					Collectible.Rogue.CounterfeitCoin,
					Collectible.Rogue.BackstabLegacy,
					Collectible.Rogue.Conceal,
					Collectible.Rogue.UndercityValiant,
					Collectible.Rogue.Betrayal,
					Collectible.Rogue.ObsidianShard,
					Collectible.Neutral.SmallTimeBuccaneerGANGS,
					Collectible.Neutral.SouthseaDeckhand,
					Collectible.Neutral.BloodsailRaider,
				},
				new List<string>
				{
					NonCollectible.Rogue.PiratesMark,
					Collectible.Rogue.BackstabLegacy,
					Collectible.Rogue.CounterfeitCoin,
					Collectible.Neutral.ArcaneAnomaly,
					Collectible.Rogue.SinisterStrikeLegacy,
					Collectible.Rogue.Betrayal,
					Collectible.Neutral.KoboldGeomancerLegacy,
					Collectible.Neutral.Spellzerker,
					Collectible.Rogue.FanOfKnivesLegacy,
					Collectible.Rogue.AcademicEspionage,
					Collectible.Rogue.TombPillagerLOE,
				}

			};

			public static List<string>[] Warrior => new[]
			{
				new List<string>
				{
					NonCollectible.Warrior.AkalisChampion,
					Collectible.Warrior.EterniumRover,
					Collectible.Warrior.Armorsmith,
					Collectible.Warrior.DrywhiskerArmorer,
					Collectible.Warrior.FieryWarAxeLegacy,
					Collectible.Warrior.MountainfireArmor,
					Collectible.Warrior.EmberscaleDrake,
					Collectible.Neutral.Waterboy,
					Collectible.Neutral.HiredGun,
					Collectible.Neutral.HalfTimeScavenger,
					Collectible.Neutral.DragonmawScorcher,
				},
				new List<string>
				{
					NonCollectible.Warrior.AkalisWarDrum,
					Collectible.Warrior.DragonRoar,
					Collectible.Neutral.FaerieDragon,
					Collectible.Neutral.FiretreeWitchdoctor,
					Collectible.Neutral.NetherspiteHistorian,
					Collectible.Warrior.FieryWarAxeLegacy,
					Collectible.Neutral.NightmareAmalgam,
					Collectible.Neutral.EbonDragonsmith,
					Collectible.Neutral.TwilightGuardian,
					Collectible.Warrior.EmberscaleDrake,
					Collectible.Neutral.BoneDrake,
				},
				new List<string>
				{
					NonCollectible.Warrior.AkalisHorn,
					Collectible.Warrior.InnerRage,
					Collectible.Warrior.NzothsFirstMate,
					Collectible.Warrior.Warbot,
					Collectible.Warrior.ExecuteLegacy,
					Collectible.Warrior.Rampage,
					Collectible.Warrior.CruelTaskmaster,
					Collectible.Warrior.BloodhoofBrave,
					Collectible.Neutral.AmaniBerserker,
					Collectible.Neutral.Deathspeaker,
					Collectible.Neutral.RagingWorgen,
				},
			};

			public static List<string>[] Shaman => new[]
			{
				new List<string>
				{
					NonCollectible.Shaman.KragwasLure,
					Collectible.Shaman.ForkedLightning,
					Collectible.Shaman.StormforgedAxe,
					Collectible.Shaman.UnboundElemental,
					Collectible.Shaman.LightningStorm,
					Collectible.Shaman.JinyuWaterspeaker,
					Collectible.Shaman.FireguardDestroyer,
					Collectible.Neutral.MurlocRaiderLegacy,
					Collectible.Neutral.DeadscaleKnight,
					Collectible.Neutral.HugeToad,
					Collectible.Neutral.TarCreeper,
				},
				new List<string>
				{
					NonCollectible.Shaman.TributeFromTheTides,
					Collectible.Shaman.BlazingInvocation,
					Collectible.Shaman.TotemicSmash,
					Collectible.Shaman.HotSpringGuardian,
					Collectible.Shaman.LightningStorm,
					Collectible.Shaman.FireElementalLegacy,
					Collectible.Neutral.EmeraldReaver,
					Collectible.Neutral.FireFly,
					Collectible.Neutral.BilefinTidehunter,
					Collectible.Neutral.BelligerentGnome,
					Collectible.Neutral.SaroniteChainGang,
				},
				new List<string>
				{
					NonCollectible.Shaman.KragwasGrace,
					Collectible.Shaman.Wartbringer,
					Collectible.Shaman.Crackle,
					Collectible.Shaman.LavaShock,
					Collectible.Shaman.MaelstromPortal,
					Collectible.Shaman.FarSight,
					Collectible.Shaman.FeralSpirit,
					Collectible.Shaman.CallInTheFinishersGANGS,
					Collectible.Shaman.RainOfToads,
					Collectible.Neutral.ManaAddict,
					Collectible.Neutral.BananaBuffoon,
				},
			};

			public static List<string>[] Paladin => new[]
			{
				new List<string>
				{
					NonCollectible.Paladin.ShirvallahsProtection,
					Collectible.Paladin.DivineStrength,
					Collectible.Paladin.MeanstreetMarshal,
					Collectible.Paladin.GrimestreetOutfitter,
					Collectible.Paladin.ParagonOfLight,
					Collectible.Paladin.FarrakiBattleaxe,
					Collectible.Neutral.ElvenArcherLegacy,
					Collectible.Neutral.InjuredKvaldir,
					Collectible.Neutral.BelligerentGnome,
					Collectible.Neutral.ArenaFanatic,
					Collectible.Neutral.StormwindKnightLegacy,
				},
				new List<string>
				{
					NonCollectible.Paladin.ShirvallahsVengeance,
					Collectible.Paladin.Bloodclaw,
					Collectible.Paladin.FlashOfLight,
					Collectible.Paladin.SealOfLight,
					Collectible.Paladin.BenevolentDjinn,
					Collectible.Paladin.TruesilverChampionLegacy,
					Collectible.Paladin.ChillbladeChampion,
					Collectible.Neutral.Crystallizer,
					Collectible.Neutral.MadBomber,
					Collectible.Neutral.HappyGhoul,
					Collectible.Neutral.MadderBomber
				},
				new List<string>
				{
					NonCollectible.Paladin.ShirvallahsGrace,
					Collectible.Neutral.ArgentSquire,
					Collectible.Paladin.DivineStrength,
					Collectible.Paladin.HandOfProtectionLegacy,
					Collectible.Paladin.FlashOfLight,
					Collectible.Paladin.PotionOfHeroism,
					Collectible.Paladin.PrimalfinChampion,
					Collectible.Neutral.BananaBuffoon,
					Collectible.Paladin.SealOfChampions,
					Collectible.Paladin.BlessingOfKingsLegacy,
					Collectible.Paladin.TruesilverChampionLegacy
				},
			};

			public static List<string>[] Hunter => new[]
			{
				new List<string>
				{
					NonCollectible.Hunter.HalazzisTrap,
					Collectible.Hunter.Candleshot,
					Collectible.Hunter.ArcaneShotLegacy,
					Collectible.Hunter.HuntersMarkLegacy,
					Collectible.Hunter.SecretPlan,
					Collectible.Hunter.ExplosiveTrap,
					Collectible.Hunter.QuickShot,
					Collectible.Hunter.AnimalCompanionLegacy,
					Collectible.Hunter.BloodscalpStrategist,
					Collectible.Hunter.BaitedArrow,
					Collectible.Neutral.BurglyBully,
				},
				new List<string>
				{
					NonCollectible.Hunter.HalazzisHunt,
					Collectible.Hunter.HuntersMarkLegacy,
					Collectible.Hunter.Alleycat,
					Collectible.Hunter.Springpaw,
					Collectible.Hunter.Glaivezooka,
					Collectible.Hunter.ScavengingHyena,
					Collectible.Hunter.AnimalCompanionLegacy,
					Collectible.Hunter.CaveHydra,
					Collectible.Hunter.HoundmasterLegacy,
					Collectible.Hunter.SavannahHighmane,
					Collectible.Neutral.DireWolfAlpha,
				},
				new List<string>
				{
					NonCollectible.Hunter.HalazzisGuise,
					Collectible.Hunter.JeweledMacaw,
					Collectible.Hunter.Springpaw,
					Collectible.Hunter.Webspinner,
					Collectible.Hunter.KillCommandLegacy,
					Collectible.Hunter.RatPack,
					Collectible.Hunter.BaitedArrow,
					Collectible.Neutral.DireWolfAlpha,
					Collectible.Neutral.SilverbackPatriarchLegacy,
					Collectible.Neutral.UntamedBeastmaster,
					Collectible.Neutral.OasisSnapjawLegacy,
				},
			};

			public static List<string>[] Druid => new[]
			{
				new List<string>
				{
					NonCollectible.Druid.GonksArmament,
					Collectible.Druid.ForbiddenAncient,
					Collectible.Druid.LesserJasperSpellstone,
					Collectible.Neutral.LowlySquire,
					Collectible.Neutral.Waterboy,
					Collectible.Druid.Wrath,
					Collectible.Druid.FerociousHowl,
					Collectible.Druid.GroveTender,
					Collectible.Neutral.HalfTimeScavenger,
					Collectible.Druid.IronwoodGolem,
					Collectible.Neutral.SnapjawShellfighter,
				},
				new List<string>
				{
					NonCollectible.Druid.GonksMark,
					Collectible.Druid.EnchantedRaven,
					Collectible.Druid.PowerOfTheWild,
					Collectible.Druid.WitchwoodApple,
					Collectible.Druid.MountedRaptor,
					Collectible.Druid.SwipeLegacy,
					Collectible.Neutral.WaxElemental,
					Collectible.Neutral.BloodfenRaptorLegacy,
					Collectible.Neutral.InfestedTauren,
					Collectible.Neutral.StormwindKnightLegacy,
					Collectible.Neutral.ArenaPatron,
				},
				new List<string>
				{
					NonCollectible.Druid.BondsOfBalance,
					Collectible.Druid.Pounce,
					Collectible.Druid.ClawLegacy,
					Collectible.Druid.EnchantedRaven,
					Collectible.Druid.PowerOfTheWild,
					Collectible.Druid.SavageStriker,
					Collectible.Druid.Gnash,
					Collectible.Druid.Bite,
					Collectible.Druid.SavageCombatant,
					Collectible.Neutral.Waterboy,
					Collectible.Neutral.SharkfinFan,
				},
			};

			public static List<string>[] Warlock => new[]
			{
				new List<string>
				{
					NonCollectible.Warlock.BloodPact,
					Collectible.Warlock.VoidwalkerLegacy,
					Collectible.Warlock.QueenOfPain,
					Collectible.Warlock.Demonfire,
					Collectible.Warlock.Duskbat,
					Collectible.Warlock.ImpLosion,
					Collectible.Warlock.LesserAmethystSpellstone,
					Collectible.Warlock.FiendishCircle,
					Collectible.Warlock.BaneOfDoomExpert1,
					Collectible.Neutral.BananaBuffoon,
					Collectible.Neutral.VioletIllusionist,
				},
				new List<string>
				{
					NonCollectible.Warlock.DarkReliquary,
					Collectible.Warlock.Shriek,
					Collectible.Warlock.SoulfireLegacy,
					Collectible.Warlock.VoidwalkerLegacy,
					Collectible.Warlock.FelstalkerLegacy,
					Collectible.Warlock.DarkshireLibrarian,
					Collectible.Warlock.RecklessDiretroll,
					Collectible.Warlock.LakkariFelhound,
					Collectible.Warlock.Soulwarden,
					Collectible.Neutral.BelligerentGnome,
					Collectible.Neutral.BananaBuffoon,
				},
				new List<string>
				{
					NonCollectible.Warlock.HireeksHunger,
					Collectible.Warlock.FlameImp,
					Collectible.Warlock.CallOfTheVoidLegacy,
					Collectible.Warlock.SpiritBomb,
					Collectible.Warlock.UnlicensedApothecary,
					Collectible.Warlock.BloodWitch,
					Collectible.Warlock.HellfireLegacy,
					Collectible.Neutral.KnifeJuggler,
					Collectible.Neutral.Waterboy,
					Collectible.Neutral.ImpMaster,
					Collectible.Neutral.BlackwaldPixie,
				}
			};

			public static List<string>[] Mage => new[]
			{
				new List<string>
				{
					NonCollectible.Mage.JanalaisMantle,
					Collectible.Mage.BabblingBook,
					Collectible.Mage.ArcaneExplosionLegacy,
					Collectible.Mage.ShimmeringTempest,
					Collectible.Mage.ExplosiveRunes,
					Collectible.Mage.SpellslingerTGT,
					Collectible.Mage.GhastlyConjurer,
					Collectible.Mage.BlastWave,
					Collectible.Neutral.TournamentAttendee,
					Collectible.Neutral.Brainstormer,
					Collectible.Neutral.KabalChemist,
				},
				new List<string>
				{
					NonCollectible.Mage.JanalaisFlame,
					Collectible.Mage.ArcaneBlast,
					Collectible.Mage.FallenHero,
					Collectible.Mage.Cinderstorm,
					Collectible.Mage.DalaranAspirantTGT,
					Collectible.Mage.FireballLegacy,
					Collectible.Neutral.AcherusVeteran,
					Collectible.Neutral.FlameJuggler,
					Collectible.Neutral.BlackwaldPixie,
					Collectible.Neutral.DragonhawkRider,
					Collectible.Neutral.FirePlumePhoenix,
				},
				new List<string>
				{
					NonCollectible.Mage.JanalaisProgeny,
					Collectible.Mage.FreezingPotion,
					Collectible.Neutral.ArcaneAnomaly,
					Collectible.Mage.FrostboltLegacy,
					Collectible.Mage.Snowchugger,
					Collectible.Neutral.VolatileElemental,
					Collectible.Neutral.HyldnirFrostrider,
					Collectible.Mage.ConeOfCold,
					Collectible.Neutral.IceCreamPeddler,
					Collectible.Mage.WaterElementalLegacy,
					Collectible.Neutral.FrostElemental
				},
			};

			public static List<string>[] Priest => new[]
			{
				new List<string>
				{
					NonCollectible.Priest.BwonsamdisSanctum,
					Collectible.Priest.CrystallineOracle,
					Collectible.Priest.SpiritLash,
					Collectible.Priest.MuseumCuratorLOE,
					Collectible.Priest.DeadRinger,
					Collectible.Priest.ShiftingShade,
					Collectible.Priest.TortollanShellraiser,
					Collectible.Neutral.MistressOfMixtures,
					Collectible.Neutral.HarvestGolem,
					Collectible.Neutral.ShallowGravedigger,
					Collectible.Neutral.TombLurker
				},
				new List<string>
				{
					NonCollectible.Priest.BwonsamdisTome,
					Collectible.Priest.PsionicProbe,
					Collectible.Priest.PowerWordShieldLegacy,
					Collectible.Priest.SpiritLash,
					Collectible.Priest.SandDrudge,
					Collectible.Priest.GildedGargoyle,
					Collectible.Priest.Mindgames,
					Collectible.Neutral.ArcaneAnomaly,
					Collectible.Neutral.ClockworkGnome,
					Collectible.Neutral.WildPyromancer,
					Collectible.Neutral.BananaBuffoon,
				},
				new List<string>
				{
					NonCollectible.Priest.BwonsamdisCovenant,
					Collectible.Priest.CircleOfHealing,
					Collectible.Priest.Regenerate,
					Collectible.Priest.FlashHeal,
					Collectible.Neutral.InjuredKvaldir,
					Collectible.Priest.LightOfTheNaaru,
					Collectible.Neutral.VoodooDoctorLegacy,
					Collectible.Neutral.GadgetzanSocialite,
					Collectible.Neutral.Waterboy,
					Collectible.Neutral.EarthenRingFarseer,
					Collectible.Neutral.InjuredBlademaster
				},
			};
		}
	}
}
