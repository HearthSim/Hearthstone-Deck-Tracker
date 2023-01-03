
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum Franchise {
		[MixpanelProperty("HS-Constructed")]
		HSConstructed,

		[MixpanelProperty("Battlegrounds")]
		Battlegrounds,

		[MixpanelProperty("Mercenaries")]
		Mercenaries,
	}
}
