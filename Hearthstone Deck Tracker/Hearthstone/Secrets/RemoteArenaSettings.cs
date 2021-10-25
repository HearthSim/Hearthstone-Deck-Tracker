using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.RemoteData;

namespace Hearthstone_Deck_Tracker.Hearthstone.Secrets
{
	public class RemoteArenaSettings : ArenaSettingsProvider
	{
		public List<CardSet> CurrentSets => Remote.Config.Data?.Arena.CurrentSets ?? new List<CardSet>();
		public List<string> ExclusiveSecrets => Remote.Config.Data?.Arena.ExclusiveSecrets?? new List<string>();
		public List<string> BannedSecrets => Remote.Config.Data?.Arena.BannedSecrets ?? new List<string>();
	}
}
