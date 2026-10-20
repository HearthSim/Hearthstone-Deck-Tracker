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

		[TestMethod]
		public void AssembledMinion_SerializesAsAnObject()
		{
			var player = new BoardStatePlayer
			{
				Board = new[]
				{
					new CardWithEnchantments(123).WithHalves(456, 789),
					new CardWithEnchantments(123, 42).WithHalves(null, 789),
					new CardWithEnchantments(123).WithHalves(null, null),
					new CardWithEnchantments(123).WithHalves(456, null),
				}
			};

			Assert.AreEqual(
				"[{\"card\":123,\"assembled_first_half\":456,\"assembled_second_half\":789}," +
				"{\"card\":123,\"enchantments\":[42],\"assembled_second_half\":789},123," +
				"{\"card\":123,\"assembled_first_half\":456}]",
				SerializeBoard(player)
			);
		}

		[TestMethod]
		public void AssembledMinionWithCardIds_SerializesHalvesAsStrings()
		{
			var player = new BoardStatePlayer
			{
				Board = new[] { new CardWithEnchantments("BE_572").WithHalves("BE_270", 130441) }
			};

			Assert.AreEqual(
				"[{\"card\":\"BE_572\",\"assembled_first_half\":\"BE_270\",\"assembled_second_half\":130441}]",
				SerializeBoard(player)
			);
		}

		[TestMethod]
		public void AssembledMinion_RoundTrips()
		{
			var player = new BoardStatePlayer
			{
				Board = new[]
				{
					new CardWithEnchantments("EX1_001", 42).WithHalves(456, null),
					new CardWithEnchantments(123).WithHalves(456, 789),
					new CardWithEnchantments(123).WithHalves("BE_270", "BE_150"),
				}
			};

			var roundTripped = JsonConvert.DeserializeObject<BoardStatePlayer>(JsonConvert.SerializeObject(player));

			CollectionAssert.AreEqual(player.Board, roundTripped.Board);
		}

		[TestMethod]
		public void BoardsDifferingOnlyInHalves_AreNotEqual()
		{
			var player = new BoardStatePlayer { Board = new[] { new CardWithEnchantments(123).WithHalves(456, 789) } };
			var other = new BoardStatePlayer { Board = new[] { new CardWithEnchantments(123).WithHalves(456, 790) } };

			Assert.IsFalse(player.Equals(other));
			Assert.AreNotEqual(new CardWithEnchantments(123).WithHalves(456, null), new CardWithEnchantments(123).WithHalves(null, 456));
			Assert.AreEqual(new CardWithEnchantments(123), new CardWithEnchantments(123).WithHalves(null, null));
		}

		[TestMethod]
		public void HeroPower_SerializesAsABareCard()
		{
			var player = new BoardStatePlayer { HeroPower = (CardRef)65542 };

			Assert.AreEqual("65542", SerializeProperty(player, "hero_power"));
		}

		[TestMethod]
		public void AssembledHeroPower_SerializesAsAnObject()
		{
			var player = new BoardStatePlayer { HeroPower = new CardWithEnchantments(132368).WithHalves(117027, 121051) };

			Assert.AreEqual(
				"{\"card\":132368,\"assembled_first_half\":117027,\"assembled_second_half\":121051}",
				SerializeProperty(player, "hero_power")
			);
		}

		[TestMethod]
		public void MissingHeroPower_IsLeftOut()
		{
			var player = new BoardStatePlayer();

			Assert.IsNull(JObject.Parse(JsonConvert.SerializeObject(player))["hero_power"]);
		}

		[TestMethod]
		public void HeroPowersDifferingOnlyInHalves_AreNotEqual()
		{
			var player = new BoardStatePlayer { HeroPower = new CardWithEnchantments(132368).WithHalves(117027, 121051) };
			var other = new BoardStatePlayer { HeroPower = new CardWithEnchantments(132368) };

			Assert.IsFalse(player.Equals(other));
			Assert.IsTrue(player.Equals(new BoardStatePlayer { HeroPower = new CardWithEnchantments(132368).WithHalves(117027, 121051) }));
		}

		private static string SerializeProperty(BoardStatePlayer player, string property)
			=> JObject.Parse(JsonConvert.SerializeObject(player))[property].ToString(Formatting.None);

		private static string SerializeBoard(BoardStatePlayer player)
			=> JObject.Parse(JsonConvert.SerializeObject(player))["board"].ToString(Formatting.None);
	}
}
