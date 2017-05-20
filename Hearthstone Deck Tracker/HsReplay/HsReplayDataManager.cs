using Hearthstone_Deck_Tracker.HsReplay.Data;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	public static class HsReplayDataManager
	{
		public static HsReplayDecks Decks { get; } = new HsReplayDecks();
		public static HsReplayWinrates Winrates { get; } = new HsReplayWinrates();
	}
}
