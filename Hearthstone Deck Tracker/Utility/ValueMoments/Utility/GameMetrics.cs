namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility
{
	public class GameMetrics
	{
		public bool ConstructedMulliganGuideOverlayDisplayed { get; set; }
		public int BattlegroundsMinionsTabClicks { get; private set; }
		public int BattlegroundsMinionsByMinionTypeFilterClicks { get; private set; }
		public int BobsBuddyTerminalCases { get; set; }
		public int MercenariesHoversOpponentMercToShowAbility { get; private set; }
		public int MercenariesHoverTasksDuringMatch { get; private set; }
		public bool Tier7HeroOverlayDisplayed { get; set; }
		public bool Tier7QuestOverlayDisplayed { get; set; }
		public bool Tier7TrinketOverlayDisplayed { get; set; }
		public bool Tier7TrialActivated { get; set; }
		public int? Tier7TrialsRemaining { get; set; }

		public void IncrementBattlegroundsMinionsTabClick()
		{
			BattlegroundsMinionsTabClicks += 1;
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
