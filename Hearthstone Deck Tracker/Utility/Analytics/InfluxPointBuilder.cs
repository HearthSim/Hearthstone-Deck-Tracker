using System;
using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker.Utility.Analytics
{
	internal class InfluxPointBuilder
	{
		private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();
		private readonly Dictionary<string, object> _tags = new Dictionary<string, object>();
		private readonly string _name;
		private DateTime? _timestamp;

		public InfluxPointBuilder(string name, bool defaultField = true)
		{
			_name = name;
			if(defaultField)
				_fields.Add("count", 1);
		}

		public InfluxPointBuilder Tag(string name, object value)
		{
			_tags.Add(name, value);
			return this;
		}

		public InfluxPointBuilder Field(string name, object value)
		{
			_fields.Add(name, value);
			return this;
		}

		public InfluxPointBuilder Timestamp(DateTime timestamp)
		{
			_timestamp = timestamp;
			return this;
		}

		public InfluxPoint Build()
		{
			if(!_fields.Any())
				throw new Exception("Missing field");
			return new InfluxPoint(_name, _tags, _fields, _timestamp ?? DateTime.UtcNow);
		}
	}
}