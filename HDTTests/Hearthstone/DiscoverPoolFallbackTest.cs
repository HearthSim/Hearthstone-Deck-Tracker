using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.DemonHunter;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Hunter;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Mage;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Cards.Priest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone
{
	/// <summary>
	/// A Discover whose pool is empty for the player's class falls back to the card's own class,
	/// mirroring the game: a Mage holding Hive Map ("Discover a Fel spell", Demon Hunter) is still
	/// offered Demon Hunter Fel spells rather than nothing.
	///
	/// These exercise DiscoverPoolCard.GetBasePool directly rather than the public GetRelatedCards.
	/// GetRelatedCards resolves the game type and format through PoolContext, which reads Core.Game,
	/// and constructing a GameV2 builds Core.Overlay - a real WPF window that cannot be created in a
	/// headless test run. Calling GetBasePool with an explicit game type and format covers the pool
	/// build, the legality filter and the fallback, which is the logic under test here.
	///
	/// They read the live card DB, so a rotation or a newly printed card can move these numbers.
	/// </summary>
	[TestClass]
	public class DiscoverPoolFallbackTest
	{
		private const GameType Gt = GameType.GT_RANKED;

		private static readonly MethodInfo GetBasePool = typeof(DiscoverPoolCard)
			.GetMethod("GetBasePool", BindingFlags.NonPublic | BindingFlags.Instance);

		private static string[] PoolFor(DiscoverPoolCard card, string playerClass, FormatType format)
		{
			var player = new Player(null, true) { CurrentClass = playerClass };
			var pool = (List<Card>)GetBasePool.Invoke(card, new object[] { player, Gt, format });
			return pool.Select(c => c.Id).OrderBy(id => id).ToArray();
		}

		[TestMethod]
		public void EmptyForPlayerClass_FallsBackToCardClass()
		{
			// No Fel spell is Standard-legal for a Mage, so the pool is empty and Hive Map falls
			// back to its own class.
			var asMage = PoolFor(new HiveMap(), "Mage", FormatType.FT_STANDARD);
			Assert.AreNotEqual(0, asMage.Length, "Hive Map in Mage must fall back, not show nothing");

			// The fallback resolves to the very pool a Demon Hunter player would be shown.
			CollectionAssert.AreEqual(PoolFor(new HiveMap(), "DemonHunter", FormatType.FT_STANDARD), asMage);
		}

		[TestMethod]
		public void FallbackIsFormatAware()
		{
			// Emptiness is judged after the legality filter, so the fallback follows the format.
			// Chaos Creation (DEEP_031) is a dual-class Mage/Warlock Fel spell: in Wild a Mage really
			// can discover it, so the pool is not empty and must NOT be replaced by the card's class.
			var wildMage = PoolFor(new HiveMap(), "Mage", FormatType.FT_WILD);
			var wildDh = PoolFor(new HiveMap(), "DemonHunter", FormatType.FT_WILD);

			Assert.AreNotEqual(0, wildMage.Length);
			CollectionAssert.AreNotEqual(wildDh, wildMage,
				"A non-empty Wild pool must be shown as-is rather than falling back");
			CollectionAssert.Contains(wildMage, "DEEP_031");
		}

		[TestMethod]
		public void NonEmptyForPlayerClass_DoesNotFallBack()
		{
			// Renew is a Priest card on the class+Neutral spell pool. Mage spells exist, so a Mage
			// holding it sees Mage spells - the fallback must not hijack a pool that has content.
			var asMage = PoolFor(new Renew(), "Mage", FormatType.FT_WILD);
			var asPriest = PoolFor(new Renew(), "Priest", FormatType.FT_WILD);

			Assert.AreNotEqual(0, asMage.Length);
			CollectionAssert.AreNotEqual(asPriest, asMage);
		}

		[TestMethod]
		public void FallbackIsPerCard_NotPerPool()
		{
			// SecretPlan (Hunter) and ArcaneKeysmith (Mage) share ClassOrNeutralSecretPool. Druid has
			// no Secrets, so both fall back - each to its OWN class. If the fallback were cached
			// against the player's class instead of the class the pool was built for, whichever ran
			// first would poison the shared entry and both would return the same list.
			var hunterCard = PoolFor(new SecretPlan(), "Druid", FormatType.FT_WILD);
			var mageCard = PoolFor(new ArcaneKeysmith(), "Druid", FormatType.FT_WILD);

			Assert.AreNotEqual(0, hunterCard.Length);
			Assert.AreNotEqual(0, mageCard.Length);
			CollectionAssert.AreNotEqual(mageCard, hunterCard);

			// ...and each matches what a player of that class would be shown.
			CollectionAssert.AreEqual(PoolFor(new SecretPlan(), "Hunter", FormatType.FT_WILD), hunterCard);
			CollectionAssert.AreEqual(PoolFor(new ArcaneKeysmith(), "Mage", FormatType.FT_WILD), mageCard);
		}
	}
}
