using System;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckEditor.Enums
{
	[Flags]
	public enum DeckEditorWarnings
	{
		[LocDescription("Enum_DeckEditorWarnings_LessThan30")]
		LessThan30Cards = 1,
		[LocDescription("Enum_DeckEditorWarnings_MoreThan30")]
		MoreThan30Cards = 1 << 1,
		[LocDescription("Enum_DeckEditorWarnings_NameAlreadyExists")]
		NameAlreadyExists = 1 << 2,
		[LocDescription("Enum_DeckEditorWarnings_LessThan40")]
		LessThan40Cards = 1 << 3,
		[LocDescription("Enum_DeckEditorWarnings_MoreThan40")]
		MoreThan40Cards = 1 << 4,
	}
}
