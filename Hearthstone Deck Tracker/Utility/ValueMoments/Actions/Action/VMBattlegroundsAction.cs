using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using Newtonsoft.Json;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action
{
	public abstract class VMBattlegroundsAction : VMEndMatchAction
	{
		public const string Tier7OverlayTrial = "Tier7Overlay";

		protected VMBattlegroundsAction(
			Franchise franchise, int? maxDailyOccurrences,
			int heroDbfId, string heroName, int finalPlacement, int finalTurn, GameType gameType, int rating, GameMetrics gameMetrics
		) : base(franchise, null, maxDailyOccurrences)
		{
			HeroDbfId = heroDbfId;
			HeroName = heroName;
			FinalPlacement = finalPlacement;
			FinalTurn = finalTurn;
			GameType = gameType;
			Rating = rating;
			Tier7HeroOverlayDisplayed = gameMetrics.Tier7HeroOverlayDisplayed;
			Tier7QuestOverlayDisplayed = gameMetrics.Tier7QuestOverlayDisplayed;
			Tier7TrinketOverlayDisplayed = gameMetrics.Tier7TrinketOverlayDisplayed;
			NumClickBattlegroundsMinionTiers = gameMetrics.BattlegroundsMinionTiersClicks;
			NumClickBattlegroundsMinionsByMinionTypeFilter = gameMetrics.BattlegroundsMinionsByMinionTypeFilterClicks;
			NumClickBattlegroundsMinionsInspiration = gameMetrics.BattlegroundsMinionsInspirationClicks;
			NumClickBattlegroundsInspirationToggle = gameMetrics.BattlegroundsInspirationToggleClicks;
			NumClickBattlegroundsInspirationMinion = gameMetrics.BattlegroundsInspirationMinionClicks;
			NumClickBattlegroundsCompGuides = gameMetrics.BattlegroundsCompGuidesClicks;
			NumClickBattlegroundsCompGuidesMinionHovers = gameMetrics.BattlegroundsCompGuidesMinionHovers;
			NumClickBattlegroundsCompsTab = gameMetrics.BattlegroundsCompsTabClicks;
			NumClickBattlegroundsHeroesTab = gameMetrics.BattlegroundsHeroesTabClicks;
			NumClickBattlegroundsCardsTab = gameMetrics.BattlegroundsCardsTabClicks;
			NumClickBattlegroundsCompGuidesInspiration = gameMetrics.BattlegroundsCompGuidesInspirationClicks;
			NumClickBattlegroundsBrowserTypeFilter = gameMetrics.BattlegroundsBrowserTypeFilterClicks;
			NumClickBattlegroundsBrowserMechanicFilter = gameMetrics.BattlegroundsBrowserMechanicFilterClicks;
			NumClickBattlegroundsBrowserOpenFilterPanel = gameMetrics.BattlegroundsBrowserOpenFilterPanelClicks;
			NumBobsBuddyTerminalCases = gameMetrics.BobsBuddyTerminalCases;
			if (gameMetrics.Tier7TrialActivated)
				TrialsActivated = new[] { Tier7OverlayTrial };
			if (gameMetrics.Tier7TrialsRemaining.HasValue)
				TrialsRemaining = new[] { $"{Tier7OverlayTrial}:{gameMetrics.Tier7TrialsRemaining}" };

			if(gameMetrics.ChinaModuleEnabled)
			{
				NumClickBattlegroundsChinaModuleAction = gameMetrics.BattlegroundsChinaModuleActionClicks;
				CountBattlegroundsChinaModuleActionSuccess = gameMetrics.BattlegroundsChinaModuleActionSuccessCount;
				NumClickBattlegroundsChinaModuleAutoAction = gameMetrics.BattlegroundsChinaModuleAutoActionClicks;
				EnabledBattlegroundsChinaModuleAutoAction = gameMetrics.BattlegroundsChinaModuleAutoActionEnabled;
			}
			IsBattlegroundsChineseEnvironmentCorrect = gameMetrics.IsBattlegroundsChineseEnvironmentCorrect;

			TavernMarkersDisplayed = gameMetrics.TavernMarkersDisplayed;
			if(gameMetrics.TavernMarkersDisplayed)
			{
				TavernMarkersPinnedFromAnimatedCard = gameMetrics.TavernMarkersPinnedFromAnimatedCard;
				TavernMarkersPinnedFromCompGuide = gameMetrics.TavernMarkersPinnedFromCompGuide;
				TavernMarkersTribeToggled = gameMetrics.TavernMarkersTribeToggled;
				TavernMarkersRecommendedToggled = gameMetrics.TavernMarkersRecommendedToggled;
				TavernMarkersRecommendedDisabledTurn = gameMetrics.TavernMarkersRecommendedDisabledTurn;
				TavernMarkersRecommendedEnabled = gameMetrics.TavernMarkersRecommendedEnabled;
				TavernMarkersAutoEnableResponse = gameMetrics.TavernMarkersAutoEnableResponse;
				TavernMarkersPanelExpanded = Config.Instance.TavernMarkersPanelExpanded;

				// Value should be ignored if the user has dismissed the quick guides before
				if(Config.Instance.DismissedCompGuidesMarkerQuickGuide == gameMetrics.TavernMarkersCompGuidesQuickGuideDismissed)
					TavernMarkersCompGuidesQuickGuideDismissed = gameMetrics.TavernMarkersCompGuidesQuickGuideDismissed;

				if(Config.Instance.DismissedTavernMarkerQuickQuickGuide == gameMetrics.TavernMarkersQuickGuideDismissed)
					TavernMarkersQuickGuideDismissed = gameMetrics.TavernMarkersQuickGuideDismissed;
			}

			BattlegroundsSettings = new BattlegroundsSettings();
		}

		[JsonProperty("num_click_battlegrounds_browser_open_filter_panel")]
		public int NumClickBattlegroundsBrowserOpenFilterPanel { get; set; }

		[JsonProperty("num_click_battlegrounds_browser_mechanic_filter")]
		public int NumClickBattlegroundsBrowserMechanicFilter { get; set; }

		[JsonProperty("num_click_battlegrounds_browser_type_filter")]
		public int NumClickBattlegroundsBrowserTypeFilter { get; set; }

		[JsonProperty("hero_dbf_id")]
		public int HeroDbfId { get; }

		[JsonProperty("hero_name")]
		public string HeroName { get; }

		[JsonProperty("final_placement")]
		public int FinalPlacement { get; }

		[JsonProperty("final_turn")]
		public int FinalTurn { get; }

		[JsonProperty("game_type")]
		public GameType GameType { get; }

		[JsonProperty("battlegrounds_rating")]
		public int Rating { get; }

		[JsonIgnore]
		public bool Tier7HeroOverlayDisplayed { get; }

		[JsonIgnore]
		public bool Tier7QuestOverlayDisplayed { get; }

		[JsonIgnore]
		public bool Tier7TrinketOverlayDisplayed { get; }

		[JsonProperty("num_click_battlegrounds_minion_tiers")]
		public int NumClickBattlegroundsMinionTiers { get;  }

		[JsonProperty("num_click_battlegrounds_minions_by_minion_type_filter")]
		public int NumClickBattlegroundsMinionsByMinionTypeFilter { get;  }

		[JsonProperty("num_click_battlegrounds_minions_inspiration_button")]
		public int NumClickBattlegroundsMinionsInspiration { get; set; }

		[JsonProperty("num_click_battlegrounds_inspiration_overlay_toggle")]
		public int NumClickBattlegroundsInspirationToggle { get; set; }

		[JsonProperty("num_click_battlegrounds_inspiration_minion")]
		public int NumClickBattlegroundsInspirationMinion { get; set; }

		[JsonProperty("num_click_battlegrounds_comp_guides")]
		public int NumClickBattlegroundsCompGuides { get; set; }

		[JsonProperty("num_click_battlegrounds_comp_guides_minion_hovers")]
		public int NumClickBattlegroundsCompGuidesMinionHovers { get; set; }

		[JsonProperty("num_click_battlegrounds_comps_tab")]
		public int NumClickBattlegroundsCompsTab { get; set; }

		[JsonProperty("num_click_battlegrounds_heroes_tab")]
		public int NumClickBattlegroundsHeroesTab { get; set; }

		[JsonProperty("num_click_battlegrounds_cards_tab")]
		public int NumClickBattlegroundsCardsTab { get; set; }

		[JsonProperty("num_click_battlegrounds_comp_guides_inspiration")]
		public int NumClickBattlegroundsCompGuidesInspiration { get; set; }

		[JsonProperty("num_bobs_buddy_terminal_cases")]
		public int NumBobsBuddyTerminalCases { get;  }

		[JsonProperty("trials_activated", NullValueHandling = NullValueHandling.Ignore)]
		public string[]? TrialsActivated { get; }

		[JsonProperty("trials_remaining", NullValueHandling = NullValueHandling.Ignore)]
		public string[]? TrialsRemaining { get; }

		[JsonProperty("num_click_battlegrounds_china_module_action", NullValueHandling = NullValueHandling.Ignore)]
		public int? NumClickBattlegroundsChinaModuleAction { get; }

		[JsonProperty("count_battlegrounds_china_module_action_success", NullValueHandling = NullValueHandling.Ignore)]
		public int? CountBattlegroundsChinaModuleActionSuccess { get; }

		[JsonProperty("num_click_battlegrounds_china_module_auto_action", NullValueHandling = NullValueHandling.Ignore)]
		public int? NumClickBattlegroundsChinaModuleAutoAction { get; }

		[JsonProperty("enabled_battlegrounds_china_module_auto_action", NullValueHandling = NullValueHandling.Ignore)]
		public bool? EnabledBattlegroundsChinaModuleAutoAction { get; }

		[JsonProperty("is_battlegrounds_chinese_environment_correct", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsBattlegroundsChineseEnvironmentCorrect { get; }

		[JsonProperty("tavern_markers_displayed")]
		public bool TavernMarkersDisplayed { get; set; }

		[JsonProperty("tavern_markers_pinned_from_animated_card", NullValueHandling = NullValueHandling.Ignore)]
		public bool? TavernMarkersPinnedFromAnimatedCard { get; }

		[JsonProperty("tavern_markers_pinned_from_comp_guide", NullValueHandling = NullValueHandling.Ignore)]
		public bool? TavernMarkersPinnedFromCompGuide { get; }

		[JsonProperty("tavern_markers_tribe_toggled", NullValueHandling = NullValueHandling.Ignore)]
		public bool? TavernMarkersTribeToggled { get; }

		[JsonProperty("tavern_markers_recommended_toggled", NullValueHandling = NullValueHandling.Ignore)]
		public bool? TavernMarkersRecommendedToggled { get; }

		[JsonProperty("tavern_markers_recommended_disabled_turn", NullValueHandling = NullValueHandling.Ignore)]
		public int? TavernMarkersRecommendedDisabledTurn { get; }

		[JsonProperty("tavern_markers_recommended_enabled", NullValueHandling = NullValueHandling.Ignore)]
		public bool? TavernMarkersRecommendedEnabled { get; }

		[JsonProperty("tavern_markers_quick_guide_dismissed", NullValueHandling = NullValueHandling.Ignore)]
		public bool? TavernMarkersQuickGuideDismissed { get; }

		[JsonProperty("tavern_markers_comp_guides_quick_guide_dismissed", NullValueHandling = NullValueHandling.Ignore)]
		public bool? TavernMarkersCompGuidesQuickGuideDismissed { get; }

		[JsonProperty("tavern_markers_auto_enable_response", NullValueHandling = NullValueHandling.Ignore)]
		public string? TavernMarkersAutoEnableResponse { get; }

		[JsonProperty("tavern_markers_panel_expanded", NullValueHandling = NullValueHandling.Ignore)]
		public bool? TavernMarkersPanelExpanded { get; }

		[JsonIgnore]
		public BattlegroundsSettings BattlegroundsSettings { get; }

		[JsonProperty("hdt_battlegrounds_settings_enabled")]
		[JsonConverter(typeof(VMEnabledSettingsJsonConverter))]
		protected BattlegroundsSettings BattlegroundsSettingsEnabled { get => BattlegroundsSettings; }

		[JsonProperty("hdt_battlegrounds_settings_disabled")]
		[JsonConverter(typeof(VMDisabledSettingsJsonConverter))]
		protected BattlegroundsSettings BattlegroundsSettingsDisabled { get => BattlegroundsSettings; }
	}
}
