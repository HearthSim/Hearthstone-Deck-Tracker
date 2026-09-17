using System.Collections.Generic;
using System.Linq;
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
	}
}
