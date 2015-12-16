using System;
using System.Collections.Generic;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.BoardDamage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.BoardDamage
{
	[TestClass]
	public class EntityHelperTest
	{
		private Entity _heroA;
		private Entity _heroB;
		private Dictionary<int, Entity> _entities;

		[TestInitialize]
		public void Setup()
		{
			_entities = new Dictionary<int, Entity>();
			_entities[0] = new Entity(0);
			_entities[0].SetTag(GAME_TAG.FIRST_PLAYER, 1);
			_entities[0].IsPlayer = true;
			_entities[1] = new Entity(1);
			_entities[1].Name = "GameEntity";
			_entities[1].SetTag(GAME_TAG.TURN, 11);

			_heroA = new Entity(4);
			_heroA.CardId = "HERO_08";
			_heroA.Name = null;
			_heroA.IsPlayer = false;
			_heroA.SetTag(GAME_TAG.HEALTH, 30);
			_heroA.SetTag(GAME_TAG.ZONE, 1);
			_heroA.SetTag(GAME_TAG.CONTROLLER, 1);
			_heroA.SetTag(GAME_TAG.ENTITY_ID, 4);
			_heroA.SetTag(GAME_TAG.CARDTYPE, 3);
			_heroA.SetTag(GAME_TAG.DAMAGE, 7);
			_heroA.SetTag(GAME_TAG.ARMOR, 0);

			_heroB = new Entity(36);
			_heroB.CardId = "HERO_03";
			_heroB.Name = null;
			_heroB.IsPlayer = false;
			_heroB.SetTag(GAME_TAG.HEALTH, 30);
			_heroB.SetTag(GAME_TAG.ZONE, 1);
			_heroB.SetTag(GAME_TAG.CONTROLLER, 2);
			_heroB.SetTag(GAME_TAG.ENTITY_ID, 36);
			_heroB.SetTag(GAME_TAG.CARDTYPE, 3);
			_heroB.SetTag(GAME_TAG.DAMAGE, 14);
			_heroB.SetTag(GAME_TAG.ARMOR, 0);
			_heroB.SetTag(GAME_TAG.ATK, 1);
			_heroB.SetTag(GAME_TAG.EXHAUSTED, 1);
		}

		[TestMethod]
		public void ItIsPlayersTurn()
		{
			Assert.IsTrue(EntityHelper.IsPlayersTurn(_entities));
		}

		[TestMethod]
		public void IsHeroEntity()
		{
			Assert.IsTrue(EntityHelper.IsHero(_heroA));
		}

		[TestMethod]
		public void IsNotHeroEntity()
		{
			var card = new EntityBuilder("", 0, 0).ToEntity();
			Assert.IsFalse(EntityHelper.IsHero(card));
		}

		[TestMethod]
		public void OneOpponentHeroAndOnePlayerHero()
		{
			_heroA.IsPlayer = true;
			var ents = new Dictionary<int, Entity>()
			{
				{4, _heroA}, {36, _heroB}
			};

			Assert.IsNotNull(EntityHelper.GetHeroEntity(true, ents, 1));
		}

		[TestMethod]
		public void BothHerosAreMarkedAsNotPlayer()
		{
			var ents = new Dictionary<int, Entity>()
			{
				{4, _heroA}, {36, _heroB}
			};

			Assert.IsNotNull(EntityHelper.GetHeroEntity(true, ents, 1));
		}
	}
}
