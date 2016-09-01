using System;
using System.Collections.Generic;
using System.Threading;
using HearthMirror.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HearthWatcher.Test
{
	[TestClass]
	public class ArenaWatcherTests
	{
		private int _choicesChangedCalls;
		private int _cardPickedCalls;
		private int _completeDeckCalls;
		private int _rewardsCalls;
		private Card[] _currentChoices;
		private Card _currentPick;
		private ArenaInfo _currentArenaInfo;
		private List<RewardData> _currentRewardData;
		private TestArenaProvider _provider;
		private ArenaWatcher _watcher;

		[TestInitialize]
		public void Initialize()
		{       
			_choicesChangedCalls = 0;
			_cardPickedCalls = 0;
			_completeDeckCalls = 0;
			_rewardsCalls = 0;
			_currentChoices = null;
			_currentPick = null;
			_provider = new TestArenaProvider();
			_watcher = new ArenaWatcher(_provider);
			SetupWatcher();
		}

		private void SetupWatcher()
		{
			_watcher.OnChoicesChanged += (sender, args) =>
			{
				_currentChoices = args.Choices;
				_choicesChangedCalls++;
			};
			_watcher.OnCardPicked += (sender, args) =>
			{
				_currentPick = args.Picked;
				_cardPickedCalls++;
			};
			_watcher.OnCompleteDeck += (sender, args) =>
			{
				_currentArenaInfo = args.Info;
				_completeDeckCalls++;
			};
			_watcher.OnRewards += (sender, args) =>
			{
				_currentRewardData = args.Info.Rewards;
				_rewardsCalls++;
			};
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ProviderCanNotBeNull()
		{
			_watcher = new ArenaWatcher(null);
		}

		[TestMethod]
		public void Run_CompleteDeck()
		{
			_watcher = new ArenaWatcher(_provider, 5);
			SetupWatcher();
			_watcher.Run();
			_provider.ArenaInfo = new ArenaInfo {
				Deck = new Deck() { Id = 1, Hero = "HERO_02", Cards = new List<Card> { NewCard("AT_001", 30) } }
			};
			Thread.Sleep(100);
			Assert.IsNotNull(_currentArenaInfo);
			Assert.AreEqual(1, _completeDeckCalls);
			Assert.AreEqual(0, _rewardsCalls);
			Thread.Sleep(100);
			Assert.AreEqual(1, _completeDeckCalls);
			Assert.AreEqual(0, _rewardsCalls);
		}

		[TestMethod]
		public void Run_StartMultipleTimes_OnlyRunsOnce()
		{
			_watcher = new ArenaWatcher(_provider, 5);
			SetupWatcher();
			_watcher.Run();
			_watcher.Run();
			_watcher.Run();
			_provider.ArenaInfo = new ArenaInfo {
				Deck = new Deck() { Id = 1, Hero = "HERO_02", Cards = new List<Card> { NewCard("AT_001", 30) } }
			};
			Thread.Sleep(100);
			Assert.IsNotNull(_currentArenaInfo);
			Assert.AreEqual(1, _completeDeckCalls);
			Assert.AreEqual(0, _rewardsCalls);
		}

		[TestMethod]
		public void Run_ImmediateStop_DoesNotStart()
		{
			_watcher.Run();
			Thread.Sleep(100);
			_watcher.Stop();
			_provider.ArenaInfo = new ArenaInfo {
				Deck = new Deck() { Id = 1, Hero = "HERO_02", Cards = new List<Card> { NewCard("AT_001", 30) } }
			};
			Assert.IsNull(_currentArenaInfo);
			Assert.AreEqual(0, _completeDeckCalls);
		}

		[TestMethod]
		public void Run_NewDeck()
		{
			_watcher = new ArenaWatcher(_provider, 5);
			SetupWatcher();
			_watcher.Run();
			Thread.Sleep(100);
			Assert.AreEqual(0, _choicesChangedCalls);
			Assert.AreEqual(0, _cardPickedCalls);

			_provider.ArenaInfo = new ArenaInfo() { Deck = new Deck() { Id = 1 }, CurrentSlot = 0 };
			_provider.DraftChoices = new[] { NewCard("HERO_01"), NewCard("HERO_02"), NewCard("HERO_03") };
			Thread.Sleep(100);
			Assert.AreEqual(_provider.DraftChoices, _currentChoices);
			Assert.AreEqual(1, _choicesChangedCalls);
			Assert.AreEqual(0, _cardPickedCalls);

			_provider.DraftChoices = null;
			_provider.ArenaInfo = new ArenaInfo() { Deck = new Deck() { Id = 1, Hero = "HERO_02" }, CurrentSlot = 1 };
			Thread.Sleep(100);
			Assert.AreEqual(1, _choicesChangedCalls);
			Assert.AreEqual(0, _cardPickedCalls);

			_provider.DraftChoices = new[] { NewCard("AT_001"), NewCard("AT_002"), NewCard("AT_003") };
			Thread.Sleep(100);
			Assert.AreEqual(1, _cardPickedCalls);
			Assert.AreEqual("HERO_02", _currentPick.Id);
			Assert.AreEqual(2, _choicesChangedCalls);

			Thread.Sleep(100);
			Assert.AreEqual(2, _choicesChangedCalls);
			Assert.AreEqual(1, _cardPickedCalls);
			Assert.AreEqual(_provider.DraftChoices, _currentChoices);
		}

		[TestMethod]
		public void Manual_NewDeck_SingleUpdate()
		{
			_watcher.Update();
			Assert.AreEqual(0, _choicesChangedCalls);
			Assert.AreEqual(0, _cardPickedCalls);

			_provider.ArenaInfo = new ArenaInfo() { Deck = new Deck() { Id = 1 }, CurrentSlot = 0 };
			_provider.DraftChoices = new[] { NewCard("HERO_01"), NewCard("HERO_02"), NewCard("HERO_03") };
			_watcher.Update();
			Assert.AreEqual(_provider.DraftChoices, _currentChoices);
			Assert.AreEqual(1, _choicesChangedCalls);
			Assert.AreEqual(0, _cardPickedCalls);

			_provider.ArenaInfo = new ArenaInfo() { Deck = new Deck() { Id = 1, Hero = "HERO_02" }, CurrentSlot = 1 };
			_provider.DraftChoices = null;
			_watcher.Update();
			Assert.AreEqual(1, _choicesChangedCalls);
			Assert.AreEqual(0, _cardPickedCalls);

			_provider.DraftChoices = new[] { NewCard("AT_001"), NewCard("AT_002"), NewCard("AT_003") };
			_watcher.Update();
			Assert.AreEqual(1, _cardPickedCalls);
			Assert.AreEqual("HERO_02", _currentPick.Id);
			Assert.AreEqual(2, _choicesChangedCalls);

			_watcher.Update();
			Assert.AreEqual(2, _choicesChangedCalls);
			Assert.AreEqual(1, _cardPickedCalls);
			Assert.AreEqual(_provider.DraftChoices, _currentChoices);
		}

		[TestMethod]
		public void Manual_NewDeck_DoubleUpdate()
		{
			_watcher.Update();
			Assert.AreEqual(0, _choicesChangedCalls);
			Assert.AreEqual(0, _cardPickedCalls);

			_provider.ArenaInfo = new ArenaInfo() { Deck = new Deck() { Id = 1 }, CurrentSlot = 0 };
			_provider.DraftChoices = new[] { NewCard("HERO_01"), NewCard("HERO_02"), NewCard("HERO_03") };
			_watcher.Update();
			_watcher.Update();
			Assert.AreEqual(_provider.DraftChoices, _currentChoices);
			Assert.AreEqual(1, _choicesChangedCalls);
			Assert.AreEqual(0, _cardPickedCalls);

			_provider.ArenaInfo = new ArenaInfo() { Deck = new Deck() { Id = 1, Hero = "HERO_02" }, CurrentSlot = 1 };
			_provider.DraftChoices = null;
			_watcher.Update();
			_watcher.Update();
			Assert.AreEqual(1, _choicesChangedCalls);
			Assert.AreEqual(0, _cardPickedCalls);

			_provider.DraftChoices = new[] { NewCard("AT_001"), NewCard("AT_002"), NewCard("AT_003") };
			_watcher.Update();
			_watcher.Update();
			Assert.AreEqual(1, _cardPickedCalls);
			Assert.AreEqual("HERO_02", _currentPick.Id);
			Assert.AreEqual(2, _choicesChangedCalls);

			_watcher.Update();
			_watcher.Update();
			Assert.AreEqual(2, _choicesChangedCalls);
			Assert.AreEqual(1, _cardPickedCalls);
			Assert.AreEqual(_provider.DraftChoices, _currentChoices);
		}

		[TestMethod]
		public void Manual_SingleUpdateBetweenPickAndChoice()
		{
			_provider.ArenaInfo = new ArenaInfo() { Deck = new Deck() { Id = 1 }, CurrentSlot = 0 };
			_watcher.Update();
			_watcher.Update();
			_provider.DraftChoices = new[] { NewCard("HERO_01"), NewCard("HERO_02"), NewCard("HERO_03") };
			_watcher.Update();
			_watcher.Update();
			_provider.ArenaInfo = new ArenaInfo() { Deck = new Deck() { Id = 1, Hero = "HERO_02" }, CurrentSlot = 1 };
			_watcher.Update();
			_watcher.Update();
			_provider.DraftChoices = null;
			_watcher.Update();
			_watcher.Update();
			_provider.DraftChoices = new[] { NewCard("AT_001"), NewCard("AT_002"), NewCard("AT_003") };
			_watcher.Update();
			_watcher.Update();
			Assert.AreEqual(2, _choicesChangedCalls);
			Assert.AreEqual(1, _cardPickedCalls);
		}


		[TestMethod]
		public void Manual_DoubleUpdateBetweenChoiceAndPick()
		{
			// In this case we either have the exact same choices twice or the server was too slow to send
			// the new choices over the course of two updates
			_provider.ArenaInfo = new ArenaInfo() { Deck = new Deck() { Id = 1 }, CurrentSlot = 0 };
			_provider.DraftChoices = new[] { NewCard("HERO_01"), NewCard("HERO_02"), NewCard("HERO_03") };
			_watcher.Update();

			_provider.ArenaInfo = new ArenaInfo() { Deck = new Deck() { Id = 1, Hero = "HERO_02" }, CurrentSlot = 1 };
			_watcher.Update();
			Assert.AreEqual(1, _choicesChangedCalls);
			//We can not tell if the following actually happens: 
			//_provider.DraftChoices = new[] { NewCard("HERO_01"), NewCard("HERO_02"), NewCard("HERO_03") };
			_watcher.Update();
			Assert.AreEqual(2, _choicesChangedCalls);
			Assert.AreEqual(1, _cardPickedCalls);
		}

		[TestMethod]
		public void FinalPick_SingleUpdate()
		{
			_provider.ArenaInfo = new ArenaInfo {
				Deck = new Deck() { Id = 1, Hero = "HERO_02", Cards = new List<Card> { NewCard("AT_001", 29) } },
				CurrentSlot = 30
			};
			_provider.DraftChoices = new[] { NewCard("AT_001"), NewCard("AT_002"), NewCard("AT_003") };
			_watcher.Update();
			_watcher.Update();
			Assert.IsNull(_currentArenaInfo);
			_currentChoices = null;
			_provider.ArenaInfo = new ArenaInfo {
				Deck = new Deck() { Id = 1, Hero = "HERO_02", Cards = new List<Card> { NewCard("AT_001", 30) } },
				CurrentSlot = 31
			};
			_watcher.Update();
			_provider.DraftChoices = null;
			var exit = _watcher.Update();
			Assert.IsTrue(exit);
			Assert.AreEqual("AT_001", _currentPick.Id);
			Assert.IsNotNull(_currentArenaInfo);
			Assert.IsNull(_currentRewardData);
			Assert.IsNull(_currentChoices);
		}
		[TestMethod]
		public void Rewards()
		{
			_provider.ArenaInfo = new ArenaInfo
			{
				Deck = new Deck() {Id = 1, Hero = "HERO_02", Cards = new List<Card> {NewCard("AT_001", 30)}},
				Rewards = new List<RewardData> {new GoldRewardData(50)}
			};
			var exit = _watcher.Update();
			Assert.IsTrue(exit);
			Assert.IsNull(_currentPick);
			Assert.IsNull(_currentChoices);
			Assert.IsNotNull(_currentArenaInfo);
			Assert.IsNotNull(_currentRewardData);
			Assert.AreEqual(1, _rewardsCalls);
			Assert.AreEqual(1, _completeDeckCalls);
		}

		[TestMethod]
		public void DuplicatePick()
		{
			_provider.ArenaInfo = new ArenaInfo {
				Deck = new Deck() { Id = 1, Hero = "HERO_02", Cards = new List<Card> { NewCard("AT_001"), NewCard("AT_002", 2) } },
				CurrentSlot = 4
			};
			_provider.DraftChoices = new[] { NewCard("AT_001"), NewCard("AT_002"), NewCard("AT_003") };
			_watcher.Update();
			Assert.AreEqual(_provider.DraftChoices, _currentChoices);
			_provider.ArenaInfo = new ArenaInfo {
				Deck = new Deck() { Id = 1, Hero = "HERO_02", Cards = new List<Card> { NewCard("AT_001"), NewCard("AT_002", 3) } },
				CurrentSlot = 5
			};
			_provider.DraftChoices = new[] { NewCard("AT_004"), NewCard("AT_005"), NewCard("AT_006") };
			_watcher.Update();
			Assert.AreEqual(_provider.DraftChoices, _currentChoices);
			Assert.AreEqual("AT_002", _currentPick.Id);
		}

		public static Card NewCard(string id, int count = 1)
		{
			return new Card(id, count, false);
		}

		public class TestArenaProvider : IArenaProvider
		{
			public ArenaInfo ArenaInfo { get; set; }
			public ArenaInfo GetArenaInfo() => ArenaInfo;

			public Card[] DraftChoices { get; set; }
			public Card[] GetDraftChoices() => DraftChoices;

			public int Delay => 50;
		}
	}
}
