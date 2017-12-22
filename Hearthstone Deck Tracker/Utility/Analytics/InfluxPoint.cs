using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Utility.Analytics
{
	internal class InfluxPoint
	{
		private readonly Regex _escape = new Regex("[,= ]");

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

		private string Escape(string str)
		{
			return _escape.Replace(str, "\\$&");
		}

		public string ToLineProtocol()
		{
			var tags = string.Join(",", Tags.Select(x => $"{x.Key}={Escape(x.Value.ToString())}"));
			var fields = string.Join(",", Fields.Select(x => $"{x.Key}={GetValueString(x.Value)}"));
			return $"{Name}{(tags.Any() ? $",{tags}" : "")} {fields} {Timestamp.ToUnixTime()}";
		}

		public string GetValueString(object value)
		{
			if(int.TryParse(value.ToString(), out var intValue))
				return value.ToString();
			return $"\"{Escape(value.ToString())}\"";
		}
	}
}
