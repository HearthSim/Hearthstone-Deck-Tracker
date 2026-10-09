using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Responses;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPowerPicking;

public class BattlegroundsHeroPowerPickingViewModel : ViewModel
{
	public bool ChoicesVisible
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
		}
	}

	public bool IsViewingTeammate
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
		}
	}

	public Visibility Visibility => ChoicesVisible && !IsViewingTeammate && HeroPowerStats != null ? Visible : Collapsed;

	public Visibility StatsVisibility
	{
		get => GetProp(Collapsed);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(VisibilityToggleIcon));
			OnPropertyChanged(nameof(VisibilityToggleText));
		}
	}

	public Visual? VisibilityToggleIcon =>
		Application.Current.TryFindResource(StatsVisibility == Visible ? "eye_slash" : "eye") as Visual;

	[LocalizedProp]
	public string VisibilityToggleText => StatsVisibility == Visible
		? LocUtil.Get("BattlegroundsHeroPicking_VisibilityToggle_Hide")
		: LocUtil.Get("BattlegroundsHeroPicking_VisibilityToggle_Show");

	public List<BattlegroundsSingleHeroPowerViewModel>? HeroPowerStats
	{
		get => GetProp<List<BattlegroundsSingleHeroPowerViewModel>?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
		}
	}

	public OverlayMessageViewModel Message { get; } = new();

	public void ShowErrorMessage() => Message.Error();
	public void ShowDisabledMessage() => Message.Disabled();

	public void Reset()
	{
		HeroPowerStats = null;
		IsViewingTeammate = false;
		Message.Clear();
	}

	public double Scaling { get => GetProp(1.0); set => SetProp(value); }

	public void SetHeroPowerStats(
		IEnumerable<int> heroPowerDbfIds,
		BattlegroundsHeroPickStats.BattlegroundsSingleHeroPickStats[] stats,
		Dictionary<string, string>? parameters,
		int? minMmr,
		bool anomalyAdjusted
	)
	{
		HeroPowerStats = heroPowerDbfIds.Select(dbfId => new BattlegroundsSingleHeroPowerViewModel(dbfId, stats.FirstOrDefault(x => x.HeroDbfId == dbfId), SetPlacementVisible)).ToList();
		var filterValue = parameters != null && parameters.TryGetValue("mmrPercentile", out var x) ? x : null;

		Message.Mmr(filterValue, minMmr, anomalyAdjusted);

		StatsVisibility = Config.Instance.ShowBattlegroundsHeroPicking ? Visible : Collapsed;
	}

	public void SetPlacementVisible(bool isVisible)
	{
		if(HeroPowerStats == null)
			return;
		var visibility = isVisible ? Visible : Collapsed;
		foreach(var heroPower in HeroPowerStats)
			heroPower.BgsHeroHeaderVM.PlacementDistributionVisibility = visibility;
	}
}
