using Hearthstone_Deck_Tracker.Controls.Overlay.Arena;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace HDTTests.Controls.Overlay.Arena
{
	[TestClass]
	public class ArenaPickApiTests
	{
		#region MessageTypeConverter.ReadJson

		[DataTestMethod]
		[DataRow("\"ENHANCED_BY\"", MessageType.EnhancedBy)]
		[DataRow("\"CARDS_ENABLED\"", MessageType.CardsEnabled)]
		[DataRow("\"OFFERED_COPY\"", MessageType.OfferedCopy)]
		[DataRow("\"LOW_SYNERGY\"", MessageType.LowSynergy)]
		[DataRow("\"HIGHLANDER\"", MessageType.Highlander)]
		[DataRow("\"SOFT_HIGHLANDER\"", MessageType.SoftHighlander)]
		[DataRow("\"HIGHLANDER_CHANCES\"", MessageType.HighlanderChances)]
		[DataRow("\"VERY_RARE\"", MessageType.VeryRare)]
		[DataRow("\"SCORE_BOOST\"", MessageType.ScoreBoost)]
		[DataRow("\"QUEST_HELPS\"", MessageType.QuestHelps)]
		[DataRow("\"QUEST_NUM_REQ\"", MessageType.QuestNumReq)]
		[DataRow("\"INVALID\"", MessageType.Invalid)]
		public void ReadJson_KnownStrings_MapToExpectedType(string json, MessageType expected)
		{
			Assert.AreEqual(expected, JsonConvert.DeserializeObject<MessageType>(json));
		}

		[TestMethod]
		public void ReadJson_IsCaseInsensitiveAndTrimsWhitespace()
		{
			Assert.AreEqual(MessageType.EnhancedBy, JsonConvert.DeserializeObject<MessageType>("\" enhanced_by \""));
			Assert.AreEqual(MessageType.VeryRare, JsonConvert.DeserializeObject<MessageType>("\"Very_Rare\""));
		}

		[TestMethod]
		public void ReadJson_UnknownString_ReturnsInvalid()
		{
			Assert.AreEqual(MessageType.Invalid, JsonConvert.DeserializeObject<MessageType>("\"SOMETHING_ELSE\""));
		}

		[DataTestMethod]
		[DataRow("\"\"")]
		[DataRow("\"   \"")]
		public void ReadJson_EmptyOrWhitespace_ReturnsInvalid(string json)
		{
			Assert.AreEqual(MessageType.Invalid, JsonConvert.DeserializeObject<MessageType>(json));
		}

		[DataTestMethod]
		[DataRow("123")]
		[DataRow("true")]
		[DataRow("null")]
		public void ReadJson_NonStringToken_ReturnsInvalid(string json)
		{
			Assert.AreEqual(MessageType.Invalid, JsonConvert.DeserializeObject<MessageType>(json));
		}

		#endregion

		#region MessageTypeConverter.WriteJson

		[DataTestMethod]
		[DataRow(MessageType.EnhancedBy, "ENHANCED_BY")]
		[DataRow(MessageType.CardsEnabled, "CARDS_ENABLED")]
		[DataRow(MessageType.OfferedCopy, "OFFERED_COPY")]
		[DataRow(MessageType.LowSynergy, "LOW_SYNERGY")]
		[DataRow(MessageType.Highlander, "HIGHLANDER")]
		[DataRow(MessageType.SoftHighlander, "SOFT_HIGHLANDER")]
		[DataRow(MessageType.HighlanderChances, "HIGHLANDER_CHANCES")]
		[DataRow(MessageType.VeryRare, "VERY_RARE")]
		[DataRow(MessageType.ScoreBoost, "SCORE_BOOST")]
		[DataRow(MessageType.QuestHelps, "QUEST_HELPS")]
		[DataRow(MessageType.QuestNumReq, "QUEST_NUM_REQ")]
		[DataRow(MessageType.Invalid, "INVALID")]
		public void WriteJson_SerializesToExpectedWireString(MessageType type, string expectedWire)
		{
			Assert.AreEqual("\"" + expectedWire + "\"", JsonConvert.SerializeObject(type));
		}

		[DataTestMethod]
		[DataRow(MessageType.EnhancedBy)]
		[DataRow(MessageType.LowSynergy)]
		[DataRow(MessageType.QuestNumReq)]
		[DataRow(MessageType.Invalid)]
		public void WriteThenRead_RoundTripsToSameValue(MessageType type)
		{
			var json = JsonConvert.SerializeObject(type);
			Assert.AreEqual(type, JsonConvert.DeserializeObject<MessageType>(json));
		}

		#endregion

		#region Message.ParseContent

		[TestMethod]
		public void ParseContent_WithPopulatedContent_DeserializesFields()
		{
			var msg = JsonConvert.DeserializeObject<ArenaCardPickApiResponse.Message>(
				"{\"type\":\"LOW_SYNERGY\",\"content\":{\"remaining_picks\":7}}");

			Assert.AreEqual(MessageType.LowSynergy, msg.Type);
			var content = msg.ParseContent<LowSynergyMessageContent>();
			Assert.AreEqual(7, content.RemainingPicks);
		}

		[TestMethod]
		public void ParseContent_WithNullContent_ReturnsDefaultInstance()
		{
			var msg = JsonConvert.DeserializeObject<ArenaCardPickApiResponse.Message>(
				"{\"type\":\"LOW_SYNERGY\",\"content\":null}");

			var content = msg.ParseContent<LowSynergyMessageContent>();
			Assert.IsNotNull(content);
			Assert.IsNull(content.RemainingPicks);
		}

		[TestMethod]
		public void ParseContent_WithEmptyContent_ReturnsDefaultInstance()
		{
			var msg = JsonConvert.DeserializeObject<ArenaCardPickApiResponse.Message>(
				"{\"type\":\"LOW_SYNERGY\",\"content\":{}}");

			var content = msg.ParseContent<LowSynergyMessageContent>();
			Assert.IsNotNull(content);
			Assert.IsNull(content.RemainingPicks);
		}

		[TestMethod]
		public void ParseContent_HighlanderContent_MapsCardId()
		{
			var msg = JsonConvert.DeserializeObject<ArenaCardPickApiResponse.Message>(
				"{\"type\":\"HIGHLANDER\",\"content\":{\"highlander_card_id\":\"AT_001\"}}");

			Assert.AreEqual(MessageType.Highlander, msg.Type);
			Assert.AreEqual("AT_001", msg.ParseContent<HighlanderMessageContent>().HighlanderCardId);
		}

		[TestMethod]
		public void Message_UnknownType_DeserializesAsInvalid()
		{
			var msg = JsonConvert.DeserializeObject<ArenaCardPickApiResponse.Message>(
				"{\"type\":\"WHO_KNOWS\",\"content\":{}}");

			Assert.AreEqual(MessageType.Invalid, msg.Type);
		}

		#endregion
	}
}
