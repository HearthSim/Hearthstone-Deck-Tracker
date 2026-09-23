using System.Collections.Generic;
using System.Linq;
using HearthDb;
using HearthDb.Enums;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Battlegrounds
{
	[TestClass]
	public class BattlegroundsDbMinionPoolTest
	{
		private static readonly BattlegroundsDb Fallback = new BattlegroundsDb(null);

		private static BattlegroundsMinionPoolEntry Minion(int dbfId, int tier, params Race[] races) => new BattlegroundsMinionPoolEntry
		{
			DbfId = dbfId,
			Tier = tier,
			CardType = (int)CardType.MINION,
			MinionTypes = races.Cast<int>().ToList(),
		};

		private static BattlegroundsMinionPoolEntry Spell(int dbfId, int tier) => new BattlegroundsMinionPoolEntry
		{
			DbfId = dbfId,
			Tier = tier,
			CardType = (int)CardType.BATTLEGROUND_SPELL,
			MinionTypes = new List<int>(),
		};

		private static BattlegroundsMinionPool Pool(IEnumerable<Race> activeRaces, params BattlegroundsMinionPoolEntry[] cards) => new BattlegroundsMinionPool
		{
			Cards = cards.ToList(),
			ActiveMinionTypes = activeRaces.Cast<int>().ToList(),
		};

		[TestMethod]
		public void PoolFilesCardsUnderTheServerTierAndMinionTypes()
		{
			var beast = Fallback.GetCards(1, Race.BEAST, false).First();
			var neutral = Fallback.GetCards(2, Race.INVALID, false).First();
			var leftOut = Fallback.GetCards(1, Race.BEAST, false).First(x => x.DbfId != beast.DbfId);
			var spell = Fallback.GetSpells(1, false).First();

			var db = BattlegroundsDb.FromMinionPool(Pool(
				new[] { Race.BEAST },
				Minion(beast.DbfId, 3, Race.BEAST),
				Minion(neutral.DbfId, 2, Race.INVALID),
				Spell(spell.DbfId, 4)
			), Fallback);

			CollectionAssert.Contains(db.GetCards(3, Race.BEAST, false).Select(x => x.DbfId).ToList(), beast.DbfId);
			Assert.IsFalse(db.GetCards(1, Race.BEAST, false).Any());
			CollectionAssert.Contains(db.GetCards(2, Race.INVALID, false).Select(x => x.DbfId).ToList(), neutral.DbfId);
			CollectionAssert.AreEquivalent(new[] { spell.DbfId }, db.GetSpells(4, false).Select(x => x.DbfId).ToList());
			Assert.IsFalse(db.GetSpells(1, false).Any());

			var available = db.GetCardsByRaces(new[] { Race.BEAST, Race.INVALID }, false).Select(x => x.DbfId).Distinct().ToList();
			CollectionAssert.AreEquivalent(new[] { beast.DbfId, neutral.DbfId }, available);
			CollectionAssert.DoesNotContain(available, leftOut.DbfId);
		}

		[TestMethod]
		public void BannedCardsAreShownFadedButAreNotAvailable()
		{
			var banned = Fallback.GetCards(1, Race.BEAST, false).First();
			var allowed = Fallback.GetCards(1, Race.BEAST, false).First(x => x.DbfId != banned.DbfId);
			var bannedSpell = Fallback.GetSpells(1, false).First();
			var bannedEntry = Minion(banned.DbfId, 1, Race.BEAST);
			bannedEntry.Banned = true;
			var bannedSpellEntry = Spell(bannedSpell.DbfId, 1);
			bannedSpellEntry.Banned = true;

			var db = BattlegroundsDb.FromMinionPool(Pool(
				new[] { Race.BEAST },
				bannedEntry,
				Minion(allowed.DbfId, 1, Race.BEAST),
				bannedSpellEntry
			), Fallback);

			var shown = db.GetCards(1, Race.BEAST, false);
			Assert.AreEqual(0, shown.Single(x => x.DbfId == banned.DbfId).Count);
			Assert.AreEqual(1, shown.Single(x => x.DbfId == allowed.DbfId).Count);
			Assert.AreEqual(0, db.GetSpells(1, false).Single().Count);

			CollectionAssert.AreEquivalent(new[] { allowed.DbfId }, db.GetCardsByRaces(new[] { Race.BEAST }, false).Select(x => x.DbfId).ToList());
			Assert.IsFalse(db.GetSpells(false).Any());
			Assert.IsTrue(db.IsBanned(banned.DbfId));
		}

		[TestMethod]
		public void PoolIgnoresDuosExclusivityAsTheServerAlreadySentTheModesPool()
		{
			var duosOnly = Cards.All.Values.FirstOrDefault(x =>
				x.Entity.GetTag(GameTag.IS_BACON_DUOS_EXCLUSIVE) > 0
				&& x.Entity.GetTag(GameTag.IS_BACON_POOL_MINION) == 1
			);
			if(duosOnly == null)
				Assert.Inconclusive("no Duos-exclusive minion in the card data");

			var db = BattlegroundsDb.FromMinionPool(Pool(new[] { Race.BEAST }, Minion(duosOnly.DbfId, 2, Race.INVALID)), Fallback);

			Assert.AreEqual(duosOnly.DbfId, db.GetCards(2, Race.INVALID, false).Single().DbfId);
			Assert.AreEqual(duosOnly.DbfId, db.GetCards(2, Race.INVALID, true).Single().DbfId);
		}

		[TestMethod]
		public void MinionsAreAlsoListedUnderActiveTribesTheyAreASubsetOf()
		{
			var subsetMinion = Cards.All.Values.FirstOrDefault(x =>
				x.Entity.GetTag(GameTag.BACON_SUBSET_MURLOC) > 0
				&& x.Race != Race.MURLOC
				&& x.SecondaryRace != Race.MURLOC
			);
			if(subsetMinion == null)
				Assert.Inconclusive("no minion with a Murloc subset tag in the card data");

			var withMurlocs = BattlegroundsDb.FromMinionPool(Pool(new[] { Race.MURLOC }, Minion(subsetMinion.DbfId, 3, Race.INVALID)), Fallback);
			var withoutMurlocs = BattlegroundsDb.FromMinionPool(Pool(new[] { Race.BEAST }, Minion(subsetMinion.DbfId, 3, Race.INVALID)), Fallback);

			Assert.AreEqual(subsetMinion.DbfId, withMurlocs.GetCards(3, Race.MURLOC, false).Single().DbfId);
			Assert.AreEqual(subsetMinion.DbfId, withMurlocs.GetCards(3, Race.INVALID, false).Single().DbfId);
			Assert.IsFalse(withoutMurlocs.GetCards(3, Race.MURLOC, false).Any());
		}

		[TestMethod]
		public void DarkParadoxPrefersThisGamesVariantOverTheGenericCard()
		{
			var generic = Cards.All[HearthDb.CardIds.NonCollectible.Neutral.DarkParadox];
			var variant = Cards.All[HearthDb.CardIds.NonCollectible.Neutral.DarkParadox_DarkParadoxToken2];
			var beast = Fallback.GetCards(1, Race.BEAST, false).First();

			var withVariant = BattlegroundsDb.FromMinionPool(Pool(
				new[] { Race.BEAST },
				Minion(generic.DbfId, 1, Race.INVALID),
				Minion(variant.DbfId, 5, Race.INVALID),
				Minion(beast.DbfId, 1, Race.BEAST)
			), Fallback);
			var genericOnly = BattlegroundsDb.FromMinionPool(Pool(new[] { Race.BEAST }, Minion(generic.DbfId, 1, Race.INVALID)), Fallback);
			var without = BattlegroundsDb.FromMinionPool(Pool(new[] { Race.BEAST }, Minion(beast.DbfId, 1, Race.BEAST)), Fallback);
			var bannedEntry = Minion(variant.DbfId, 5, Race.INVALID);
			bannedEntry.Banned = true;
			var banned = BattlegroundsDb.FromMinionPool(Pool(new[] { Race.BEAST }, bannedEntry), Fallback);

			Assert.AreEqual(variant.DbfId, withVariant.DarkParadox?.DbfId);
			Assert.AreEqual(5, withVariant.DarkParadoxTier);
			Assert.AreEqual(generic.DbfId, genericOnly.DarkParadox?.DbfId);
			Assert.IsNull(without.DarkParadox);
			Assert.IsNull(without.DarkParadoxTier);
			Assert.IsNull(banned.DarkParadox);
		}

		[TestMethod]
		public void RacesAndBuddiesComeFromTheFallback()
		{
			var db = BattlegroundsDb.FromMinionPool(Pool(new[] { Race.BEAST }), Fallback);

			CollectionAssert.IsSubsetOf(Fallback.Races.ToList(), db.Races.ToList());
			var buddyTier = Enumerable.Range(1, 6).First(tier => Fallback.GetBuddies(tier, false).Any());
			CollectionAssert.AreEqual(
				Fallback.GetBuddies(buddyTier, false).Select(x => x.DbfId).ToList(),
				db.GetBuddies(buddyTier, false).Select(x => x.DbfId).ToList()
			);
		}
	}
}
