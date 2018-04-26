using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using HearthDb.Enums;
using static HearthDb.CardIds;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class DungeonRun
	{
		public static Deck GetDefaultDeck(string playerClass, CardSet set)
		{
			var cards = GetCards(playerClass, set);
			if(cards == null)
				return null;
			var deck = new Deck
			{
				Cards = new ObservableCollection<Card>(cards.Select(Database.GetCardFromId)),
				Class = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(playerClass.ToLowerInvariant()),
				IsDungeonDeck = true,
				LastEdited = DateTime.Now
			};
			//Hack to avoid multiple templates. Should be good enough 99.9% of the time.
			var template = Config.Instance.DungeonRunDeckNameTemplate.Replace("Dungeon Run", "Monster Hunt");
			deck.Name = Helper.ParseDeckNameTemplate(template, deck);
			return deck;
		}

		private static List<string> GetCards(string playerClass, CardSet set)
		{
			switch(set)
			{
				case CardSet.LOOTAPALOOZA:
				{
					switch(playerClass.ToUpperInvariant())
					{
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
			}
			return null;
		}

		public static bool IsDungeonBoss(string cardId) => cardId != null && (cardId.Contains("LOOT") || cardId.Contains("GIL")) && cardId.Contains("BOSS");

		private static class GilDefaultDecks
		{
			public static List<string> Rogue => new List<string>
			{
				Collectible.Neutral.ElvenArcher,
				Collectible.Rogue.SinisterStrike,
				Collectible.Neutral.WorgenInfiltrator,
				Collectible.Neutral.BloodsailRaider,
				Collectible.Hunter.Glaivezooka,
				Collectible.Hunter.SnakeTrap,
				Collectible.Rogue.BlinkFox,
				Collectible.Rogue.FanOfKnives,
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
				Collectible.Hunter.HuntingMastiff,
				Collectible.Hunter.ForlornStalker,
				Collectible.Hunter.KillCommand,
				Collectible.Hunter.UnleashTheHounds,
				Collectible.Hunter.Houndmaster,
				Collectible.Neutral.SwiftMessenger
			};

			public static List<string> Mage => new List<string>
			{
				Collectible.Mage.ArcaneMissiles,
				Collectible.Mage.ManaWyrm,
				Collectible.Neutral.MadBomber,
				Collectible.Mage.PrimordialGlyph,
				Collectible.Mage.ShimmeringTempest,
				Collectible.Mage.UnstablePortal,
				Collectible.Mage.Spellslinger,
				Collectible.Neutral.TinkmasterOverspark,
				Collectible.Mage.WaterElemental,
				Collectible.Neutral.Blingtron3000
			};
		}

		private static class LootDefaultDecks
		{
			public static List<string> Rogue => new List<string>
			{
				Collectible.Rogue.Backstab,
				Collectible.Rogue.DeadlyPoison,
				Collectible.Rogue.PitSnake,
				Collectible.Rogue.SinisterStrike,
				Collectible.Neutral.GilblinStalker,
				Collectible.Rogue.UndercityHuckster,
				Collectible.Rogue.Si7Agent,
				Collectible.Rogue.UnearthedRaptor,
				Collectible.Rogue.Assassinate,
				Collectible.Rogue.Vanish,
			};

			public static List<string> Warrior = new List<string>
			{
				Collectible.Warrior.Warbot,
				Collectible.Neutral.AmaniBerserker,
				Collectible.Warrior.CruelTaskmaster,
				Collectible.Warrior.HeroicStrike,
				Collectible.Warrior.Bash,
				Collectible.Warrior.FieryWarAxe,
				Collectible.Neutral.HiredGun,
				Collectible.Neutral.RagingWorgen,
				Collectible.Neutral.DreadCorsair,
				Collectible.Warrior.Brawl,
			};

			public static List<string> Shaman = new List<string>
			{
				Collectible.Shaman.AirElemental,
				Collectible.Shaman.LightningBolt,
				Collectible.Shaman.FlametongueTotem,
				Collectible.Neutral.MurlocTidehunter,
				Collectible.Shaman.StormforgedAxe,
				Collectible.Shaman.LightningStorm,
				Collectible.Shaman.UnboundElemental,
				Collectible.Neutral.DefenderOfArgus,
				Collectible.Shaman.Hex,
				Collectible.Shaman.FireElemental,
			};

			public static List<string> Paladin = new List<string>
			{
				Collectible.Paladin.BlessingOfMight,
				Collectible.Neutral.GoldshireFootman,
				Collectible.Paladin.NobleSacrifice,
				Collectible.Paladin.ArgentProtector,
				Collectible.Paladin.Equality,
				Collectible.Paladin.HolyLight,
				Collectible.Neutral.EarthenRingFarseer,
				Collectible.Paladin.Consecration,
				Collectible.Neutral.StormwindKnight,
				Collectible.Paladin.TruesilverChampion,
			};

			public static List<string> Hunter = new List<string>
			{
				Collectible.Hunter.HuntersMark,
				Collectible.Neutral.StonetuskBoar,
				Collectible.Neutral.DireWolfAlpha,
				Collectible.Hunter.ExplosiveTrap,
				Collectible.Hunter.AnimalCompanion,
				Collectible.Hunter.DeadlyShot,
				Collectible.Hunter.EaglehornBow,
				Collectible.Neutral.JunglePanther,
				Collectible.Hunter.UnleashTheHounds,
				Collectible.Neutral.OasisSnapjaw,
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
				Collectible.Druid.Swipe,
				Collectible.Druid.DruidOfTheClaw,
			};

			public static List<string> Warlock = new List<string>
			{
				Collectible.Warlock.Corruption,
				Collectible.Warlock.MortalCoil,
				Collectible.Warlock.Voidwalker,
				Collectible.Neutral.KnifeJuggler,
				Collectible.Neutral.SunfuryProtector,
				Collectible.Warlock.DrainLife,
				Collectible.Neutral.ImpMaster,
				Collectible.Neutral.DarkIronDwarf,
				Collectible.Warlock.Hellfire,
				Collectible.Warlock.Doomguard,
			};

			public static List<string> Mage = new List<string>
			{
				Collectible.Mage.ArcaneMissiles,
				Collectible.Mage.ManaWyrm,
				Collectible.Neutral.Doomsayer,
				Collectible.Mage.Frostbolt,
				Collectible.Mage.SorcerersApprentice,
				Collectible.Neutral.EarthenRingFarseer,
				Collectible.Mage.IceBarrier,
				Collectible.Neutral.ChillwindYeti,
				Collectible.Mage.Fireball,
				Collectible.Mage.Blizzard,
			};

			public static List<string> Priest = new List<string>
			{
				Collectible.Priest.HolySmite,
				Collectible.Priest.NorthshireCleric,
				Collectible.Priest.PotionOfMadness,
				Collectible.Neutral.FaerieDragon,
				Collectible.Priest.MindBlast,
				Collectible.Priest.ShadowWordPain,
				Collectible.Priest.DarkCultist,
				Collectible.Priest.AuchenaiSoulpriest,
				Collectible.Priest.Lightspawn,
				Collectible.Priest.HolyNova,
			};
		}
	}
}
