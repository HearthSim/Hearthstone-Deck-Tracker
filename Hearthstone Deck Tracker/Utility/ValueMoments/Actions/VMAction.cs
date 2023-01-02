using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

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
		
		/**
		 * if maxDailyOccurrences is null, this action is not sent to the event counter and will always be sent to Mixpanel
		 */
		protected VMAction(string eventName, Source source, string actionType, int? maxDailyOccurrences, Dictionary<string, object> properties)
		{
			EventName = eventName;
			Properties = new Dictionary<string, object>(properties){
				{ "action_type", actionType },
				{ "action_source", source },
			};
			EnrichedProperties = new ValueMomentEnrichedProperties(GetEventId(), maxDailyOccurrences);
		}

		public string EventId { get => GetEventId(); }
		public string EventName { get; }
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

				if(EnrichedProperties.CurrentDailyOccurrences != null)
					props.Add("cur_daily_occurrences", EnrichedProperties.CurrentDailyOccurrences);
				if(EnrichedProperties.MaximumDailyOccurrences != null)
					props.Add("max_daily_occurrences", EnrichedProperties.MaximumDailyOccurrences);
				if(EnrichedProperties.PreviousDailyOccurrences != null)
					props.Add("prev_daily_occurrences", EnrichedProperties.PreviousDailyOccurrences);

				foreach(var property in EnrichedProperties.ClientSettings)
					if (Helper.TryGetAttribute<MixpanelPropertyAttribute>(property.Key, out var cAttr) && cAttr?.Name != null)
						props.Add(cAttr.Name, property.Value);

				props.Add(
					"hdt_general_settings_enabled",
					EnrichedProperties.HDTGeneralSettingsEnabled.Select(x => GetMixpanelPropertyName(x))
				);
				props.Add(
					"hdt_general_settings_disabled",
					EnrichedProperties.HDTGeneralSettingsDisabled.Select(x => GetMixpanelPropertyName(x))
				);

				return props;
			}
		}

		public ValueMomentEnrichedProperties EnrichedProperties { get; private set; }

		public void AddProperties(Dictionary<string, object> newProperties)
		{
			foreach(var property in newProperties)
				Properties.Add(property.Key, property.Value);
		}

		private string GetEventId()
		{
			Properties.TryGetValue("franchise", out var franchise);
			Properties.TryGetValue("sub_franchise", out var subFranchise);
			var id = EventName;

			if(franchise != null)
			{
				var firstFranchise = ((Franchise[])franchise)[0];
				id += $"_{GetMixpanelPropertyName(firstFranchise)?.ToLower()}";
			}	

			if(subFranchise != null)
			{
				string[] subFranchiseArray = (string[])subFranchise;
				if(subFranchiseArray.Length > 0)
					id += $"_{((string[])subFranchise)[0].ToLower()}";
			}

			return id;
		}

		private static string? GetMixpanelPropertyName(object obj)
		{
			Helper.TryGetAttribute<MixpanelPropertyAttribute>(obj, out var attr);
			return attr?.Name;
		}
	}
}
