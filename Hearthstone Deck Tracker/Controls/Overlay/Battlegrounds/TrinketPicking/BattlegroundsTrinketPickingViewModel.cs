using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Responses;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.TrinketPicking;

public class BattlegroundsTrinketPickingViewModel : ViewModel
{
	public bool ChoicesVisible
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
			UpdateMetrics();
		}
	}

	public Visibility Visibility => ChoicesVisible && TrinketStats != null ? Visible : Collapsed;

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
		? LocUtil.Get("BattlegroundsTrinketPicking_VisibilityToggle_Hide")
		: LocUtil.Get("BattlegroundsTrinketPicking_VisibilityToggle_Show");

	public List<StatsHeaderViewModel>? TrinketStats
	{
		get => GetProp<List<StatsHeaderViewModel>?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
			UpdateMetrics();
		}
	}

	public OverlayMessageViewModel Message { get; } = new();

	public void Reset()
	{
		TrinketStats = null;
		Message.Clear();
	}

	public double Scaling { get => GetProp(1.0); set => SetProp(value); }

	public void SetTrinketStats(
		IEnumerable<BattlegroundsTrinketPickStats.BattlegroundsSingleTrinketPickStats> stats
	)
	{
		TrinketStats = stats.Select(x => new StatsHeaderViewModel(x.Tier, x.AvgPlacement, x.PickRate)).ToList();
		StatsVisibility = Config.Instance.AutoShowBattlegroundsTrinketPicking ? Visible : Collapsed;
	}

	private void UpdateMetrics()
	{
		if(TrinketStats != null && Visibility == Visible && StatsVisibility == Visible)
			Core.Game.Metrics.Tier7TrinketOverlayDisplayed = true;
	}
}
