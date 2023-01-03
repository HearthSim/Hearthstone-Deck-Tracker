
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum Franchise {
		[MixpanelProperty("All")]
		All,

		[MixpanelProperty("HS-Constructed")]
		HSConstructed,

		[MixpanelProperty("Battlegrounds")]
		Battlegrounds,

		[MixpanelProperty("Mercenaries")]
		Mercenaries,
	}
}
