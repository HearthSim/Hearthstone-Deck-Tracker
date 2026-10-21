using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.CardExtraInfo;
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

		// From a real game: Shadow Sculpt assembled a Grimestreet Outfitter, offering Eyemaul and
		// Veteran Warmedic. Each half is its own choice entity, marked by setting the half it does
		// not provide to 1.
		private const string Target = "CORE_CFM_753";
		private const string Eyemaul = "BE_349";
		private const string Warmedic = "CORE_BAR_878";

		private static Entity Choice(int id, string cardId, GameTag marker)
		{
			var entity = new Entity(id) { CardId = cardId };
			entity.SetTag(marker, 1);
			return entity;
		}

		private static readonly Entity[] Choices =
		{
			Choice(121, Eyemaul, GameTag.CRAFTABLE_SECOND_HALF_DBID),
			Choice(122, Eyemaul, GameTag.CRAFTABLE_FIRST_HALF_DBID),
			Choice(123, Warmedic, GameTag.CRAFTABLE_SECOND_HALF_DBID),
			Choice(124, Warmedic, GameTag.CRAFTABLE_FIRST_HALF_DBID),
		};

		[DataTestMethod]
		[DataRow(0, 1, 121)]
		[DataRow(0, 2, 122)]
		[DataRow(1, 1, 123)]
		[DataRow(1, 2, 124)]
		public void FindsChoiceBehindHalfButton(int choiceIndex, int half, int expectedEntityId)
		{
			Assert.AreEqual(expectedEntityId, RelatedCardsManager.GetAssembleChoice(Choices, choiceIndex, half)?.Id);
		}

		[DataTestMethod]
		[DataRow(-1, 1)]
		[DataRow(2, 1)]
		[DataRow(0, 0)]
		[DataRow(0, 3)]
		public void NoChoiceOutsideTheButtons(int choiceIndex, int half)
		{
			Assert.IsNull(RelatedCardsManager.GetAssembleChoice(Choices, choiceIndex, half));
		}

		[TestMethod]
		public void EffectHalfChoiceBringsItsOwnEffect()
		{
			var target = new Entity(120) { CardId = Target };
			Assert.AreEqual(Eyemaul, RelatedCardsManager.GetAssembleChoiceEffectCardId(Choices[1], target));
		}

		[TestMethod]
		public void TriggerHalfChoiceKeepsTargetEffect()
		{
			var target = new Entity(120) { CardId = Target };
			Assert.AreEqual(Target, RelatedCardsManager.GetAssembleChoiceEffectCardId(Choices[0], target));
		}

		[TestMethod]
		public void TriggerHalfChoiceKeepsAlreadyAssembledTargetEffect()
		{
			var target = new Entity(120) { CardId = Target };
			target.SetTag(GameTag.CRAFTABLE_SECOND_HALF_DBID, DbfId(EffectHalfCardId));
			Assert.AreEqual(EffectHalfCardId, RelatedCardsManager.GetAssembleChoiceEffectCardId(Choices[2], target));
		}

		private static Entity Outfitter(int firstHalfDbfId, int secondHalfDbfId)
		{
			var entity = new Entity(120) { CardId = Target };
			entity.SetTag(GameTag.CRAFTABLE_FIRST_HALF_DBID, firstHalfDbfId);
			entity.SetTag(GameTag.CRAFTABLE_SECOND_HALF_DBID, secondHalfDbfId);
			return entity;
		}

		[TestMethod]
		public void UnassembledCardHasNoHalves()
		{
			Assert.IsNull(AssembledHalves.FromEntity(new Entity(120) { CardId = Target }));
		}

		[TestMethod]
		public void ChoiceEntityIsNotAnAssembledCard()
		{
			foreach(var choice in Choices)
				Assert.IsNull(AssembledHalves.FromEntity(choice));
		}

		[TestMethod]
		public void AssembledCardIsToldApartByItsHalves()
		{
			var assembled = Outfitter(0, DbfId(Eyemaul));
			Assert.AreEqual(new AssembledHalves(0, DbfId(Eyemaul)), assembled.Info.ExtraInfo);
			Assert.AreEqual(assembled.Info.ExtraInfo, Outfitter(0, DbfId(Eyemaul)).Info.ExtraInfo);
			Assert.AreNotEqual(assembled.Info.ExtraInfo, Outfitter(0, DbfId(Warmedic)).Info.ExtraInfo);
		}

		[TestMethod]
		public void SetExtraInfoWinsOverHalves()
		{
			var assembled = Outfitter(0, DbfId(Eyemaul));
			var counter = new IncindiusCounter(1);
			assembled.Info.ExtraInfo = counter;
			Assert.AreSame(counter, assembled.Info.ExtraInfo);
		}

		[TestMethod]
		public void AssembledTextTakesTheEffectHalf()
		{
			var halves = new AssembledHalves(0, DbfId(Eyemaul));
			Assert.AreEqual("<b>Battlecry:</b> summon two 2/2 Elementals with <b>Rush</b>.", halves.GetText(Database.GetCardFromId(Target)));
		}

		[TestMethod]
		public void AssembledTextTakesTheTriggerHalf()
		{
			var halves = new AssembledHalves(DbfId(Eyemaul), 0);
			Assert.AreEqual("After your hero attacks, Give all minions in your hand +1/+1.", halves.GetText(Database.GetCardFromId(Target)));
		}

		[TestMethod]
		public void AssembledRelatedCardsComeFromTheEffectHalf()
		{
			Assert.AreEqual(Eyemaul, new AssembledHalves(0, DbfId(Eyemaul)).EffectCardId(Target));
			Assert.AreEqual(Target, new AssembledHalves(DbfId(Eyemaul), 0).EffectCardId(Target));
		}
	}
}
