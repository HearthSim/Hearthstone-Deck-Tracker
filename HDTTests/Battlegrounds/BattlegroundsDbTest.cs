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
	}
}
