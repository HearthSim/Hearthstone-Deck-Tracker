using System.Collections.Generic;
using System.Collections.ObjectModel;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Importing;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class WebImportTest
	{

		[TestMethod]
		public void InvalidUrlTest()
		{
			var found = DeckImporter.Import(@"http://hsdecktracker.net").Result;
			Assert.IsNull(found);
		}

		[TestMethod]
		public void Hearthstats()
		{
			Deck expected = CreateDeck();
			Deck found = DeckImporter.Import(@"https://hearthstats.net/decks/senfglas-patron--248").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		[TestMethod]
		public void Hearthpwn()
		{
			Deck expected = CreateDeck();
			Deck found = DeckImporter.Import(@"http://www.hearthpwn.com/decks/267064-grim-patron-senfglas").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		[TestMethod]
		public void HearthpwnDeckBuilder()
		{
			Deck expected = CreateDeck();
			Deck found = DeckImporter.Import(@"http://www.hearthpwn.com/deckbuilder/warrior#69:2;161:2;193:2;215:2;227:2;246:2;261:1;328:1;366:1;428:2;632:2;644:2;664:2;7734:2;7757:2;14435:2;14454:1").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		[TestMethod]
		public void IcyVeins()
		{
			Deck expected = CreateDeck("Inner Rage;2", "Fiery War Axe;1", "Shield Block;2", "Dread Corsair;0", "Cruel Taskmaster;0");
			Deck found = DeckImporter.Import(@"http://www.icy-veins.com/hearthstone/low-budget-warrior-grim-patron-otk-one-turn-kill-brm-deck").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		[TestMethod]
		public void Hearthhead()
		{
			Deck expected = CreateDeck("Inner Rage;0", "Commanding Shout;1", "Cruel Taskmaster;2", "Dread Corsair;2", "Armorsmith;0");
			Deck found = DeckImporter.Import(@"http://www.hearthhead.com/deck=104740/forsen-senfglas").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		[TestMethod]
		public void HearthstonePlayers()
		{
			Deck expected = CreateDeck();
			Deck found = DeckImporter.Import(@"http://hearthstoneplayers.com/shengs-budget-patron-warrior-deck/").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		[TestMethod]
		public void HearthstoneTopDecks()
		{
			Deck expected = CreateDeck();
			Deck found = DeckImporter.Import(@"http://www.hearthstonetopdecks.com/decks/lifecoachs-archon-league-w-1-patron-warrior/").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		[TestMethod]
		public void HearthnewsFR()
		{
			Deck expected = CreateDeck("Inner Rage;0", "Commanding Shout;1", "Cruel Taskmaster;2", "Dread Corsair;2", "Armorsmith;0");
			Deck found = DeckImporter.Import(@"http://www.hearthnews.fr/decks/4620").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		[TestMethod]
		public void HearthBuilder()
		{
			Deck expected = CreateDeck("Inner Rage;0", "Commanding Shout;1", "Cruel Taskmaster;2", "Dread Corsair;2", "Armorsmith;0");
			Deck found = DeckImporter.Import(@"http://www.hearthbuilder.com/decks/senfglas-1-legend-grim-patron-warrior").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}		

		[TestMethod]
		public void HearthstoneDecksFR()
		{
			Deck expected = CreateDeck("Inner Rage;0", "Cruel Taskmaster;2", "Slam;0", "Grommash Hellscream;1", "Loot Hoarder;2", "Unstable Ghoul;0", "Dr. Boom;1");
			Deck found = DeckImporter.Import(@"http://www.hearthstone-decks.com/deck/voir/grim-patron-by-sjow-3163").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		[TestMethod]
		public void HearthstoneHerosDE()
		{
			Deck expected = CreateDeck("Commanding Shout;1", "Unstable Ghoul;1", "Gnomish Inventor;1", "Dread Corsair;2");
			Deck found = DeckImporter.Import(@"http://www.hearthstoneheroes.de/decks/grim-patron-oder-grimmiger-gast-von-trump-inspiriert/").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		[TestMethod]
		public void EliteHearthstone()
		{
			Deck expected = CreateDeck("Inner Rage;2", "Loot Hoarder;2", "Cruel Taskmaster;0", "Gnomish Inventor;0");
			Deck found = DeckImporter.Import(@"http://www.elitehearthstone.net/deck-7918-patron-warrior").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		[TestMethod]
		public void TempoStorm()
		{
			Deck expected = CreateDeck();
			Deck found = DeckImporter.Import(@"https://tempostorm.com/hearthstone/decks/senfglas-patron").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		[TestMethod]
		public void HearthstoneTopDeck()
		{
			Deck expected = CreateDeck();
			Deck found = DeckImporter.Import(@"http://www.hearthstonetopdeck.com/deck/wild/4700/otk-patron-senfglas-lifecoach").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}				

		[TestMethod]
		public void HearthArena()
		{
			Deck found = DeckImporter.Import(@"http://www.heartharena.com/arena-run/i2s8ht").Result;
			Assert.IsTrue(AreDecksEqual(arena, found));
		}

		[TestMethod]
		public void ManaCrystals()
		{
			Deck expected = CreateDeck("Inner Rage;2", "Shield Slam;2", "Grommash Hellscream;1", "Piloted Shredder;2",
				"Dr. Boom;1", "Warsong Commander;0", "Slam;0", "Gnomish Inventor;0", "Emperor Thaurissan;0");
			Deck found = DeckImporter.Import(@"https://manacrystals.com/decklists/172-zalae-s-patron-warrior").Result;
			Assert.IsTrue(AreDecksEqual(expected, found));
		}

		/* WebBrowser causes test to hang, for some reason */
		//[TestMethod]
		//public void ArenaValue()
		//{
		//	Deck found = DeckImporter.Import(@"http://www.arenavalue.com/s/AnuBGh").Result;
		//	Assert.IsTrue(AreDecksEqual(arena, found));
		//}

		//--- SetUp ---

		private static List<Card> cardList;
		private static Deck arena;

		[ClassInitialize]
		public static void Init(TestContext context)
		{
			List<string> cardNames = new List<string>
			{
				"Inner Rage;1",
				"Execute;2",
				"Whirlwind;2",
				"Armorsmith;2",
				"Battle Rage;2",
				"Commanding Shout;0",
				"Cruel Taskmaster;1",
				"Fiery War Axe;2",
				"Loot Hoarder;0",
				"Slam;2",
				"Unstable Ghoul;2",
				"Acolyte of Pain;2",
				"Frothing Berserker;2",
				"Shield Block;0",
				"Warsong Commander;2",
				"Death's Bite;2",
				"Dread Corsair;1",
				"Gnomish Inventor;2",
				"Grim Patron;2",				
				"Emperor Thaurissan;1",
				"Grommash Hellscream;0",
				"Dr. Boom;0",
				"Piloted Shredder;0",
				"Shield Slam;0"
			};

			cardList = DefaultDeck(cardNames);

			List<string> arenaNames = new List<string>
			{
				"Elven Archer;1",
				"Stonetusk Boar;1",
				"Worgen Infiltrator;2",
				"Young Priestess;1",
				"Acidic Swamp Ooze;2",
				"Argent Watchman;1",
				"Freezing Trap;2",
				"Novice Engineer;1",
				"Puddlestomper;1",
				"Sunfury Protector;1",
				"Acolyte of Pain;1",
				"Alarm-o-Bot;1",
				"Flying Machine;2",
				"Ironfur Grizzly;2",
				"Razorfen Hunter;1",
				"Silverback Patriarch;1",
				"Stablemaster;1",
				"Sen'jin Shieldmasta;1",
				"Abomination;1",
				"Blackwing Corruptor;1",
				"Madder Bomber;1",
				"Starving Buzzard;1",
				"Acidmaw;1",
				"Sneed's Old Shredder;1",
				"Malygos;1"
			};

			var arenaList = DefaultDeck(arenaNames);
			arena = new Deck();
			arena.Cards = new ObservableCollection<Card>(arenaList);
		}

		//--- Helpers ---

		private static Deck CreateDeck(params string[] edits)
		{
			var editMap = new Dictionary<string, int>();
			foreach(var e in edits)
			{
				var split = e.Split(';');
				editMap.Add(split[0], int.Parse(split[1]));
			}

			var deckList = new List<Card>();
			foreach(var c in cardList)
			{
				var edit = c;
				if(editMap.ContainsKey(c.Name))
				{
					var cc = (Card)c.Clone();
					cc.Count = editMap[c.Name];
					edit = cc;
				}
				if(edit.Count > 0)
					deckList.Add(edit);
			}

			var deck = new Deck();
			deck.Cards = new ObservableCollection<Card>(deckList);

			return deck;
		}	

		private static List<Card> DefaultDeck(List<string> names)
		{
			var cards = new List<Card>();

			foreach(var n in names)
			{
				var split = n.Split(';');
				var card = Database.GetCardFromName(split[0]);
				card.Count = int.Parse(split[1]);
				cards.Add(card);
			}

			return cards;
		}

		private bool AreDecksEqual(Deck a, Deck b)
		{
			var ca = a.Cards.ToSortedCardList();
			var cb = b.Cards.ToSortedCardList();

			if(ca.Count != cb.Count)
				return false;
			
			for(int i = 0; i < ca.Count; i++)
			{
				if(!ca[i].EqualsWithCount(cb[i]))
					return false;
			}

			return true;
		}
	}
}
