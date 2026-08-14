using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Session;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Hearthstone_Deck_Tracker.Utility.Battlegrounds.BattlegroundsLastGames;

namespace HDTTests.Battlegrounds
{
	[TestClass]
	public class BattlegroundsLastGamesTest
	{
		private static GameItem AddGame(BattlegroundsLastGames games, int rating, int ratingAfter, DateTime startTime)
		{
			games.AddGame(
				"1_2", startTime.ToString("o"), startTime.AddMinutes(20).ToString("o"), "TB_BaconShop_HERO_01",
				rating, ratingAfter, 3, Array.Empty<Entity>(), false, false, save: false
			);
			return games.Games.Last();
		}

		private static string Serialize(BattlegroundsLastGames games)
		{
			using(var writer = new StringWriter())
			{
				new XmlSerializer(typeof(BattlegroundsLastGames)).Serialize(writer, games);
				return writer.ToString();
			}
		}

		private static BattlegroundsLastGames Deserialize(string xml)
		{
			using(var reader = new StringReader(xml))
				return (BattlegroundsLastGames)new XmlSerializer(typeof(BattlegroundsLastGames)).Deserialize(reader);
		}

		[TestMethod]
		public void GameWithUnavailablePostGameRating_IsNotASeasonReset()
		{
			var games = new BattlegroundsLastGames();

			var game = AddGame(games, 665, 0, DateTime.Now);

			Assert.IsNull(game.RatingAfter);
			Assert.AreEqual(665, game.RatingAfterOrCarriedForward);
			Assert.IsFalse(game.SeasonReset);
		}

		[TestMethod]
		public void GameWithPostGameRating_KeepsIt()
		{
			var games = new BattlegroundsLastGames();

			var game = AddGame(games, 665, 712, DateTime.Now);

			Assert.AreEqual(712, game.RatingAfter);
			Assert.IsFalse(game.SeasonReset);
		}

		[TestMethod]
		public void GameRolledOverBySeasonReset_IsASeasonReset()
		{
			var games = new BattlegroundsLastGames();

			var game = AddGame(games, 665, 12, DateTime.Now);

			Assert.AreEqual(12, game.RatingAfter);
			Assert.IsTrue(game.SeasonReset);
		}

		[TestMethod]
		public void SeasonResetFollowingGameWithUnavailablePostGameRating_IsStillDetected()
		{
			var games = new BattlegroundsLastGames();

			var previous = AddGame(games, 665, 0, DateTime.Now.AddHours(-1));
			var next = AddGame(games, 12, 40, DateTime.Now);

			Assert.IsTrue(IsRatingReset(previous.RatingAfterOrCarriedForward, next.Rating));
		}

		[TestMethod]
		public void GameWithUnavailablePostGameRating_IsNotWrittenToDisk()
		{
			var games = new BattlegroundsLastGames();
			AddGame(games, 665, 0, DateTime.Now);

			Assert.IsFalse(Serialize(games).Contains("RatingAfter"));
		}

		[TestMethod]
		public void GameWrittenBeforeMissingRatingsWereDistinguishable_LoadsAsUnavailable()
		{
			var games = Deserialize(
				"<BgsLastGames><Game Player=\"1_2\" Rating=\"665\" RatingAfter=\"0\" Placemenent=\"3\" /></BgsLastGames>"
			);

			var game = games.Games.Single();

			Assert.IsNull(game.RatingAfter);
			Assert.IsFalse(game.SeasonReset);
		}

		[TestMethod]
		public void GameWithUnavailablePostGameRating_ShowsNoMMRDelta()
		{
			var games = new BattlegroundsLastGames();

			var viewModel = new BattlegroundsGameViewModel(AddGame(games, 665, 0, DateTime.Now));

			Assert.AreEqual("-", viewModel.MMRDeltaText);
		}

		[TestMethod]
		public void GameWithUnchangedRating_ShowsAZeroMMRDelta()
		{
			var games = new BattlegroundsLastGames();

			var viewModel = new BattlegroundsGameViewModel(AddGame(games, 665, 665, DateTime.Now));

			Assert.AreEqual("0", viewModel.MMRDeltaText);
		}
	}
}
