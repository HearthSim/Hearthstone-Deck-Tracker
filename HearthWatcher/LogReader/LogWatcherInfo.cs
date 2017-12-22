namespace HearthWatcher.LogReader
{
	public class LogWatcherInfo
	{
		public bool HasFilters => StartsWithFilters != null || ContainsFilters != null;

		public string Name { get; set; }
		public string[] StartsWithFilters { get; set; }
		public string[] ContainsFilters { get; set; }
		public bool Reset { get; set; } = true;
	}
}
