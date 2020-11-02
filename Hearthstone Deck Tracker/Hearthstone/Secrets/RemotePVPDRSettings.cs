using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Hearthstone.Secrets
{
	public class RemotePVPDRSettings : PVPDRSettingsProvider
	{
		public List<CardSet> CurrentSets => RemoteConfig.Instance.Data?.PVPDR.CurrentSets ?? new List<CardSet>();
		public List<string> ExclusiveSecrets => RemoteConfig.Instance.Data?.PVPDR.ExclusiveSecrets?? new List<string>();
		public List<string> BannedSecrets => RemoteConfig.Instance.Data?.PVPDR.BannedSecrets ?? new List<string>();
	}
}
