using HearthDb.Enums;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class GameTypeResolutionTest
	{
		[TestMethod]
		public void DoesNotTreatUnresolvedGameTypeAsTraditionalDuringGameplay()
		{
			Core._game = null;
			var game = Core._game = new GameV2();
			game.CurrentMode = Mode.GAMEPLAY;

			Assert.AreEqual(GameType.GT_UNKNOWN, game.CurrentGameType);
			Assert.IsFalse(game.IsTraditionalHearthstoneMatch);
			Assert.IsFalse(game.IsBattlegroundsMatch);
			Assert.IsFalse(game.IsMercenariesMatch);
		}
	}
}
