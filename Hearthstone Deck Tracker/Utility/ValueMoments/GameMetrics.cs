
namespace Hearthstone_Deck_Tracker.Utility.ValueMoments
{
	public class GameMetrics
	{
		public int BattlegroundsMinionsTabClicks { get; private set; }
		public int MercenariesHoversOpponentMercToShowAbility { get; private set; }
		public int MercenariesHoverTasksDuringMatch { get; private set; }

		public void IncrementBattlegroundsMinionsTabClick()
		{
			BattlegroundsMinionsTabClicks += 1;
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
