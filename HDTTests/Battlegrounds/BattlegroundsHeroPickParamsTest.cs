using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static HearthDb.CardIds.NonCollectible;

namespace HDTTests.Battlegrounds
{
	[TestClass]
	public class BattlegroundsHeroPickParamsTest
	{
		private static readonly HashSet<Race> Races = new HashSet<Race> { Race.BEAST, Race.DEMON, Race.MURLOC, Race.PIRATE, Race.UNDEAD };

		private GameV2 _game;

		[TestInitialize]
		public void Setup()
		{
			// otherwise the params' card language is read from the registry and saved to the config
			if(Config.Instance.LastSeenHearthstoneLang == null)
				Config.Instance.LastSeenHearthstoneLang = "enUS";
			Core._game = null;
			_game = Core._game = new GameV2();
		}

		private static Entity CreateHero(int id, string cardId, int zonePosition)
		{
			var hero = new Entity(id) { CardId = cardId };
			hero.SetTag(GameTag.ZONE_POSITION, zonePosition);
			return hero;
		}

		private static int DbfId(string cardId) => HearthDb.Cards.All[cardId].DbfId;

		[TestMethod]
		public void HeroPickParams_RerollDuringTheRacesWaitIsRequestedAfterTheInitialHeroes()
		{
			var initialHeroes = _game.SnapshotBattlegroundsOfferedHeroes(new[]
			{
				CreateHero(2, Neutral.GalakrondTavernBrawl, 2),
				CreateHero(1, Neutral.EdwinVancleefTavernBrawl, 1),
			});
			_game.SnapshotBattlegroundsOfferedHeroes(new[]
			{
				CreateHero(1, Neutral.EdwinVancleefTavernBrawl, 1),
				CreateHero(2, Neutral.TheRatKingTavernBrawl, 2),
			});

			_game.CacheBattlegroundsHeroPickParams(initialHeroes, Races);
			var initialParams = _game.GetBattlegroundsHeroPickParams();

			CollectionAssert.AreEqual(new[] { DbfId(Neutral.EdwinVancleefTavernBrawl), DbfId(Neutral.GalakrondTavernBrawl) }, initialParams.HeroDbfIds);
			Assert.IsFalse(initialParams.IsReroll);

			initialParams.HeroPickRef = "initial-ref";
			_game.CacheBattlegroundsHeroRerollParams();
			var rerollParams = _game.GetBattlegroundsHeroPickParams();

			CollectionAssert.AreEqual(new[] { DbfId(Neutral.EdwinVancleefTavernBrawl), DbfId(Neutral.TheRatKingTavernBrawl) }, rerollParams.HeroDbfIds);
			Assert.IsTrue(rerollParams.IsReroll);
			Assert.AreEqual("initial-ref", rerollParams.HeroPickRef);
			CollectionAssert.AreEquivalent(Races.Cast<int>().ToArray(), rerollParams.BattlegroundsRaces);
		}

		[TestMethod]
		public void HeroRerollParams_AreNotCachedWithoutInitialParams()
		{
			_game.SnapshotBattlegroundsOfferedHeroes(new[]
			{
				CreateHero(1, Neutral.EdwinVancleefTavernBrawl, 1),
				CreateHero(2, Neutral.TheRatKingTavernBrawl, 2),
			});

			_game.CacheBattlegroundsHeroRerollParams();

			Assert.IsNull(_game.GetBattlegroundsHeroPickParams());
		}
	}
}
