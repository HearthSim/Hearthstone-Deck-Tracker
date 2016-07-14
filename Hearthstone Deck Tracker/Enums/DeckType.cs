namespace Hearthstone_Deck_Tracker.Enums
{
	public enum DeckType
	{
		All,
		Arena,
		Standard,
		Wild,

		//obsolete - to avoid breaking things when loading this from config
		Constructed = Wild
	}
}