using System;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking
{
	public class BattlegroundsHeroHeaderViewModel : StatsHeaderViewModel
	{
		public double[]? PlacementDistribution { get; }
		public Action<bool>? OnPlacementHover { get; }

		public BattlegroundsHeroHeaderViewModel(int? tier, double? avgPlacement, double? pickRate, double[]? placementDistribution, Action<bool> onPlacementHover) : base(tier, avgPlacement, pickRate)
		{
			PlacementDistribution = placementDistribution;
			OnPlacementHover = onPlacementHover;
		}

		public string TierTooltipTitle => Tier switch
		{
			(>= 1) and (<= 4) => LocUtil.Get($"BattlegroundsHeroPicking_Header_Tier{Tier}Tooltip_Title"),
			_ => "",
		};

		public string TierTooltipText => Tier switch
		{
			(>= 1) and (<= 4) => LocUtil.Get($"BattlegroundsHeroPicking_Header_Tier{Tier}Tooltip_Desc"),
			_ => "",
		};

		public Visibility PlacementDistributionVisibility { get => GetProp(Visibility.Collapsed); set => SetProp(value); }
	}
}
