using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Plugins;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public abstract class VMAction
	{
		/**
		 * if maxDailyOccurrences is null, this action is not sent to the event counter and will always be sent to Mixpanel
		 */
		protected VMAction(
			string name, ActionSource source, string type, Franchise franchise, SubFranchise[]? subFranchise,
			int? maxDailyOccurrences, bool withPersonalStatsSettings = false
		)
		{
			Name = name;
			Source = source;
			Type = type;
			Franchise = franchise;
			SubFranchise = subFranchise;
			GeneralSettings = new GeneralSettings();
			PersonalStatsSettings = withPersonalStatsSettings ? new PersonalStatsSettings() : null;

			var rect = Helper.GetHearthstoneMonitorRect();
			ScreenHeight = rect.Height;
			ScreenWidth = rect.Width;

			if(maxDailyOccurrences != null)
			{
				var curEventDailyCount = DailyEventsCount.Instance.GetEventDailyCount(Id);
				var newCurrentDailyCount = DailyEventsCount.Instance.UpdateEventDailyCount(Id);
				var eventCounterWasReset = curEventDailyCount > 0 && newCurrentDailyCount == 1;

				CurrentDailyOccurrences = newCurrentDailyCount;
				MaximumDailyOccurrences = maxDailyOccurrences;
				if(eventCounterWasReset)
					PreviousDailyOccurrences = curEventDailyCount;
			}
		}

		private string? _id;
		[JsonIgnore]
		public string Id
		{
			get
			{
				if(_id == null)
				{
					var franchise = Franchise == Franchise.All ? Franchise.HSConstructed : Franchise;
					Helper.TryGetAttribute<JsonPropertyAttribute>(franchise, out var attr);
					var id = $"{Name}_{attr?.PropertyName}";

					if(SubFranchise?.Length > 0 && Helper.TryGetAttribute(SubFranchise[0], out attr))
						id += $"_{attr?.PropertyName}";

					_id = id.ToLower();
				}

				return _id;
			}
		}

		[JsonIgnore]
		public string Name { get; }

		[JsonProperty("action_source")]
		[JsonConverter(typeof(EnumJsonConverter))]
		public ActionSource Source { get; }

		[JsonProperty("action_type")]
		public string Type { get; }

		[JsonProperty("domain")]
		protected string Domain { get => "hsreplay.net"; }

		[JsonProperty("franchise")]
		[JsonConverter(typeof(FranchiseJsonConverter))]
		public Franchise Franchise { get; }
		
		[JsonProperty("sub_franchise", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(EnumJsonConverter))]
		public SubFranchise[]? SubFranchise { get; }

		[JsonProperty("is_authenticated")]
		public bool IsAuthenticated { get => HSReplayNetOAuth.IsFullyAuthenticated; }

		[JsonProperty("screen_height")]
		public int ScreenHeight { get; }

		[JsonProperty("screen_width")]
		public int ScreenWidth { get; }

		[JsonProperty("card_language")]
		public string CardLanguage
		{
			get => Config.Instance.SelectedLanguage.Substring(0, 2);
		}

		[JsonProperty("appearance_language")]
		public string AppearanceLanguage
		{
			get => Config.Instance.Localization.ToString().Substring(0, 2);
		}

		[JsonProperty("hdt_plugins")]
		public string?[] HDTPlugins
		{
			get => PluginManager.Instance.Plugins.Where(x => x.IsEnabled).Select(x => x.Name).ToArray();
		}


		[JsonIgnore]
		public GeneralSettings GeneralSettings { get; }

		[JsonProperty("hdt_general_settings_enabled")]
		[JsonConverter(typeof(VMEnabledSettingsJsonConverter))]
		protected GeneralSettings GeneralSettingsEnabled { get => GeneralSettings; }

		[JsonProperty("hdt_general_settings_disabled")]
		[JsonConverter(typeof(VMDisabledSettingsJsonConverter))]
		protected GeneralSettings GeneralSettingsDisabled { get => GeneralSettings; }


		[JsonIgnore]
		public PersonalStatsSettings? PersonalStatsSettings { get; }

		[JsonProperty("hdt_personal_stats_settings_enabled", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(VMEnabledSettingsJsonConverter))]
		protected PersonalStatsSettings? PersonalStatsSettingsEnabled { get => PersonalStatsSettings; }

		[JsonProperty("hdt_personal_stats_settings_disabled", NullValueHandling = NullValueHandling.Ignore)]
		[JsonConverter(typeof(VMDisabledSettingsJsonConverter))]
		protected PersonalStatsSettings? PersonalStatsSettingsDisabled { get => PersonalStatsSettings; }


		[JsonProperty("cur_daily_occurrences", NullValueHandling = NullValueHandling.Ignore)]
		public int? CurrentDailyOccurrences { get; }

		[JsonProperty("max_daily_occurrences", NullValueHandling = NullValueHandling.Ignore)]
		public int? MaximumDailyOccurrences { get; }

		[JsonProperty("prev_daily_occurrences", NullValueHandling = NullValueHandling.Ignore)]
		public int? PreviousDailyOccurrences { get; }


		private List<ValueMoment>? _valueMoments;
		[JsonIgnore]
		public List<ValueMoment> ValueMoments
		{
			get
			{
				if(_valueMoments == null)
				{
					_valueMoments = ValueMomentManager.GetValueMoments(this).ToList();
					foreach(var valueMoment in _valueMoments)
						DailyEventsCount.Instance.UpdateEventDailyCount(valueMoment.Name);
				}
				return _valueMoments;
			}
		}

		[JsonProperty("free_value_moments", NullValueHandling = NullValueHandling.Ignore)]
		public string[] FreeValueMoments
		{
			get => ValueMoments.Where(vm => vm.IsFree).Select(vm => vm.Name).ToArray();
		}

		[JsonProperty("paid_value_moments", NullValueHandling = NullValueHandling.Ignore)]
		public string[] PaidValueMoments
		{
			get => ValueMoments.Where(vm => vm.IsPaid).Select(vm => vm.Name).ToArray();
		}

		[JsonProperty("has_free_value_moment", NullValueHandling = NullValueHandling.Ignore)]
		public bool HasFreeValueMoment { get => ValueMoments.Exists(vm => vm.IsFree); }

		[JsonProperty("has_paid_value_moment", NullValueHandling = NullValueHandling.Ignore)]
		public bool HasPaidValueMoment { get => ValueMoments.Exists(vm => vm.IsPaid); }
	}
}
