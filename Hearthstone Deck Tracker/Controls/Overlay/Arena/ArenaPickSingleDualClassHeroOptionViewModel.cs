using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public class ArenaPickSingleDualClassHeroOptionViewModel : ArenaPickSingleHeroOptionViewModel
{
	private const int HorizontalMargin = 55;

	public ArenaPickSingleDualClassHeroOptionViewModel(ArenaHeroPickApiResponse.ResponseData? data, bool isUnderground, int position)
		: base(data, isUnderground, MarginForPosition(position, 170))
	{
		Margin = MarginForPosition(position, 428);
	}

	// Loading State
	public ArenaPickSingleDualClassHeroOptionViewModel(bool isUnderground, int position) : base(isUnderground, MarginForPosition(position, 560))
	{
		Margin = MarginForPosition(position, 630);
	}

	public Thickness Margin { get; }

	private static Thickness MarginForPosition(int position, double top) =>
		new(position != 2 ? 0 : HorizontalMargin, top, position != 0 ? 0 : HorizontalMargin, 0);
}
