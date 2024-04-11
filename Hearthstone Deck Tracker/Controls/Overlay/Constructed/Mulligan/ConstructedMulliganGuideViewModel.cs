using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan;

public class ConstructedMulliganGuideViewModel : ViewModel
{
	public Visibility Visibility
	{
		get => GetProp(Collapsed);
		set
		{
			SetProp(value);
			UpdateMetrics();
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
		? LocUtil.Get("ConstructedMulliganGuide_VisibilityToggle_Hide")
		: LocUtil.Get("ConstructedMulliganGuide_VisibilityToggle_Show");

	public List<ConstructedMulliganSingleCardViewModel>? CardStats
	{
		get => GetProp<List<ConstructedMulliganSingleCardViewModel>?>(null);
		set => SetProp(value);
	}

	public OverlayMessageViewModel Message { get; } = new();

	public void Reset()
	{
		CardStats = null;
		Visibility = Collapsed;
		StatsVisibility = Collapsed;
		Message.Clear();
	}

	public double Scaling { get => GetProp(1.0); set => SetProp(value); }

	public void SetMulliganData(IEnumerable<SingleCardStats>? stats, int? maxRank, Dictionary<string, string>? selectedParams)
	{
		CardStats = stats?.Select(x => new ConstructedMulliganSingleCardViewModel(x, maxRank)).ToList();

		if(selectedParams != null) // keep existing text around otherwise until cleared by .Reset()
		{
			CardClass? opponentClass = selectedParams.TryGetValue("opponent_class", out var opponentClassString)
				&& Enum.TryParse(opponentClassString, out CardClass opp)
				? opp : null;

			OverlayMessageViewModel.PlayerInitiative? initiative = selectedParams.TryGetValue("PlayerInitiative", out var initiativeString)
				&& Enum.TryParse(initiativeString, out OverlayMessageViewModel.PlayerInitiative init)
				? init : null;

			if(opponentClass.HasValue && initiative.HasValue)
			{
				Message.Scope(opponentClass.Value, initiative.Value);
			}
		}

		Visibility = Visible;
		StatsVisibility = Config.Instance.AutoShowMulliganGuide ? Visible : Collapsed;

		UpdateMetrics();
	}

	private void UpdateMetrics()
	{
		if(Visibility == Visible && StatsVisibility == Visible)
			Core.Game.Metrics.ConstructedMulliganGuideOverlayDisplayed = true;
	}
}
