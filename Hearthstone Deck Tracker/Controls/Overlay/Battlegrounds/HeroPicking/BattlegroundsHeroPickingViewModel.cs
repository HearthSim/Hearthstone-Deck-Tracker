using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Responses;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking
{
	public class BattlegroundsHeroPickingViewModel : ViewModel
	{
		public bool IsViewingTeammate
		{
			get => GetProp(false);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(Visibility));
				UpdateMetrics();
			}
		}

		public Visibility Visibility
		{
			get
			{
				if(IsViewingTeammate)
					return Collapsed;
				return HeroStats != null ? Visible : Collapsed;
			}
		}

		public Visibility StatsVisibility
		{
			get => GetProp(Collapsed);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(VisibilityToggleIcon));
				OnPropertyChanged(nameof(VisibilityToggleText));
				UpdateMetrics();
			}
		}

		public Visual? VisibilityToggleIcon =>
			Application.Current.TryFindResource(StatsVisibility == Visible ? "eye_slash" : "eye") as Visual;
		public string VisibilityToggleText => StatsVisibility == Visible
			? LocUtil.Get("BattlegroundsHeroPicking_VisibilityToggle_Hide")
			: LocUtil.Get("BattlegroundsHeroPicking_VisibilityToggle_Show");

		public List<BattlegroundsSingleHeroViewModel>? HeroStats
		{
			get => GetProp<List<BattlegroundsSingleHeroViewModel>?>(null);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(Visibility));
				UpdateMetrics();
			}
		}

		public OverlayMessageViewModel Message { get; } = new();

		public void ShowErrorMessage() => Message.Error();
		public void ShowDisabledMessage() => Message.Disabled();

		public void Reset()
		{
			HeroStats = null;
			IsViewingTeammate = false;
			StatsVisibility = Collapsed;
			Message.Clear();
		}

		public double Scaling { get => GetProp(1.0); set => SetProp(value); }

		public void SetHeroStats(
			IEnumerable<BattlegroundsHeroPickStats.BattlegroundsSingleHeroPickStats> stats,
			Dictionary<string, string>? parameters,
			int? minMmr,
			bool anomalyAdjusted
		)
		{
			HeroStats = stats.Select(x => new BattlegroundsSingleHeroViewModel(x, SetPlacementVisible)).ToList();
			var filterValue = parameters != null && parameters.TryGetValue("mmrPercentile", out var x) ? x : null;

			Message.Mmr(filterValue, minMmr, anomalyAdjusted);

			StatsVisibility = Config.Instance.ShowBattlegroundsHeroPicking ? Visible : Collapsed;
		}

		public void InvalidateSingleHeroStats(int dbfId)
		{
			HeroStats = HeroStats?.Select(x => x.HeroDbfId == dbfId ? new BattlegroundsSingleHeroViewModel(null, SetPlacementVisible) : x).ToList();
		}

		private void UpdateMetrics()
		{
			if(HeroStats != null && Visibility == Visible && StatsVisibility == Visible)
				Core.Game.Metrics.Tier7HeroOverlayDisplayed = true;
		}

		public void SetPlacementVisible(bool isVisible)
		{
			if(HeroStats == null)
				return;
			var visibility = isVisible ? Visible : Collapsed;
			foreach(var hero in HeroStats)
				hero.BgsHeroHeaderVM.PlacementDistributionVisibility = visibility;
		}
	}
}
