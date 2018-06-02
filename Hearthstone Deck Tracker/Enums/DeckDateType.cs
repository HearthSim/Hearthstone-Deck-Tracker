using System.ComponentModel;

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum DeckDateType
	{
		[LocDescription("Enum_None")]
		None,
		[LocDescription("Enum_Last_Played")]
		LastPlayed,
		[LocDescription("Enum_Last_Edited")]
		LastEdited
	}
}
