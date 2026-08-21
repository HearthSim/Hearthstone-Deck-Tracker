using System.Collections.Generic;
using System.Linq;
using HearthDb;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class BattlegroundsKeywordFilterTest
	{
		private const string LockboxLocKey = "Battlegrounds_Browser_Filter_Lockbox";

		private static BattlegroundsKeyword Lockbox => BattlegroundsUtils.GetAvailableKeywords(null)
			.Single(x => x is MentionedKeyword keyword && keyword.LocKey == LockboxLocKey);

		private static readonly HashSet<Race> WithoutPirates = new HashSet<Race>
		{
			Race.MURLOC, Race.DEMON, Race.MECHANICAL, Race.BEAST, Race.DRAGON,
		};

		private static readonly HashSet<Race> WithPirates = new HashSet<Race>(WithoutPirates) { Race.PIRATE };

		[TestMethod]
		public void LockboxFilterIsHiddenInALobbyWithoutPirates()
		{
			CollectionAssert.DoesNotContain(BattlegroundsUtils.GetAvailableKeywords(WithoutPirates), Lockbox);
		}

		[TestMethod]
		public void LockboxFilterIsShownInALobbyWithPirates()
		{
			CollectionAssert.Contains(BattlegroundsUtils.GetAvailableKeywords(WithPirates), Lockbox);
		}

		[TestMethod]
		public void UngatedFiltersSurviveAMissingMinionType()
		{
			var gated = BattlegroundsUtils.GetAvailableKeywords(WithoutPirates);
			var ungated = BattlegroundsUtils.GetAvailableKeywords(null).Where(x => x.RequiredRace is null);

			CollectionAssert.AreEquivalent(ungated.ToList(), gated);
		}

		[TestMethod]
		public void EveryLockboxCardInThePoolIsAPirate()
		{
			var matching = Cards.All.Values
				.Where(x => x.Entity.GetTag(GameTag.TECH_LEVEL) > 0)
				.Where(x =>
					x.Entity.GetTag(GameTag.IS_BACON_POOL_MINION) == 1
					|| x.Entity.GetTag(GameTag.IS_BACON_POOL_SPELL) == 1
				)
				.Where(x => Lockbox.Matches(x.Entity.GetTag, x.GetLocText(Locale.enUS)))
				.ToList();

			Assert.IsTrue(matching.Count > 0, "the Lockbox keyword no longer matches any card in the pool");
			CollectionAssert.AreEqual(new[] { Race.PIRATE }, matching.Select(x => x.Race).Distinct().ToArray());
		}
	}
}
