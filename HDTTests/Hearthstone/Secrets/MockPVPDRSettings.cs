using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Secrets;
using System.Collections.Generic;

namespace HDTTests.Hearthstone.Secrets
{
	public class MockPVPDRSettings : PVPDRSettingsProvider
	{
		public List<CardSet> CurrentSets => new List<CardSet>
			{ CardSet.CORE, CardSet.EXPERT1, CardSet.NAXX, CardSet.SCHOLOMANCE, CardSet.KARA};
		public List<string> ExclusiveSecrets => new List<string> { "BCON_012" };
		public List<string> BannedSecrets => new List<string> { "BCON_012" };
	}
}
