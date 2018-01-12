using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility;
using static HearthDb.CardIds;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class DungeonRun
	{
		public static Deck GetDefaultDeck(string playerClass)
		{
			var cards = GetCards(playerClass);
			if(cards == null)
				return null;
			var deck = new Deck
			{
				Cards = new ObservableCollection<Card>(cards.Select(Database.GetCardFromId)),
				Class = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(playerClass.ToLowerInvariant()),
				IsDungeonDeck = true,
				LastEdited = DateTime.Now
			};
			deck.Name = Helper.ParseDeckNameTemplate(Config.Instance.DungeonRunDeckNameTemplate, deck);
			return deck;
		}

		private static List<string> GetCards(string playerClass)
		{
			switch(playerClass.ToUpperInvariant())
			{
				case "ROGUE":
					return DefaultDecks.Rogue;
				case "WARRIOR":
					return DefaultDecks.Warrior;
				case "SHAMAN":
					return DefaultDecks.Shaman;
				case "PALADIN":
					return DefaultDecks.Paladin;
				case "HUNTER":
					return DefaultDecks.Hunter;
				case "DRUID":
					return DefaultDecks.Druid;
				case "WARLOCK":
					return DefaultDecks.Warlock;
				case "MAGE":
					return DefaultDecks.Mage;
				case "PRIEST":
					return DefaultDecks.Priest;
			}
			return null;
		}

		public static bool IsDungeonBoss(string cardId) => cardId != null && cardId.Contains("LOOT") && cardId.Contains("BOSS");

		private static class DefaultDecks
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
