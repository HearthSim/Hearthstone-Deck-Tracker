using System;
using System.Collections.Generic;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public class ArenaPickSingleHeroOptionViewModel : ViewModel
{
	private static readonly List<string> Tiers = new() { "A", "B", "C", "D" };

	public ArenaPickSingleHeroOptionViewModel(ArenaHeroPickApiResponse.ResponseData? data, bool isUnderground)
	{
		Data = data;
		IsUnderground = isUnderground;
		HasStats = data != null;
		Winrate = data?.Winrate ?? 0;
		Pickrate = data?.PickRate ?? 0;

		var tier = data?.Tier?.ToUpperInvariant() ?? "-";
		var plaqueLevel = (int)MathUtil.Clamp(5 - Tiers.IndexOf(tier), 1, 5);
		PlaqueViewModel = new ArenaPlaqueViewModel(tier, plaqueLevel, data?.DeckClass ?? 0, isUnderground);
	}

	// Loading State
	public ArenaPickSingleHeroOptionViewModel(bool isUnderground)
	{
		IsUnderground = isUnderground;
		PlaqueViewModel = new ArenaPlaqueViewModel("", 0, 0, isUnderground);
	}

	public ArenaHeroPickApiResponse.ResponseData? Data { get; }

	public bool IsUnderground { get; }

	public bool HasStats { get; }
	public double Winrate { get; }
	public double Pickrate { get; }

	private static readonly SolidColorBrush NormalBorderColor = Helper.BrushFromHex("#067F93")!;
	private static readonly SolidColorBrush UndergroundBorderColor = Helper.BrushFromHex("#932020")!;
	public SolidColorBrush BadgeBorderColor => IsUnderground ? UndergroundBorderColor : NormalBorderColor;

	private static readonly SolidColorBrush NormalForegroundColor = Helper.BrushFromHex("#168Fa3")!;

	// This is intentionally different from ArenaPickSingleCardOption!
	// The stats here are less solid than the icons below the cards, and need a brighter red.
	private static readonly SolidColorBrush UndergroundForegroundColor = Helper.BrushFromHex("#DC4343")!;

	public SolidColorBrush BadgeForegroundColor => IsUnderground ? UndergroundForegroundColor : NormalForegroundColor;

	public ArenaPlaqueViewModel PlaqueViewModel { get; }
}
