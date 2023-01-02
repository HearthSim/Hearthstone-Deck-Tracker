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

				if(Properties.Keys.Contains("franchise"))
					props["franchise"] = ((Franchise[])Properties["franchise"]).Select(x =>
					{
						Helper.TryGetAttribute<MixpanelPropertyAttribute>(x, out var attr);
						return attr?.Name;
					});

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
