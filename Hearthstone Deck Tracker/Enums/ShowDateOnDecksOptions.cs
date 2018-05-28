using System.ComponentModel;

namespace Hearthstone_Deck_Tracker.Enums
{
	public enum ShowDateOnDecksOptions
	{
		[Description("Don't Show Date")]
		show_no_date,
		[Description("Last Time Played")]
		show_last_played_date,
		[Description("Last Time Edited")]
		show_last_edited_date
	}
}
