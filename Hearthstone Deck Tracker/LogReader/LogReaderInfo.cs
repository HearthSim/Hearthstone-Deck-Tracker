namespace Hearthstone_Deck_Tracker.LogReader
{
	public class LogReaderInfo
	{

		public bool HasFilters => StartsWithFilters != null || ContainsFilters != null;

		public string Name { get; set; }
		public string[] StartsWithFilters { get; set; }
		public string[] ContainsFilters { get; set; }
		public string FilePath { get; set; }
		public bool Reset { get; set; } = true;
	}
}