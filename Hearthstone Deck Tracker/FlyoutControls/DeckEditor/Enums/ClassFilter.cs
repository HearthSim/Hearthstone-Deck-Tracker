using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckEditor.Enums
{
	public enum ClassFilter
	{
		[LocDescription("MainWindow_DeckBuilder_Filter_Type_All")]
		All,
		[LocDescription("MainWindow_DeckBuilder_Filter_Type_Class")]
		ClassOnly,
		[LocDescription("MainWindow_DeckBuilder_Filter_Type_Neutral")]
		NeutralOnly
	}
}
