using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public class ArenaPickSingleHeroPowerOptionViewModel : ViewModel
{
	private static readonly List<string> Tiers = new() { "A", "B", "C", "D" };
	private const int HorizontalMargin = 47;

	public ArenaPickSingleHeroPowerOptionViewModel(ArenaHeroPickApiResponse.ResponseData? data, bool isUnderground, int position)
	{
		Data = data;
		IsUnderground = isUnderground;
		HasStats = data != null;
		Winrate = data?.Winrate ?? 0;
		Pickrate = data?.PickRate ?? 0;
		Margin = new Thickness(position != 2 ? 0 : HorizontalMargin, 630, position != 0 ? 0 : HorizontalMargin, 0);

		var plaqueMargin =  new Thickness(position != 2 ? 0 : HorizontalMargin, 560, position != 0 ? 0 : HorizontalMargin, 0);
		var tier = data?.Tier?.ToUpperInvariant() ?? "-";
		var plaqueLevel = (int)MathUtil.Clamp(5 - Tiers.IndexOf(tier), 1, 5);
		PlaqueViewModel = new ArenaPlaqueViewModel(tier, plaqueLevel, data?.DeckClass ?? 0, isUnderground, plaqueMargin);
	}

	// Loading State
	public ArenaPickSingleHeroPowerOptionViewModel(bool isUnderground, int position)
	{
		IsUnderground = isUnderground;
		Margin = new Thickness(position != 2 ? 0 : HorizontalMargin, 630, position != 0 ? 0 : HorizontalMargin, 0);

		var plaqueMargin =  new Thickness(position != 2 ? 0 : HorizontalMargin, 560, position != 0 ? 0 : HorizontalMargin, 0);
		PlaqueViewModel = new ArenaPlaqueViewModel("", 0, 0, isUnderground, plaqueMargin);
	}

	public ArenaHeroPickApiResponse.ResponseData? Data { get; }

	public bool IsUnderground { get; }

	public bool HasStats { get; }
	public double Winrate { get; }
	public double Pickrate { get; }
	public Thickness Margin { get; }

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
