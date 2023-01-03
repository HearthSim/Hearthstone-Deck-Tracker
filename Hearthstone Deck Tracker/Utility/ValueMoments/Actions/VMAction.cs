using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions
{
	public abstract class VMAction
	{

		/**
		 * if maxDailyOccurrences is null, this action is not sent to the event counter and will always be sent to Mixpanel
		 */
		protected VMAction(string actionName, ActionSource source, string actionType, Franchise franchise, int? maxDailyOccurrences, Dictionary<string, object> properties, bool withPersonalStatsSettings = false)
		{
			ActionName = actionName;
			Franchise = franchise;
			Properties = new Dictionary<string, object>(properties){
				{ "action_type", actionType },
				{ "action_source", source },
			};
			ClientProperties = new ClientProperties(withPersonalStatsSettings);

			if(maxDailyOccurrences != null)
			{
				var curEventDailyCount = DailyEventsCount.Instance.GetEventDailyCount(GetEventId());
				var newCurrentDailyCount = DailyEventsCount.Instance.UpdateEventDailyCount(GetEventId());
				var eventCounterWasReset = curEventDailyCount > 0 && newCurrentDailyCount == 1;

				CurrentDailyOccurrences = newCurrentDailyCount;
				MaximumDailyOccurrences = maxDailyOccurrences;
				if(eventCounterWasReset)
					PreviousDailyOccurrences = curEventDailyCount;
			}
		}

		public string ActionId { get => GetEventId(); }
		public string ActionName { get; }
		public Franchise Franchise { get; }
		public int? CurrentDailyOccurrences { get; }
		public int? MaximumDailyOccurrences { get; }
		public int? PreviousDailyOccurrences { get; }

		public Dictionary<string, object> Properties { get; }

		public ClientProperties ClientProperties { get; }
		public FranchiseProperties? FranchiseProperties { get; protected set; }

		public Dictionary<string, object> MixpanelPayload
		{
			get
			{
				var props = new Dictionary<string, object>(Properties)
				{
					{ "domain", "hsreplay.net" }
				};

				if(
					Properties.Keys.Contains("action_source") &&
					Helper.TryGetAttribute<MixpanelPropertyAttribute>(Properties["action_source"], out var sAttr) &&
					sAttr?.Name != null
				)
					props["action_source"] = sAttr.Name;

				var franchises = Franchise == Franchise.All
					? new[] { Franchise.HSConstructed, Franchise.Battlegrounds, Franchise.Mercenaries }
					: new[] { Franchise };
				props["franchise"] = franchises.Select(x => GetMixpanelPropertyName(x)).ToArray();

				var vmProperties = ValueMomentManager.GetValueMomentsProperties(GetValueMoments());
				foreach(var property in vmProperties)
					props[property.Key] = property.Value;

				if(Properties.TryGetValue("action_name", out var actionName) && actionName is Enum)
					props["action_name"] = $"{GetMixpanelPropertyName(actionName)}";

				if(Properties.TryGetValue("toast", out var toastName) && toastName is Enum)
					props["toast"] = $"{GetMixpanelPropertyName(toastName)}";

				if(CurrentDailyOccurrences != null)
					props.Add("cur_daily_occurrences", CurrentDailyOccurrences);
				if(MaximumDailyOccurrences != null)
					props.Add("max_daily_occurrences", MaximumDailyOccurrences);
				if(PreviousDailyOccurrences != null)
					props.Add("prev_daily_occurrences", PreviousDailyOccurrences);

				foreach(var property in ClientProperties.ClientSettings)
					if(Helper.TryGetAttribute<MixpanelPropertyAttribute>(property.Key, out var cAttr) && cAttr?.Name != null)
						props.Add(cAttr.Name, property.Value);

				props.Add(
					"hdt_general_settings_enabled",
					ClientProperties.HDTGeneralSettingsEnabled.Select(x => GetMixpanelPropertyName(x)).ToArray()
				);
				props.Add(
					"hdt_general_settings_disabled",
					ClientProperties.HDTGeneralSettingsDisabled.Select(x => GetMixpanelPropertyName(x)).ToArray()
				);

				if(ClientProperties.HasPersonalStatsSettings)
				{
					props.Add(
						"hdt_personal_stats_settings_enabled",
						ClientProperties.PersonalStatsSettingsEnabled!.Select(x => GetMixpanelPropertyName(x)).ToArray()
					);
					props.Add(
						"hdt_personal_stats_settings_disabled",
						ClientProperties.PersonalStatsSettingsDisabled!.Select(x => GetMixpanelPropertyName(x)).ToArray()
					);
				}

				if(Franchise == Franchise.All || FranchiseProperties == null)
					return props;

				switch(Franchise)
				{
					case Franchise.HSConstructed:
						foreach(var property in FranchiseProperties.HearthstoneExtraData!)
							if(Helper.TryGetAttribute<MixpanelPropertyAttribute>(property.Key, out var cAttr) && cAttr?.Name != null)
								props.Add(cAttr.Name, property.Value);
						props.Add(
							"hdt_hsconstructed_settings_enabled",
							FranchiseProperties.HearthstoneSettingsEnabled!.Select(x => GetMixpanelPropertyName(x)).ToArray()
						);
						props.Add(
							"hdt_hsconstructed_settings_disabled",
							FranchiseProperties.HearthstoneSettingsDisabled!.Select(x => GetMixpanelPropertyName(x)).ToArray()
						);
						break;
					case Franchise.Battlegrounds:
						foreach(var property in FranchiseProperties.BattlegroundsExtraData!)
							if(Helper.TryGetAttribute<MixpanelPropertyAttribute>(property.Key, out var cAttr) && cAttr?.Name != null)
								props.Add(cAttr.Name, property.Value);
						props.Add(
							"hdt_battlegrounds_settings_enabled",
							FranchiseProperties.BattlegroundsSettingsEnabled!.Select(x => GetMixpanelPropertyName(x)).ToArray()
						);
						props.Add(
							"hdt_battlegrounds_settings_disabled",
							FranchiseProperties.BattlegroundsSettingsDisabled!.Select(x => GetMixpanelPropertyName(x)).ToArray()
						);
						// Dropping some properties, but why?
						props.Remove(GetMixpanelPropertyName(BattlegroundsSettings.Tier7HeroOverlay)!);
						props.Remove(GetMixpanelPropertyName(BattlegroundsSettings.Tier7QuestOverlay)!);
						break;
					case Franchise.Mercenaries:
						foreach(var property in FranchiseProperties.MercenariesExtraData!)
							if(Helper.TryGetAttribute<MixpanelPropertyAttribute>(property.Key, out var cAttr) && cAttr?.Name != null)
								props.Add(cAttr.Name, property.Value);
						props.Add(
							"hdt_mercenaries_settings_enabled",
							FranchiseProperties.MercenariesSettingsEnabled!.Select(x => GetMixpanelPropertyName(x)).ToArray()
						);
						props.Add(
							"hdt_mercenaries_settings_disabled",
							FranchiseProperties.MercenariesSettingsDisabled!.Select(x => GetMixpanelPropertyName(x)).ToArray()
						);
						break;
				}

				return props;
			}
		}

		public List<ValueMoment> GetValueMoments()
		{
			var valueMoments = ValueMomentManager.GetValueMoments(this).ToList();
			foreach(var valueMoment in valueMoments)
				DailyEventsCount.Instance.UpdateEventDailyCount(valueMoment.Name);

			return valueMoments;
		}

		private string GetEventId()
		{
			Properties.TryGetValue("franchise", out var franchise);
			Properties.TryGetValue("sub_franchise", out var subFranchise);
			var id = ActionName;

			if(franchise != null)
			{
				var firstFranchise = ((Franchise[])franchise)[0];
				id += $"_{GetMixpanelPropertyName(firstFranchise)?.ToLower()}";
			}

			if(subFranchise != null)
			{
				var subFranchiseArray = (string[])subFranchise;
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
