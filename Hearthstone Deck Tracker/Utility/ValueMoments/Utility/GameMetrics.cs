namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility
{
	public class GameMetrics
	{
		public bool ConstructedMulliganGuideOverlayDisplayed { get; set; }
		public int BattlegroundsMinionTiersClicks { get; private set; }
		public int BattlegroundsMinionsByMinionTypeFilterClicks { get; private set; }
		public int BattlegroundsMinionsInspirationClicks { get; set; }
		public int BattlegroundsInspirationToggleClicks { get; set; }
		public int BattlegroundsInspirationMinionClicks { get; set; }
		public int BattlegroundsCompGuidesClicks { get; set; }
		public int BattlegroundsCompGuidesMinionHovers { get; set; }
		public int BattlegroundsCardsTabClicks { get; set; }
		public int BattlegroundsCompsTabClicks { get; set; }
		public int BattlegroundsHeroesTabClicks { get; set; }
		public int BattlegroundsCompGuidesInspirationClicks { get; set; }
		public int BobsBuddyTerminalCases { get; set; }
		public int MercenariesHoversOpponentMercToShowAbility { get; private set; }
		public int MercenariesHoverTasksDuringMatch { get; private set; }
		public bool Tier7HeroOverlayDisplayed { get; set; }
		public bool Tier7QuestOverlayDisplayed { get; set; }
		public bool Tier7TrinketOverlayDisplayed { get; set; }
		public bool Tier7TrialActivated { get; set; }
		public int? Tier7TrialsRemaining { get; set; }
		public bool ArenaTrialActivated { get; set; }
		public int? ArenaTrialsRemaining { get; set; }
		public int BattlegroundsBrowserTypeFilterClicks { get; set; }
		public int BattlegroundsBrowserMechanicFilterClicks { get; set; }
		public int BattlegroundsBrowserOpenFilterPanelClicks { get; set; }
		public bool ChinaModuleEnabled { get; set; }

		public bool? IsBattlegroundsChineseEnvironmentCorrect { get; set; }

		public int BattlegroundsChinaModuleActionSuccessCount { get; set; }

		public int BattlegroundsChinaModuleActionClicks { get; set; }

		public int BattlegroundsChinaModuleAutoActionClicks { get; set; }

		public bool BattlegroundsChinaModuleAutoActionEnabled { get; set; }


		public void IncrementBattlegroundsMinionsTiersClick()
		{
			BattlegroundsMinionTiersClicks += 1;
		}

		public void IncrementBattlegroundsMinionsByMinionTypeClick()
		{
			BattlegroundsMinionsByMinionTypeFilterClicks += 1;
		}

		public void IncrementBobsBuddyTerminalCase()
		{
			BobsBuddyTerminalCases += 1;
		}

		public void IncrementMercenariesHoversOpponentMercToShowAbility()
		{
			MercenariesHoversOpponentMercToShowAbility += 1;
		}

		public void IncrementMercenariesTaskHoverDuringMatch()
		{
			MercenariesHoverTasksDuringMatch += 1;
		}
	}
}
