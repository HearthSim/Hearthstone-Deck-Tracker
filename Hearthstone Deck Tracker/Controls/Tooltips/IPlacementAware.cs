using System.Windows.Controls.Primitives;

namespace Hearthstone_Deck_Tracker.Controls.Tooltips;

public interface IPlacementAware
{
	void SetPlacement(PlacementMode placement);
}
