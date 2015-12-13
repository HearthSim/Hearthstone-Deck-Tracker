namespace Hearthstone_Deck_Tracker.Enums.Hearthstone
{
	public enum TAG_PLAYSTATE
	{
		INVALID,
		PLAYING,
		WINNING,
		LOSING,
		WON,
		LOST,
		TIED,
		DISCONNECTED,
		CONCEDED,

		//Renamed
		QUIT = CONCEDED
	}
}