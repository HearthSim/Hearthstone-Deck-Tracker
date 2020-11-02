using System.Collections.Generic;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Hearthstone.Secrets
{
	public interface PVPDRSettingsProvider
	{
		List<CardSet> CurrentSets { get; }
		List<string> ExclusiveSecrets { get;  }
		List<string> BannedSecrets { get; }
	}
}
