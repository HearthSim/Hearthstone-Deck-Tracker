using System.Linq;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.Settings;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem.Settings;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone.RelatedCardsSystem
{
	/// <summary>
	/// The runtime chokepoint (RelatedCardsManager.GetCardsOpponentMayHave) is not exercised end to
	/// end here: running the real heuristics reads Core.Game, and constructing a GameV2 builds a WPF
	/// overlay window that cannot exist in a headless run. The override resolution it delegates to is
	/// pure, so that is what is covered instead.
	/// </summary>
	[TestClass]
	public class RelatedCardVisibilitySettingsTests
	{
		private const string CardId = HearthDb.CardIds.Collectible.Demonhunter.JaceDarkweaver;

		private int _saves;

		[TestInitialize]
		public void TestInitialize()
		{
			Config.Instance.RelatedCardVisibilityOverrides.Clear();
			RelatedCardVisibilitySettings.Instance.Invalidate();
			_saves = 0;
			RelatedCardVisibilitySettings.Instance.SaveConfig = () => _saves++;
		}

		[TestCleanup]
		public void TestCleanup()
		{
			Config.Instance.RelatedCardVisibilityOverrides.Clear();
			RelatedCardVisibilitySettings.Instance.Invalidate();
			RelatedCardVisibilitySettings.Instance.SaveConfig = Config.Save;
		}

		[DataTestMethod]
		[DataRow(CounterVisibility.Auto, true, true)]
		[DataRow(CounterVisibility.Auto, false, false)]
		[DataRow(CounterVisibility.Enabled, true, true)]
		[DataRow(CounterVisibility.Enabled, false, true)]
		[DataRow(CounterVisibility.Disabled, true, false)]
		[DataRow(CounterVisibility.Disabled, false, false)]
		public void Resolve_OverrideWinsAndAutoFallsBackToTheHeuristic(CounterVisibility mode, bool heuristic, bool expected)
		{
			Assert.AreEqual(expected, RelatedCardVisibilitySettings.Resolve(mode, () => heuristic));
		}

		[TestMethod]
		public void Resolve_OnlyEvaluatesTheHeuristicForAuto()
		{
			// Heuristics read game state, so an explicit choice must not pay for (or depend on) one.
			var calls = 0;
			bool Heuristic() { calls++; return true; }

			RelatedCardVisibilitySettings.Resolve(CounterVisibility.Enabled, Heuristic);
			RelatedCardVisibilitySettings.Resolve(CounterVisibility.Disabled, Heuristic);
			Assert.AreEqual(0, calls);

			RelatedCardVisibilitySettings.Resolve(CounterVisibility.Auto, Heuristic);
			Assert.AreEqual(1, calls);
		}

		[TestMethod]
		public void UnknownCard_IsAuto()
		{
			Assert.AreEqual(CounterVisibility.Auto, RelatedCardVisibilitySettings.Instance.GetOpponent(CardId));
		}

		[TestMethod]
		public void Set_StoresSparselyAndSaves()
		{
			var settings = RelatedCardVisibilitySettings.Instance;

			settings.SetOpponent(CardId, CounterVisibility.Enabled);

			Assert.AreEqual(CounterVisibility.Enabled, settings.GetOpponent(CardId));
			Assert.AreEqual(1, Config.Instance.RelatedCardVisibilityOverrides.Count);
			Assert.AreEqual(1, _saves);
			Assert.IsTrue(settings.HasAnyOverride);
		}

		[TestMethod]
		public void Set_BackToAuto_RemovesTheEntry()
		{
			var settings = RelatedCardVisibilitySettings.Instance;
			settings.SetOpponent(CardId, CounterVisibility.Disabled);

			settings.SetOpponent(CardId, CounterVisibility.Auto);

			Assert.AreEqual(CounterVisibility.Auto, settings.GetOpponent(CardId));
			Assert.AreEqual(0, Config.Instance.RelatedCardVisibilityOverrides.Count);
			Assert.IsFalse(settings.HasAnyOverride);
		}

		[TestMethod]
		public void Set_SameValueTwice_SavesAndNotifiesOnce()
		{
			var settings = RelatedCardVisibilitySettings.Instance;
			var changes = 0;
			settings.Changed += (s, e) => changes++;

			settings.SetOpponent(CardId, CounterVisibility.Enabled);
			settings.SetOpponent(CardId, CounterVisibility.Enabled);

			Assert.AreEqual(1, _saves);
			Assert.AreEqual(1, changes);
		}

		[TestMethod]
		public void Set_EmptyCardId_IsIgnored()
		{
			RelatedCardVisibilitySettings.Instance.SetOpponent("", CounterVisibility.Enabled);

			Assert.AreEqual(0, Config.Instance.RelatedCardVisibilityOverrides.Count);
			Assert.AreEqual(0, _saves);
		}

		[TestMethod]
		public void ResetAll_ClearsKnownCardsAndKeepsEntriesFromNewerVersions()
		{
			var settings = RelatedCardVisibilitySettings.Instance;
			settings.SetOpponent(CardId, CounterVisibility.Enabled);
			Config.Instance.RelatedCardVisibilityOverrides.Add(new RelatedCardVisibilityOverride
			{
				CardId = "CardFromANewerVersion",
				Opponent = CounterVisibility.Enabled,
			});
			settings.Invalidate();

			settings.ResetAll();

			Assert.AreEqual(CounterVisibility.Auto, settings.GetOpponent(CardId));
			Assert.AreEqual("CardFromANewerVersion", Config.Instance.RelatedCardVisibilityOverrides.Single().CardId);
		}

		[TestMethod]
		public void GetOpponentCardIds_ReturnsOnlyKnownCardsWithThatMode()
		{
			var settings = RelatedCardVisibilitySettings.Instance;
			settings.SetOpponent(CardId, CounterVisibility.Enabled);
			Config.Instance.RelatedCardVisibilityOverrides.Add(new RelatedCardVisibilityOverride
			{
				CardId = "CardFromANewerVersion",
				Opponent = CounterVisibility.Enabled,
			});
			settings.Invalidate();

			CollectionAssert.AreEqual(new[] { CardId }, settings.GetOpponentCardIds(CounterVisibility.Enabled).ToArray());
			Assert.AreEqual(0, settings.GetOpponentCardIds(CounterVisibility.Disabled).Count());
			Assert.AreEqual(0, settings.GetOpponentCardIds(CounterVisibility.Auto).Count());
		}

		[TestMethod]
		public void Catalog_ListsRegisteredRelatedCardsWithoutTheDefaultPlaceholder()
		{
			var known = RelatedCardCatalog.KnownCardIds;

			Assert.IsTrue(known.Contains(CardId));
			Assert.IsFalse(known.Contains(""));
		}

		// Arfus: the original and the Core reprint are one card with two ids.
		private const string ArfusOriginal = "ICC_854";
		private const string ArfusCore = "CORE_ICC_854";

		[TestMethod]
		public void Catalog_FoldsTheIdsOfOneCardIntoOneRow()
		{
			var rows = RelatedCardCatalog.Descriptors.Where(d => d.CardIds.Contains(ArfusOriginal)).ToList();

			Assert.AreEqual(1, rows.Count);
			CollectionAssert.AreEquivalent(new[] { ArfusOriginal, ArfusCore }, rows[0].CardIds.ToArray());
			Assert.AreEqual(rows[0].CardId, rows[0].CardIds[0]);
		}

		[TestMethod]
		public void Catalog_EveryRegisteredCardIsInExactlyOneRow()
		{
			var allIds = RelatedCardCatalog.Descriptors.SelectMany(d => d.CardIds).ToList();

			Assert.AreEqual(allIds.Count, allIds.Distinct().Count());
			CollectionAssert.AreEquivalent(RelatedCardCatalog.KnownCardIds.ToList(), allIds);
			Assert.IsTrue(RelatedCardCatalog.Descriptors.Count < RelatedCardCatalog.KnownCardIds.Count);
		}

		[TestMethod]
		public void Catalog_KeepsSameNamedCardsThatReallyDifferApart()
		{
			// The minion and the 8/8 token it becomes share a name but are not the same card.
			var geist = "MIS_006";
			var geistToken = "MIS_006t";

			CollectionAssert.DoesNotContain(RelatedCardCatalog.GetVariantIds(geist).ToList(), geistToken);
		}

		[TestMethod]
		public void OverrideAppliesToEveryIdOfTheCard()
		{
			var settings = RelatedCardVisibilitySettings.Instance;

			settings.SetOpponent(ArfusOriginal, CounterVisibility.Enabled);

			Assert.AreEqual(CounterVisibility.Enabled, settings.GetOpponent(ArfusOriginal));
			Assert.AreEqual(CounterVisibility.Enabled, settings.GetOpponent(ArfusCore));
			Assert.AreEqual(1, Config.Instance.RelatedCardVisibilityOverrides.Count);
		}

		[TestMethod]
		public void OverrideIsStoredUnderTheRepresentativeWhicheverIdWasUsed()
		{
			var representative = RelatedCardCatalog.GetVariantIds(ArfusOriginal)[0];
			var other = representative == ArfusOriginal ? ArfusCore : ArfusOriginal;

			RelatedCardVisibilitySettings.Instance.SetOpponent(other, CounterVisibility.Disabled);

			Assert.AreEqual(representative, Config.Instance.RelatedCardVisibilityOverrides.Single().CardId);
		}

		[TestMethod]
		public void Set_BackToAuto_ClearsEveryIdOfTheCard()
		{
			var settings = RelatedCardVisibilitySettings.Instance;
			settings.SetOpponent(ArfusOriginal, CounterVisibility.Enabled);

			settings.SetOpponent(ArfusCore, CounterVisibility.Auto);

			Assert.AreEqual(CounterVisibility.Auto, settings.GetOpponent(ArfusOriginal));
			Assert.AreEqual(CounterVisibility.Auto, settings.GetOpponent(ArfusCore));
			Assert.AreEqual(0, Config.Instance.RelatedCardVisibilityOverrides.Count);
		}

		[TestMethod]
		public void EntryUnderANonRepresentativeId_StillAppliesAndIsFoldedOnTheNextChange()
		{
			// e.g. saved before a reprint joined the card, or when another id represented it.
			var representative = RelatedCardCatalog.GetVariantIds(ArfusOriginal)[0];
			var other = representative == ArfusOriginal ? ArfusCore : ArfusOriginal;
			Config.Instance.RelatedCardVisibilityOverrides.Add(new RelatedCardVisibilityOverride
			{
				CardId = other,
				Opponent = CounterVisibility.Disabled,
			});
			var settings = RelatedCardVisibilitySettings.Instance;
			settings.Invalidate();

			Assert.AreEqual(CounterVisibility.Disabled, settings.GetOpponent(representative));
			Assert.AreEqual(CounterVisibility.Disabled, settings.GetOpponent(other));

			settings.SetOpponent(representative, CounterVisibility.Enabled);

			var entry = Config.Instance.RelatedCardVisibilityOverrides.Single();
			Assert.AreEqual(representative, entry.CardId);
			Assert.AreEqual(CounterVisibility.Enabled, entry.Opponent);
		}

		[TestMethod]
		public void GetOpponentCardIds_ReportsACardOnceHoweverManyIdsItHas()
		{
			var settings = RelatedCardVisibilitySettings.Instance;
			settings.SetOpponent(ArfusOriginal, CounterVisibility.Enabled);
			Config.Instance.RelatedCardVisibilityOverrides.Add(new RelatedCardVisibilityOverride
			{
				CardId = RelatedCardCatalog.GetVariantIds(ArfusOriginal)[1],
				Opponent = CounterVisibility.Enabled,
			});
			settings.Invalidate();

			Assert.AreEqual(1, settings.GetOpponentCardIds(CounterVisibility.Enabled).Count());
		}

		/// <summary>
		/// Guards the other catalog assertions from passing vacuously: a related card the card
		/// database cannot resolve is skipped from the page rather than failing, so it has to be
		/// caught here.
		/// </summary>
		[TestMethod]
		public void Catalog_HasNoCardsMissingFromTheDatabase()
		{
			CollectionAssert.AreEqual(new string[0], RelatedCardCatalog.MissingCardIds.ToArray());
			Assert.AreEqual(RelatedCardCatalog.KnownCardIds.Count, RelatedCardCatalog.Descriptors.SelectMany(d => d.CardIds).Count());
		}
	}
}
