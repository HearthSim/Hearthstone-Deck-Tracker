using System;

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum DeckType
	{
		[LocDescription("Enum_DeckType_All", true)]
		All,
		[LocDescription("Enum_DeckType_Arena", true)]
		Arena,
		[LocDescription("Enum_DeckType_Standard", true)]
		Standard,
		[LocDescription("Enum_DeckType_Wild", true)]
		Wild,

		//obsolete - to avoid breaking things when loading this from config
		Constructed = Wild
	}
}
