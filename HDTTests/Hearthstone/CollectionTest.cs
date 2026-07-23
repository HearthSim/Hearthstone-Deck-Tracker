using System.Linq;
using System.Text.RegularExpressions;
using HearthMirror.Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HDTCollection = Hearthstone_Deck_Tracker.Hearthstone.Collection;

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class CollectionTest
	{
		// Builds a collection with a single card owned in two premium variants
		// (2 normal + 1 golden) so the premium-count layout can be asserted.
		private static HDTCollection BuildCollection()
		{
			var mirror = new Collection();
			mirror.Cards.Add(new Card("HDT_TEST_CARD", 2, 0, 0));
			mirror.Cards.Add(new Card("HDT_TEST_CARD", 1, 1, 0));
			var battleTag = new BattleTag { Name = "Tester", Number = "1234" };
			return new HDTCollection(1, 2, battleTag, mirror);
		}

		[TestMethod]
		public void Serialize_ProducesHSReplayTopLevelKeys()
		{
			var json = JObject.Parse(JsonConvert.SerializeObject(BuildCollection()));
			CollectionAssert.AreEquivalent(
				new[] { "collection", "favorite_heroes", "cardbacks", "favorite_cardback", "dust", "player_records" },
				json.Properties().Select(p => p.Name).ToArray());
		}

		[TestMethod]
		public void Serialize_OmitsAccountIdentifiers()
		{
			var json = JsonConvert.SerializeObject(BuildCollection());
			StringAssert.DoesNotMatch(json, new Regex("AccountHi|AccountLo|BattleTag"));
		}

		[TestMethod]
		public void Serialize_GroupsCardCountsByPremiumType()
		{
			var json = JObject.Parse(JsonConvert.SerializeObject(BuildCollection()));
			var cards = (JObject)json["collection"];
			Assert.AreEqual(1, cards.Count);
			var counts = cards.Properties().First().Value.ToObject<int[]>();
			// [normal, golden, diamond, signature, trial x4]
			CollectionAssert.AreEqual(new[] { 2, 1, 0, 0, 0, 0, 0, 0 }, counts);
		}

		[TestMethod]
		public void Size_SumsAllCardCounts()
		{
			Assert.AreEqual(3, BuildCollection().Size());
		}
	}
}
