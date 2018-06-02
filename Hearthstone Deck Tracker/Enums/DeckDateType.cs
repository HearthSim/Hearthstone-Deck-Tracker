using System.ComponentModel;

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum DeckDateType
	{
		[LocDescription("Enum_DeckDateType_None")]
		None,
		[LocDescription("Enum_DeckDateType_LastPlayed")]
		LastPlayed,
		[LocDescription("Enum_DeckDateType_LastEdited")]
		LastEdited
	}
}
