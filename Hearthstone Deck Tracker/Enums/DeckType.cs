namespace Hearthstone_Deck_Tracker.Enums
{
	public enum DeckType
	{
		[LocDescription("Enum_DeckType_All")]
		All,
		[LocDescription("Enum_DeckType_Arena")]
		Arena,
		[LocDescription("Enum_DeckType_Standard")]
		Standard,
		[LocDescription("Enum_DeckType_Wild")]
		Wild,

		//obsolete - to avoid breaking things when loading this from config
		Constructed = Wild
	}
}
