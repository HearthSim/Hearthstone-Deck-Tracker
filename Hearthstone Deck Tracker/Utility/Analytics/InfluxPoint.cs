using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Utility.Analytics
{
	internal class InfluxPoint
	{
		private readonly Regex _escape = new Regex("[,= ]");

		public InfluxPoint(string name, Dictionary<string, object> tags, Dictionary<string, object> fields, DateTime utcNow, bool highPrecision = false)
		{
			Name = name;
			Tags = tags ?? new Dictionary<string, object>();
			Fields = fields ?? new Dictionary<string, object>();
			Timestamp = utcNow;
			HighPrecision = highPrecision;
		}

		public string Name { get; }
		public Dictionary<string, object> Tags { get; }
		public Dictionary<string, object> Fields { get; }
		public DateTime Timestamp { get; }
		public bool HighPrecision { get; }

		private string Escape(string str)
		{
			return _escape.Replace(str, "\\$&");
		}

		public string ToLineProtocol()
		{
			var tags = string.Join(",", Tags.Select(x => $"{x.Key}={Escape(x.Value.ToString())}"));
			var fields = string.Join(",", Fields.Select(x => $"{x.Key}={GetValueString(x.Value)}"));
			var timestamp = HighPrecision ? Timestamp.ToUnixTimeMicroSeconds() : Timestamp.ToUnixTimeSeconds();
			return $"{Name}{(tags.Any() ? $",{tags}" : "")} {fields} {timestamp}";
		}

		public string GetValueString(object value)
		{
			var valStr = value.ToString();
			if(int.TryParse(valStr, out var intValue))
				return valStr;
			if(double.TryParse(valStr, out var floatValue))
				return floatValue.ToString(CultureInfo.GetCultureInfo("en-US"));
			return $"\"{Escape(value.ToString())}\"";
		}
	}
}
