#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hearthstone_Deck_Tracker.Hearthstone;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace HDTTests.Hearthstone
{
	[TestClass]
	public class CardIdConstantsTest
	{
		[TestMethod]
		public void Druid()
		{
			VerifyCardIds(typeof(CardIds.Druid));
		}

		[TestMethod]
		public void Mage()
		{
			VerifyCardIds(typeof(CardIds.Mage));
		}

		[TestMethod]
		public void Neutral()
		{
			VerifyCardIds(typeof(CardIds.Neutral));
		}

		[TestMethod]
		public void Priest()
		{
			VerifyCardIds(typeof(CardIds.Priest));
		}

		[TestMethod]
		public void Rogue()
		{
			VerifyCardIds(typeof(CardIds.Rogue));
		}

		[TestMethod]
		public void Warlock()
		{
			VerifyCardIds(typeof(CardIds.Warlock));
		}

		[TestMethod]
		public void Warrior()
		{
			VerifyCardIds(typeof(CardIds.Warrior));
		}

		private void VerifyCardIds(Type type)
		{
			foreach(var field in GetConstants(type))
			{
				var card = Database.GetCardFromId(field.GetValue(field).ToString());
				Assert.IsNotNull(card);
				Assert.AreEqual(field.Name.ToLower(), card.Name.Replace(" ", "").Replace("!", "").ToLower());
			}
		}

		private List<FieldInfo> GetConstants(Type type)
		{
			FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

			return fieldInfos.Where(fi => fi.IsLiteral && !fi.IsInitOnly).ToList();
		}
	}
}