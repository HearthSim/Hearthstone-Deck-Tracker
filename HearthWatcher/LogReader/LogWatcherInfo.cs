using System.Linq;

namespace HearthWatcher.LogReader
{
	public class LogWatcherInfo
	{
		public LogWatcherInfo(string name)
		{
			Name = name;
		}

		public bool HasFilters => StartsWithFilters != null || ContainsFilters != null;

		public bool Matches(string lineContent) => !HasFilters
			|| (StartsWithFilters?.Any(lineContent.StartsWith) ?? false)
			|| (ContainsFilters?.Any(lineContent.Contains) ?? false);

		public string Name { get; }
		public string[]? StartsWithFilters { get; set; }
		public string[]? ContainsFilters { get; set; }
		public bool Reset { get; set; } = true;
	}
}
