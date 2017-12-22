using System;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckEditor.Enums
{
	[Flags]
	public enum DeckEditorErrors
	{
		[LocDescription("Enum_DeckEditorErrors_NameRequired")]
		NameRequired = 1
	}
}
