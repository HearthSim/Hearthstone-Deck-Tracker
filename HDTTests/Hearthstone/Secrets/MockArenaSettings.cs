using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Secrets;
using System.Collections.Generic;

namespace HDTTests.Hearthstone.Secrets
{
	public class MockArenaSettings : ArenaSettingsProvider
	{
		public List<CardSet> CurrentSets => new List<CardSet>
			{ CardSet.CORE, CardSet.EXPERT1, CardSet.NAXX, CardSet.OG, CardSet.GANGS, CardSet.GILNEAS, CardSet.DALARAN };
		public List<string> ExclusiveSecrets => new List<string> { "BCON_012" };
		public List<string> BannedSecrets => new List<string> { "BCON_012" };
	}
}
