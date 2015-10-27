namespace Hearthstone_Deck_Tracker.LogReader
{
	public class LogReaderInfo
	{
		public string Name { get; set; }

		public bool HasFilters
		{
			get { return StartsWithFilters != null && ContainsFilters != null; }
		}
		public string[] StartsWithFilters { get; set; }
		public string[] ContainsFilters { get; set; }
		public string FilePath { get; set; }
	}
}