using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.LogReader.Handlers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class TurnAttributionTest
	{
		[TestMethod]
		public void AttributesTurnStartToPlayer_WhenGhostEntityRenameIsDropped()
		{
			// A Bob ghost entity's rename from "Hero A" to "Hero B" never resolves, leaving it
			// with a stale CURRENT_PLAYER=1 while the player's own entity also gets CURRENT_PLAYER=1
			// at shop start.
			Core._game = null;
			var game = Core._game = new GameV2();
			game.Player.Id = 1;

			var playerEntity = new Entity(2) { Name = "Player#1234" };
			playerEntity.SetTag(GameTag.PLAYER_ID, 1);
			game.Entities.Add(2, playerEntity);

			var ghostEntity = new Entity(3) { Name = "Ghost" };
			ghostEntity.SetTag(GameTag.PLAYER_ID, 9);
			ghostEntity.SetTag(GameTag.BACON_DUMMY_PLAYER, 1);
			game.Entities.Add(3, ghostEntity);

			var gameEventHandler = new GameEventHandler(game);
			var gameState = new HsGameState(game);
			var powerHandler = new PowerHandler();

			powerHandler.Handle("TAG_CHANGE Entity=Hero A tag=CURRENT_PLAYER value=1", DateTime.Now, gameState, game);
			powerHandler.Handle("TAG_CHANGE Entity=Hero A tag=HERO_ENTITY value=13770", DateTime.Now, gameState, game);
			powerHandler.Handle("TAG_CHANGE Entity=Hero B tag=CURRENT_PLAYER value=0", DateTime.Now, gameState, game);
			powerHandler.Handle("TAG_CHANGE Entity=Player#1234 tag=CURRENT_PLAYER value=1", DateTime.Now, gameState, game);

			Assert.AreEqual(ActivePlayer.Player, gameEventHandler.CurrentTurnActivePlayer);
		}
	}
}
