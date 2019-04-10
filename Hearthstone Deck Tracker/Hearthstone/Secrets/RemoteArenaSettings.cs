using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.Secrets
{
	public class RemoteArenaSettings : ArenaSettingsProvider
	{
		public List<CardSet> CurrentSets => RemoteConfig.Instance.Data?.Arena.CurrentSets ?? new List<CardSet>();
		public List<string> ExclusiveSecrets => RemoteConfig.Instance.Data?.Arena.ExclusiveSecrets?? new List<string>();
		public List<string> BannedSecrets => RemoteConfig.Instance.Data?.Arena.BannedSecrets ?? new List<string>();
	}
}
