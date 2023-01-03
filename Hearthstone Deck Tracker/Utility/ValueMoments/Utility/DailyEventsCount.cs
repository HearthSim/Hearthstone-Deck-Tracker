using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility
{
	[XmlRoot("DailyEventsCount")]
	public sealed class DailyEventsCount
	{
		private static readonly Lazy<DailyEventsCount> LazyInstance = new Lazy<DailyEventsCount>(Load);

		public static DailyEventsCount Instance = LazyInstance.Value;

		[XmlElement("Event")]
		public List<EventItem> Events { get; set; } = new List<EventItem>();

		private static string DataPath => Path.Combine(Config.AppDataPath, "DailyEventsCount.xml");

		private static DailyEventsCount Load()
		{
			if(!File.Exists(DataPath))
				return new DailyEventsCount();
			try
			{
				return XmlManager<DailyEventsCount>.Load(DataPath);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
			return new DailyEventsCount();
		}

		public int GetEventDailyCount(string eventId)
		{
			var existing = Events.FirstOrDefault(x => x.Id == eventId);
			return existing?.Count ?? 0;
		}

		public int UpdateEventDailyCount(string eventId)
		{
			var existing = Events.FirstOrDefault(x => x.Id == eventId);
			if(existing == null)
			{
				existing = new EventItem(eventId, DateTime.Now.ToString("o"), 1);
				Events.Add(existing);
			}
			else
			{
				var lastTimestamp = DateTime.Parse(existing.Timestamp);
				if(DateTime.Now - lastTimestamp < TimeSpan.FromDays(1))
					existing.Count += 1;
				else
				{
					existing.Timestamp = DateTime.Now.ToString("o");
					existing.Count = 1;
				}
			}
			Save();
			return existing.Count;
		}

		public void SetEventDailyCount(string eventId, int count)
		{
			var existing = Events.FirstOrDefault(x => x.Id == eventId);
			if(existing == null)
			{
				existing = new EventItem(eventId, DateTime.Now.ToString("o"), 0);
				Events.Add(existing);
			}

			existing.Count = count;
			Save();
		}

		public void Clear(string eventId)
		{
			var existing = Events.FirstOrDefault(x => x.Id == eventId);
			if(existing != null)
			{
				existing.Count = 0;
				Save();
			}
		}

		public static void Save()
		{
			try
			{
				XmlManager<DailyEventsCount>.Save(DataPath, Instance);
			}
			catch(Exception ex)
			{
				Log.Error(ex);
			}
		}

		public class EventItem
		{
			public EventItem(string id, string timestamp, int count)
			{
				Id = id;
				Timestamp = timestamp;
				Count = count;
			}

			public EventItem()
			{
			}

			[XmlAttribute("id")]
			public string? Id { get; set; }

			[XmlAttribute("timestamp")]
			public string? Timestamp { get; set; }

			[XmlAttribute("count")]
			public int Count { get; set; }
		}
	}
}
