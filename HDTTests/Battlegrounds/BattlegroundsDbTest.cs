using System.Collections.Generic;
using System.Linq;
using HearthDb;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Battlegrounds
{
	[TestClass]
	public class BattlegroundsDbTest
	{
		private static readonly Race TribeInCardData = Race.NAGA;

		private static readonly RemoteData.MetaPeriod PeriodWithoutTheTribe = new RemoteData.MetaPeriod
		{
			MinionTypes = new List<Race> { Race.BEAST, Race.MURLOC },
		};

		[TestMethod]
		public void TribeMissingFromTheMetaPeriodIsNotInRacesButItsMinionsStayReachable()
		{
			var db = new BattlegroundsDb(PeriodWithoutTheTribe);

			CollectionAssert.IsSubsetOf(new[] { Race.BEAST, Race.MURLOC, Race.INVALID, Race.ALL }, db.Races.ToList());
			CollectionAssert.DoesNotContain(db.Races.ToList(), TribeInCardData);
			Assert.IsTrue(db.GetCardsByRaces(new[] { TribeInCardData }, false).Any());
		}

		[TestMethod]
		public void RacesFallBackToTheCardDataWithoutAMetaPeriod()
		{
			var db = new BattlegroundsDb(null);

			CollectionAssert.Contains(db.Races.ToList(), TribeInCardData);
		}

		private static readonly BattlegroundsKeyword AnyPoolSpell = new TagKeyword(GameTag.TECH_LEVEL, "");

		[TestMethod]
		public void KeywordSpellsIncludeDuosExclusiveSpellsOnlyInDuos()
		{
			var db = new BattlegroundsDb(null);
			var duosSpell = Cards.All.Values.FirstOrDefault(x =>
				x.Type == CardType.BATTLEGROUND_SPELL
				&& x.Entity.GetTag(GameTag.IS_BACON_POOL_SPELL) == 1
				&& x.Entity.GetTag(GameTag.IS_BACON_DUOS_EXCLUSIVE) > 0
			);
			if(duosSpell == null)
				Assert.Inconclusive("no Duos-exclusive spell in the card data");

			CollectionAssert.Contains(db.GetSpells(AnyPoolSpell, true).Select(x => x.DbfId).ToList(), duosSpell.DbfId);
			CollectionAssert.DoesNotContain(db.GetSpells(AnyPoolSpell, false).Select(x => x.DbfId).ToList(), duosSpell.DbfId);
		}

		private static RemoteData.MetaPeriod PeriodWithTavernPool(params int[] dbfIds) => new RemoteData.MetaPeriod
		{
			TavernPool = dbfIds.Select(x => new RemoteData.CardInfo { DbfId = x }).ToList(),
		};

		[TestMethod]
		public void TavernPoolOnlyListsItsCardsRegardlessOfGameModeExclusivity()
		{
			var assembled = new BattlegroundsDb(null);
			var poolMinions = assembled.GetCardsByRaces(assembled.Races, false);
			var listedMinion = poolMinions.First();
			var unlistedMinion = poolMinions.First(x => x.DbfId != listedMinion.DbfId);
			var duosMinion = Cards.All.Values.FirstOrDefault(x =>
				x.Type == CardType.MINION
				&& x.Entity.GetTag(GameTag.TECH_LEVEL) > 0
				&& x.Entity.GetTag(GameTag.IS_BACON_POOL_MINION) == 1
				&& x.Entity.GetTag(GameTag.IS_BACON_DUOS_EXCLUSIVE) > 0
			);
			if(duosMinion == null)
				Assert.Inconclusive("no Duos-exclusive minion in the card data");
			var spell = assembled.GetSpells(false).First();

			var db = new BattlegroundsDb(PeriodWithTavernPool(listedMinion.DbfId, duosMinion.DbfId, spell.DbfId));

			var minions = db.GetCardsByRaces(db.Races, false).Select(x => x.DbfId).ToList();
			CollectionAssert.AreEquivalent(new[] { listedMinion.DbfId, duosMinion.DbfId }, minions.Distinct().ToList());
			CollectionAssert.DoesNotContain(minions, unlistedMinion.DbfId);
			CollectionAssert.AreEqual(new[] { spell.DbfId }, db.GetSpells(false).Select(x => x.DbfId).ToList());
		}

		[TestMethod]
		public void TavernPoolWithoutAKnownMinionFallsBackToTheCardData()
		{
			var assembled = new BattlegroundsDb(null);
			var assembledMinions = assembled.GetCardsByRaces(assembled.Races, false).Select(x => x.DbfId).Distinct().ToList();
			var spell = assembled.GetSpells(false).First();

			foreach(var period in new[] { PeriodWithTavernPool(), PeriodWithTavernPool(spell.DbfId, int.MaxValue) })
			{
				var db = new BattlegroundsDb(period);

				CollectionAssert.AreEquivalent(assembledMinions, db.GetCardsByRaces(db.Races, false).Select(x => x.DbfId).Distinct().ToList());
				CollectionAssert.AreEquivalent(assembled.GetSpells(false).Select(x => x.DbfId).ToList(), db.GetSpells(false).Select(x => x.DbfId).ToList());
			}
		}
	}
}
