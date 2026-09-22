using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.Utility.RemoteData
{
	internal static class Remote
	{
		public static DataLoader<RemoteData.Config?> Config { get; }
			= DataLoader<RemoteData.Config>.JsonFromWeb("https://hsdecktracker.net/config.json");

		public static DataLoader<List<RemoteData.Mercenary>?> Mercenaries { get; }
			= DataLoader<List<RemoteData.Mercenary>>.JsonFromWeb("https://api.hearthstonejson.com/v1/latest/enUS/mercenaries.json");

		public static DataLoader<RemoteData.LiveSecrets?> LiveSecrets { get; }
			= DataLoader<RemoteData.LiveSecrets>.JsonFromWeb("https://hsreplay.net/api/v1/live/secrets/");

		public static DataLoader<RemoteData.MetaPeriod?> BattlegroundsLiveMetaPeriod { get; }
			= DataLoader<RemoteData.MetaPeriod>.JsonFromWeb(() =>
			{
				const string url = "https://hsreplay.net/api/v1/battlegrounds/meta_periods/live/";
				var region = Core.Game.CurrentRegion;
				return region == Region.UNKNOWN ? url : $"{url}?region={(BnetRegion)region}";
			});
	}
}
