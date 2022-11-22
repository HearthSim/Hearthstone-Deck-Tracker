using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public abstract class VMAction
	{
		public class Source
		{
			public const string App = "app";
			public const string MainWindow = "mainWindow";
			public const string Overlay = "overlay";
		}

		protected VMAction(string eventName, string source, string actionType, int? maxDailyOccurrences, Dictionary<string, object> properties)
		{
			EventName = eventName;
			MaxDailyOccurrences = maxDailyOccurrences; // if this is null, this action is not sent to the event counter and will always be sent to Mixpanel
			Properties = new Dictionary<string, object>(properties){
				{ "action_type", actionType },
				{ "action_source", source },
			};
		}

		public string EventName { get; }
		public int? MaxDailyOccurrences { get; }
		public Dictionary<string, object> Properties { get; }

		public void AddProperties(Dictionary<string, object> newProperties)
		{
			foreach(var property in newProperties)
				Properties.Add(property.Key, property.Value);
		}
	}
}
