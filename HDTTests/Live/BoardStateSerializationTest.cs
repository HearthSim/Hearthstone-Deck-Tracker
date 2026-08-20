using Hearthstone_Deck_Tracker.Live.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HDTTests.Live
{
	[TestClass]
	public class BoardStateSerializationTest
	{
		[TestMethod]
		public void BoardWithoutEnchantments_SerializesAsFlatDbfIdArray()
		{
			var player = new BoardStatePlayer { Board = new CardWithEnchantments[] { 123, 456 } };

			Assert.AreEqual("[123,456]", SerializeBoard(player));
		}

		[TestMethod]
		public void BoardWithEnchantments_SerializesTheSlotAsAnArray()
		{
			var player = new BoardStatePlayer
			{
				Board = new[]
				{
					new CardWithEnchantments(123),
					new CardWithEnchantments(456, 789, 1011),
				}
			};

			Assert.AreEqual("[123,[456,789,1011]]", SerializeBoard(player));
		}

		[TestMethod]
		public void BoardWithCardIds_SerializesCardsAsStrings()
		{
			var player = new BoardStatePlayer
			{
				Board = new[] { new CardWithEnchantments("EX1_001", "EX1_001e"), new CardWithEnchantments(456) }
			};

			Assert.AreEqual("[[\"EX1_001\",\"EX1_001e\"],456]", SerializeBoard(player));
		}

		[TestMethod]
		public void EmptyBoard_SerializesAsEmptyArray()
		{
			var player = new BoardStatePlayer { Board = new CardWithEnchantments[0] };

			Assert.AreEqual("[]", SerializeBoard(player));
		}

		[TestMethod]
		public void MixedBoard_RoundTrips()
		{
			var player = new BoardStatePlayer
			{
				Board = new[]
				{
					new CardWithEnchantments(123),
					new CardWithEnchantments(456, 789),
					new CardWithEnchantments("EX1_001", 42),
				}
			};

			var roundTripped = JsonConvert.DeserializeObject<BoardStatePlayer>(JsonConvert.SerializeObject(player));

			CollectionAssert.AreEqual(player.Board, roundTripped.Board);
		}

		[TestMethod]
		public void BoardsDifferingOnlyInEnchantments_AreNotEqual()
		{
			var player = new BoardStatePlayer { Board = new[] { new CardWithEnchantments(123) } };
			var other = new BoardStatePlayer { Board = new[] { new CardWithEnchantments(123, 456) } };

			Assert.IsFalse(player.Equals(other));
		}

		[TestMethod]
		public void DbfIdAndCardIdCards_AreNotEqual()
		{
			Assert.AreNotEqual(new CardWithEnchantments(123), new CardWithEnchantments("EX1_001"));
		}

		private static string SerializeBoard(BoardStatePlayer player)
			=> JObject.Parse(JsonConvert.SerializeObject(player))["board"].ToString(Formatting.None);
	}
}
