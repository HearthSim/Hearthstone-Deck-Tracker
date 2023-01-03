
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums
{
	public enum HearthstoneSettings
	{
		[MixpanelProperty("hide_decks")]
		HideDecks,

		[MixpanelProperty("hide_timers")]
		HideTimers,
	}
}
