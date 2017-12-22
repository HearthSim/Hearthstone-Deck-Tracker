using System;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.Secrets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Hearthstone.Secrets
{
	[TestClass]
	public class SecretTest
	{
		[TestMethod]
		[ExpectedException(typeof(ArgumentException),
			"Able to instantiate secret from non-secret entity")]
		public void InvalidEntity_NoSecret()
		{
			var entity = new Entity(0);
			var secret = new Secret(entity);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentException),
			"Able to instantiate secret from non-secret entity")]
		public void InvalidEntity_NoClass()
		{
			var entity = new Entity(0);
			entity.SetTag(GameTag.SECRET, 1);
			var secret = new Secret(entity);
		}

		[TestMethod]
		public void ValidEntity()
		{
			var entity = new Entity(0);
			entity.SetTag(GameTag.SECRET, 1);
			entity.SetTag(GameTag.CLASS, (int)CardClass.PALADIN);
			var secret = new Secret(entity);
			Assert.AreEqual(secret.Entity, entity);

			Assert.AreEqual(CardIds.Secrets.Paladin.All.Count, secret.Excluded.Count);

			foreach(var id in CardIds.Secrets.Hunter.All)
				Assert.IsFalse(secret.Excluded.ContainsKey(id));
			foreach(var id in CardIds.Secrets.Mage.All)
				Assert.IsFalse(secret.Excluded.ContainsKey(id));

			foreach(var id in CardIds.Secrets.Paladin.All)
			{
				Assert.IsTrue(secret.Excluded.ContainsKey(id));
				Assert.IsFalse(secret.Excluded[id]);
			}

			foreach(var id in CardIds.Secrets.Paladin.All)
				secret.Exclude(id);

			foreach(var id in CardIds.Secrets.Paladin.All)
				Assert.IsTrue(secret.Excluded[id]);

			secret.Include(CardIds.Secrets.Paladin.Avenge);
			Assert.IsFalse(secret.IsExcluded(CardIds.Secrets.Paladin.Avenge));
		}
	}
}
