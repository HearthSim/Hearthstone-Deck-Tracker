using System.ComponentModel;
using Hearthstone_Deck_Tracker.Enums;

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckEditor.Enums
{
	public enum CostFilter
	{
		[LocDescription("MainWindow_DeckBuilder_Filter_Cost_All")]
		All = -1,
		[Description("0")]
		Zero = 0,
		[Description("1")]
		One = 1,
		[Description("2")]
		Two = 2,
		[Description("3")]
		Three = 3,
		[Description("4")]
		Four = 4,
		[Description("5")]
		Five = 5,
		[Description("6")]
		Six = 6,
		[Description("7")]
		Seven = 7,
		[Description("8")]
		Eight = 8,
		[Description("9+")]
		NinePlus = 9
	}
}
