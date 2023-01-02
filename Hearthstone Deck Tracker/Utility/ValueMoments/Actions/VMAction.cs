using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public abstract class VMAction
	{
		public enum Source
		{
			[MixpanelProperty("app")]
			App,

			[MixpanelProperty("mainWindow")]
			MainWindow,

			[MixpanelProperty("overlay")]
			Overlay,
		}


		protected VMAction(string eventName, Source source, string actionType, int? maxDailyOccurrences, Dictionary<string, object> properties)
		{
			EventName = eventName;
			MaxDailyOccurrences = maxDailyOccurrences; // if this is null, this action is not sent to the event counter and will always be sent to Mixpanel
			Properties = new Dictionary<string, object>(properties){
				{ "action_type", actionType },
				{ "action_source", source },
			};
		}

		public string EventId { get => GetEventId(); }
		public string EventName { get; }
		public int? MaxDailyOccurrences { get; }
		public Dictionary<string, object> Properties { get; }

		public Dictionary<string, object> MixpanelProperties {
			get
			{

				var props = new Dictionary<string, object>(Properties);
				if(
					Properties.Keys.Contains("action_source") &&
					Helper.TryGetAttribute<MixpanelPropertyAttribute>(Properties["action_source"], out var sAttr) &&
					sAttr?.Name != null
				)
					props["action_source"] = sAttr.Name;

				if(Properties.TryGetValue("franchise", out var franchise))
					props["franchise"] = ((Franchise[])franchise).Select(x => GetMixpanelPropertyName(x));

				if(Properties.TryGetValue("action_name", out var action_name) && action_name is Enum)
					props["action_name"] = $"{GetMixpanelPropertyName(action_name)}";

				if(Properties.TryGetValue("toast", out var toastName) && toastName is Enum)
					props["toast"] = $"{GetMixpanelPropertyName(toastName)}";

				return props;
			}
		}
		public void AddProperties(Dictionary<string, object> newProperties)
		{
			foreach(var property in newProperties)
				Properties.Add(property.Key, property.Value);
		}
	}
}
