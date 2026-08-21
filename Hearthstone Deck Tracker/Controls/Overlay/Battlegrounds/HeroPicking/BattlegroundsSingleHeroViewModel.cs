using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking
{
	public class BattlegroundsSingleHeroViewModel : ViewModel
	{
		public BattlegroundsHeroHeaderViewModel BgsHeroHeaderVM { get; }

		public int? HeroDbfId { get; }

		public BattlegroundsSingleHeroViewModel(BattlegroundsHeroPickStats.BattlegroundsSingleHeroPickStats? stats, Action<bool> onPlacementHover)
		{
			HeroDbfId = stats?.HeroDbfId;
			BgsHeroHeaderVM = new(stats?.Tier, stats?.AvgPlacement, stats?.PickRate, stats?.PlacementDistribution ?? Enumerable.Repeat(0.0, 8).ToArray(), onPlacementHover);
		}
	}
}
