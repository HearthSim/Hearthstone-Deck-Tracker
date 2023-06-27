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
		[LocDescription("Enum_DeckType_Twist", true)]
		Twist,
		[LocDescription("Enum_DeckType_Dungeon", true)]
		Dungeon,
		[LocDescription("Enum_DeckType_Duels", true)]
		Duels,
		[LocDescription("Enum_DeckType_Brawl", true)]
		Brawl,

		// Unused
		[LocDescription("Enum_DeckType_Classic", true)]
		Classic,
	}
}
