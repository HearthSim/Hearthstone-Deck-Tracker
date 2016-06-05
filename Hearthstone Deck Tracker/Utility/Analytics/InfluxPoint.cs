using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Utility.Analytics
{
	internal class InfluxPoint
	{
		public InfluxPoint(string name, Dictionary<string, object> tags, Dictionary<string, object> fields, DateTime utcNow)
		{
			Name = name;
			Tags = tags ?? new Dictionary<string, object>();
			Fields = fields ?? new Dictionary<string, object>();
			Timestamp = utcNow;
		}

		public string Name { get; }
		public Dictionary<string, object> Tags { get; }
		public Dictionary<string, object> Fields { get; }
		public DateTime Timestamp { get; }

		public string ToLineProtocol()
		{
			var tags = string.Join(",", Tags.Select(x => $"{x.Key}={x.Value}"));
			var fields = string.Join(",", Fields.Select(x => $"{x.Key}={x.Value}"));
			return $"{Name}{(tags.Any() ? $",{tags}" : "")} {fields} {Timestamp.ToUnixTime()}";
		}
	}
}