using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HearthDb;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WPFLocalizeExtension.Engine;

namespace HDTTests.Battlegrounds
{
	[TestClass]
	public class BattlegroundsKeywordTest
	{
		private static IEnumerable<HearthDb.Card> BaconPool => Cards.All.Values.Where(x =>
			x.Entity.GetTag(GameTag.TECH_LEVEL) > 0 && x.Entity.GetTag(GameTag.IS_BACON_POOL_MINION) > 0);

		private static List<string> MatchingCardNames(BattlegroundsKeyword keyword) => BaconPool
			.Where(x => keyword.Matches(x.Entity.GetTag, x.GetLocText(Locale.enUS)))
			.Select(x => x.Name)
			.ToList();

		private static List<string> RebornCardNames(BattlegroundsDb db) => Enumerable.Range(1, 7)
			.SelectMany(tier => db.GetCards(tier, new TagKeyword(GameTag.REBORN, "GameTag_Reborn"), db.Races, false))
			.Select(x => x.Name)
			.ToList();

		private static void WithCulture(string culture, Action action)
		{
			var previous = LocalizeDictionary.Instance.Culture;
			LocalizeDictionary.Instance.Culture = new CultureInfo(culture);
			try
			{
				action();
			}
			finally
			{
				LocalizeDictionary.Instance.Culture = previous;
			}
		}

		[TestMethod]
		public void KeywordFilterMatchesTheSameCardsInEveryLanguage()
		{
			var db = new BattlegroundsDb();

			var english = new List<string>();
			WithCulture("en-US", () => english = RebornCardNames(db));

			Assert.IsTrue(english.Any());
			WithCulture("de-DE", () => CollectionAssert.AreEquivalent(english, RebornCardNames(db)));
		}

		[TestMethod]
		public void KeywordNameIsLocalizedWhileMatchingStaysEnglish()
		{
			WithCulture("de-DE", () =>
			{
				Assert.AreEqual("Todesröcheln", LocUtil.Get("GameTag_Deathrattle"));
				Assert.AreEqual("Deathrattle", LocUtil.GetEnglish("GameTag_Deathrattle"));
			});
		}

		[TestMethod]
		public void ActivateMatchesCardsDespiteHavingNoTagCoverage()
		{
			var activate = new TagKeyword(GameTag.BACON_ACTIVATE_TOOLTIP, "GameTag_BGActivate");

			Assert.IsFalse(BaconPool.Any(x => x.Entity.GetTag(GameTag.BACON_ACTIVATE_TOOLTIP) > 0));
			CollectionAssert.Contains(MatchingCardNames(activate), "Living Prison");
		}

		[TestMethod]
		public void LockboxMatchesTheCardsMentioningIt()
		{
			var lockbox = new MentionedKeyword("Battlegrounds_Browser_Filter_Lockbox");

			CollectionAssert.AreEquivalent(
				new[] { "Bilgewater Breakout", "Locked-up Mutineer", "Enterprising Escapee" },
				MatchingCardNames(lockbox)
			);
		}

		[TestMethod]
		public void AMissingStringDoesNotMatchEveryCard()
		{
			Assert.IsFalse(new MentionedKeyword("ThisStringDoesNotExist").Matches(_ => 0, "any card text"));
		}
	}
}
