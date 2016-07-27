#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hearthstone_Deck_Tracker;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HearthStats.API;
using Hearthstone_Deck_Tracker.Stats;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace HDTTests.HearthStats
{
	[TestClass]
	public class ApiTest
	{
		private static Deck _deck1;
		private static Deck _deck2;
		private static GameStats _match1;
		private static GameStats _match2;
		private static GameStats _match3;

		static ApiTest()
		{
			Config.Load();
			HearthStatsAPI.LoadCredentials();
			GenerateTestDecks();
			GenerateTestMatches();
		}

		private static void GenerateTestDecks()
		{
			_deck1 = new Deck();
			_deck1.Name = "Test1";
			_deck1.Url = "http://www.google.com";
			_deck1.Note = "test note 1";
			_deck1.Tags = new List<string> {"testtag1", "testtag2"};
			_deck1.Class = "Mage";
			foreach(var card in Database.GetActualCards().Where(c => c.PlayerClass == "Mage").Take(30))
				_deck1.Cards.Add(card);

			_deck2 = new Deck();
			_deck2.Name = "Test2";
			_deck2.Url = "http://www.amazon.com";
			_deck2.Note = "test note 2";
			_deck2.Tags = new List<string> {"testtag3", "testtag4"};
			_deck2.Class = "Druid";
			foreach(var card in Database.GetActualCards().Where(c => c.PlayerClass == "Druid").Take(30))
				_deck2.Cards.Add(card);
		}

		private static void GenerateTestMatches()
		{
			_match1 = new GameStats(GameResult.Win, "Druid", "Druid");
			_match1.Result = GameResult.Win;
			_match1.Rank = 20;
			_match1.Coin = true;
			_match1.DeckName = _deck1.Name;
			_match1.StartTime = DateTime.Now.AddMinutes(-5);
			_match1.EndTime = DateTime.Now;
			_match1.Region = Region.EU;
			_match1.GameMode = GameMode.Casual;
			_match1.PlayerName = "Epix";
			_match1.OpponentName = "trigun";
			_match1.Turns = 10;

			_match2 = new GameStats(GameResult.Win, "Priest", "Warlock");
			_match2.Result = GameResult.Win;
			_match2.Rank = 19;
			_match2.Coin = true;
			_match2.DeckName = _deck2.Name;
			_match2.StartTime = DateTime.Now.AddMinutes(-7);
			_match2.EndTime = DateTime.Now;
			_match2.Region = Region.US;
			_match2.GameMode = GameMode.Casual;
			_match2.PlayerName = "Epix";
			_match2.OpponentName = "trigun";
			_match2.Turns = 10;

			_match3 = new GameStats(GameResult.Win, "Mage", "Warrior");
			_match3.Result = GameResult.Win;
			_match3.Rank = 18;
			_match3.Coin = false;
			_match3.DeckName = _deck2.Name;
			_match3.StartTime = DateTime.Now.AddMinutes(-6);
			_match3.EndTime = DateTime.Now;
			_match3.Region = Region.ASIA;
			_match3.GameMode = GameMode.Casual;
			_match3.PlayerName = "Epix";
			_match3.OpponentName = "trigun";
			_match3.Turns = 10;
		}

		[TestMethod, TestCategory("Web")]
		public void DeckLifeCycle()
		{
			CreateDeck();
			CreateMatch();
			MoveMatch();
			MultiCreateMatch();
			DeleteMatch();
			DeleteDeck();
		}

		public void CreateDeck()
		{
			var result1 = HearthStatsAPI.PostDeckAsync(_deck1).Result;
			Debug.Assert(result1.Success, "PostDeck success");
			Debug.Assert(_deck1.HasHearthStatsId, "Deck has HearthStats ID");
			Debug.Assert(_deck1.HasHearthStatsDeckVersionId, "Deck has HearthStats version ID");

			var result2 = HearthStatsAPI.PostDeckAsync(_deck2).Result;
			Debug.Assert(result2.Success, "PostDeck success");
			Debug.Assert(_deck2.HasHearthStatsId, "Deck has HearthStats ID");
			Debug.Assert(_deck2.HasHearthStatsDeckVersionId, "Deck has HearthStats version ID");
		}

		public void CreateMatch()
		{
			_match1.HearthStatsDeckVersionId = _deck1.HearthStatsDeckVersionId;
			_match1.PlayerDeckVersion = _deck1.Version;
			var result = HearthStatsAPI.PostGameResultAsync(_match1, _deck1).Result;
			Debug.Assert(result.Success, "PostMatch success");
			Debug.Assert(_match1.HasHearthStatsId, "Match1 has HearthStats ID");
		}

		public void MultiCreateMatch()
		{
			_match2.HearthStatsDeckVersionId = _deck2.HearthStatsDeckVersionId;
			_match2.PlayerDeckVersion = _deck2.Version;
			_match3.HearthStatsDeckVersionId = _deck2.HearthStatsDeckVersionId;
			_match3.PlayerDeckVersion = _deck2.Version;
			var result = HearthStatsAPI.PostMultipleGameResultsAsync(new[] {_match2, _match3}, _deck2).Result;
			Debug.Assert(result.Success, "PostMatch success");
			Debug.Assert(_match2.HasHearthStatsId, "Match2 has HearthStats ID");
			Debug.Assert(_match3.HasHearthStatsId, "Match3 has HearthStats ID");
		}

		public void MoveMatch()
		{
			var result = HearthStatsAPI.MoveMatchAsync(_match1, _deck2).Result;
			Debug.Assert(result.Success, "PostMatch success");
		}

		public void DeleteMatch()
		{
			var result = HearthStatsAPI.DeleteMatchesAsync(new List<GameStats>(new[] {_match2})).Result;
			Debug.Assert(result.Success, "DeleteMatch1 success");
		}

		public void DeleteDeck()
		{
			var result = HearthStatsAPI.DeleteDeckAsync(new[] {_deck1, _deck2}).Result;
			Debug.Assert(result.Success, "Delete deck1 and deck2 success");
		}
	}
}