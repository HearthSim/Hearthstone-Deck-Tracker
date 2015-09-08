namespace Hearthstone_Deck_Tracker.LogReader
{
	public class LogReaderNamespaces
	{
		public static LogReaderInfo PowerLogReaderInfo
		{
			get { return new LogReaderInfo { Name = "Power", StartsWithFilters = new[] { "GameState." }, ContainsFilters = new[] { "Begin Spectating", "Start Spectator", "End Spectator" } }; }
		}
		public static LogReaderInfo AssetLogReaderInfo
		{
			get { return new LogReaderInfo { Name = "Asset" }; }
		}
		public static LogReaderInfo BobLogReaderInfo
		{
			get { return new LogReaderInfo { Name = "Bob" }; }
		}
		public static LogReaderInfo RachelleLogReaderInfo
		{
			get { return new LogReaderInfo { Name = "Rachelle" }; }
		}
		public static LogReaderInfo ZoneLogReaderInfo
		{
			get { return new LogReaderInfo {Name = "Zone", ContainsFilters = new[] {"zone from"}}; }
		}
		public static LogReaderInfo ArenaLogReaderInfo
		{
			get { return new LogReaderInfo { Name = "Arena" }; }
		}
	}
}