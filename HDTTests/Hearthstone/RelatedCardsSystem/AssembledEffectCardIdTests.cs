using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone.RelatedCardsSystem
{
	[TestClass]
	public class AssembledEffectCardIdTests
	{
		// From a real game: Sandcrafter assembled a Yesterloc whose effect half came from CORE_REV_308
		// ("Battlecry: Summon a random 2-Cost minion").
		private const string AssembledCardId = "TIME_428";
		private const string EffectHalfCardId = "CORE_REV_308";
		private const string TriggerHalfCardId = "CORE_RLK_083";

		private static Entity Assembled(int firstHalfDbfId, int secondHalfDbfId)
		{
			var entity = new Entity(1) { CardId = AssembledCardId };
			entity.SetTag(GameTag.CRAFTABLE_FIRST_HALF_DBID, firstHalfDbfId);
			entity.SetTag(GameTag.CRAFTABLE_SECOND_HALF_DBID, secondHalfDbfId);
			return entity;
		}

		private static int DbfId(string cardId) => HearthDb.Cards.All[cardId].DbfId;

		[TestMethod]
		public void ResolvesToEffectHalf()
		{
			var entity = Assembled(DbfId(TriggerHalfCardId), DbfId(EffectHalfCardId));
			Assert.AreEqual(EffectHalfCardId, RelatedCardsManager.GetEffectCardId(entity, AssembledCardId));
		}

		[TestMethod]
		public void TriggerHalfAloneKeepsOwnCardId()
		{
			var entity = Assembled(DbfId(TriggerHalfCardId), 0);
			Assert.AreEqual(AssembledCardId, RelatedCardsManager.GetEffectCardId(entity, AssembledCardId));
		}

		[DataTestMethod]
		[DataRow(0)]
		[DataRow(1)]
		public void HalfOnOfferOrMissingIsNotACard(int secondHalfDbfId)
		{
			var entity = Assembled(0, secondHalfDbfId);
			Assert.AreEqual(AssembledCardId, RelatedCardsManager.GetEffectCardId(entity, AssembledCardId));
		}

		[TestMethod]
		public void MismatchedEntityKeepsHoveredCardId()
		{
			// Hover entities are found by zone position, so the entity may not be the hovered card.
			var entity = Assembled(0, DbfId(EffectHalfCardId));
			Assert.AreEqual("EX1_001", RelatedCardsManager.GetEffectCardId(entity, "EX1_001"));
		}

		[TestMethod]
		public void NoEntityKeepsHoveredCardId()
		{
			Assert.AreEqual(AssembledCardId, RelatedCardsManager.GetEffectCardId(null, AssembledCardId));
		}
	}
}
