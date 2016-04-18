using Hearthstone_Deck_Tracker.HsReplay.Enums;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class Account
	{
		public static AccountStatus Status { get; set; }
		public static string BattleTag { get; set; }
		public static bool ReplaysArePublic { get; set; }
	}
}
