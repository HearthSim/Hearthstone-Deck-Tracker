namespace Hearthstone_Deck_Tracker.API
{
	public class LogEvents
	{
		public static readonly ActionList<string> OnArenaLogLine = new ActionList<string>();
		public static readonly ActionList<string> OnAssetLogLine = new ActionList<string>();
		public static readonly ActionList<string> OnBobLogLine = new ActionList<string>();
		public static readonly ActionList<string> OnPowerLogLine = new ActionList<string>();
		public static readonly ActionList<string> OnRachelleLogLine = new ActionList<string>();
	}
}
